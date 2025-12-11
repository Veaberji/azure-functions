using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EDU.Func.Durable.Models;
using static EDU.Func.Durable.OrderProcessingConstants;

namespace EDU.Func.Durable.Functions;

/// <summary>
/// Durable Function orchestrator for Order Processing.
/// Implements the workflow: Validate → Approve (if needed) → Pay → Ship
/// with SAGA compensation on failure.
/// </summary>
public sealed class OrderOrchestrator(IConfiguration configuration)
{

    private decimal ApprovalThreshold 
        => configuration.GetValue(ApprovalThresholdKey, DefaultApprovalThreshold);

    private int ApprovalTimeoutMinutes 
        => configuration.GetValue(ApprovalTimeoutMinutesKey, DefaultApprovalTimeoutMinutes);

    private RetryPolicy PaymentRetryPolicy => new(
        maxNumberOfAttempts: configuration.GetValue("PaymentMaxRetries", 3),
        firstRetryInterval: TimeSpan.FromSeconds(configuration.GetValue("PaymentRetryIntervalSeconds", 2)),
        backoffCoefficient: configuration.GetValue("PaymentBackoffCoefficient", 2.0));


    /// <summary>
    /// Main orchestration implementing the order processing workflow.
    /// </summary>
    [Function(nameof(RunOrchestrator))]
    public async Task<OrderResult> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var log = context.CreateReplaySafeLogger(nameof(OrderOrchestrator));
        var order = context.GetInput<OrderRequest>()!;
        var steps = new List<string>();

        log.LogInformation("Processing Order {OrderId} for ${Amount}", order.OrderId, order.Amount);

        // Step 1: Validate
        var validationResult = await ProcessValidationStep(context, order, steps, log);
        if (validationResult is not null)
        {
            return validationResult;
        }

        // Step 2: Human approval for large orders
        if (order.Amount > ApprovalThreshold)
        {
            log.LogWarning("Order {OrderId} (${Amount}) exceeds threshold ${Threshold}, requiring approval",
                order.OrderId, order.Amount, ApprovalThreshold);

            var approvalResult = await HandleManagerApproval(context, order, ApprovalTimeoutMinutes, steps, log);
            if (!approvalResult.ShouldContinue)
            {
                return approvalResult.TerminalResult!;
            }
        }

        // Step 3: Process payment with retries
        bool paymentSuccess = false;
        var paymentResult = await ProcessPaymentStep(context, order, steps, log);
        if (paymentResult is not null)
        {
            return paymentResult;
        }
        paymentSuccess = true;

        // Step 4: Ship order with SAGA compensation
        var shippingResult = await ProcessShippingStep(context, order, steps, paymentSuccess, log);
        if (shippingResult is not null)
        {
            return shippingResult;
        }

        log.LogInformation("Order {OrderId} completed successfully", order.OrderId);
        return CreateResult(order.OrderId, OrderStatus.Completed, steps);
    }


    private static async Task<OrderResult?> ProcessValidationStep(
        TaskOrchestrationContext context,
        OrderRequest order,
        List<string> steps,
        ILogger log)
    {
        var validationResult = await context.CallActivityAsync<ValidationResult>(
            nameof(OrderActivities.ValidateOrder), order);

        if (!validationResult.IsValid)
        {
            log.LogWarning("Validation failed for Order {OrderId}: {Error}", order.OrderId, validationResult.ErrorMessage);
            return CreateResult(order.OrderId, OrderStatus.ValidationFailed, steps, validationResult.ErrorMessage);
        }

        steps.Add("✓ Validated order");
        return null;
    }

    private async Task<OrderResult?> ProcessPaymentStep(
        TaskOrchestrationContext context,
        OrderRequest order,
        List<string> steps,
        ILogger log)
    {
        try
        {
            steps.Add(await context.CallActivityAsync<string>(
                nameof(OrderActivities.ProcessPayment), order, new TaskOptions(PaymentRetryPolicy)));
            return null;
        }
        catch (TaskFailedException ex)
        {
            log.LogError(ex, "Payment failed for Order {OrderId} after retries", order.OrderId);
            return CreateResult(order.OrderId, OrderStatus.PaymentFailed, steps,
                "Payment failed after multiple attempts.");
        }
    }

    private static async Task<OrderResult?> ProcessShippingStep(
        TaskOrchestrationContext context,
        OrderRequest order,
        List<string> steps,
        bool paymentWasSuccessful,
        ILogger log)
    {
        try
        {
            steps.Add(await context.CallActivityAsync<string>(
                nameof(OrderActivities.ShipOrder), order));
            return null;
        }
        catch (TaskFailedException ex)
        {
            log.LogError(ex, "Shipping failed for Order {OrderId}, initiating compensation", order.OrderId);

            if (paymentWasSuccessful)
            {
                steps.Add(await context.CallActivityAsync<string>(
                    nameof(OrderActivities.RefundPayment), order));
            }

            return CreateResult(order.OrderId, OrderStatus.ShippingFailed, steps,
                "Shipping failed. Payment has been refunded.");
        }
    }


    private static async Task<WorkflowStepResult> HandleManagerApproval(
        TaskOrchestrationContext context,
        OrderRequest order,
        int timeoutMinutes,
        List<string> steps,
        ILogger log)
    {
        steps.Add(await context.CallActivityAsync<string>(
            nameof(OrderActivities.SendApprovalNotification), context.InstanceId));

        using var cts = new CancellationTokenSource();
        var approvalTask = context.WaitForExternalEvent<bool>(ManagerApprovalEvent);
        var timeoutTask = context.CreateTimer(
            context.CurrentUtcDateTime.AddMinutes(timeoutMinutes), cts.Token);

        var completedTask = await Task.WhenAny(approvalTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            log.LogWarning("Approval timeout for Order {OrderId}", order.OrderId);
            steps.Add("Timed out waiting for manager approval");
            return WorkflowStepResult.Stop(CreateResult(order.OrderId, OrderStatus.TimedOut, steps,
                $"No approval received within {timeoutMinutes} minutes."));
        }

        await cts.CancelAsync();

        if (!approvalTask.Result)
        {
            log.LogWarning("Order {OrderId} rejected by manager", order.OrderId);
            steps.Add("Rejected by manager");
            return WorkflowStepResult.Stop(CreateResult(order.OrderId, OrderStatus.RejectedByManager, steps,
                "Order was rejected by the manager."));
        }

        steps.Add("Approved by manager");
        log.LogInformation("Order {OrderId} approved by manager", order.OrderId);
        return WorkflowStepResult.Continue();
    }


    private static OrderResult CreateResult(
        string orderId, OrderStatus status, List<string> steps, string? failure = null) =>
        new()
        {
            OrderId = orderId,
            Status = status,
            Steps = steps.AsReadOnly(),
            FailureReason = failure
        };


}

