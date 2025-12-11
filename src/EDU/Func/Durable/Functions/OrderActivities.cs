using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EDU.Func.Durable.Exceptions;
using EDU.Func.Durable.Models;
using EDU.Func.Durable.Services;
using static EDU.Func.Durable.OrderProcessingConstants;

namespace EDU.Func.Durable.Functions;

/// <summary>
/// Activity functions for Order Processing workflow.
/// Each activity is an independent, idempotent unit of work.
/// </summary>
public sealed class OrderActivities(
    ILogger<OrderActivities> logger,
    IConfiguration configuration,
    IPaymentGateway paymentGateway)
{
    [Function(nameof(ValidateOrder))]
    public ValidationResult ValidateOrder([ActivityTrigger] OrderRequest order)
    {
        logger.LogInformation("Validating Order {OrderId}...", order.OrderId);

        var result = order.Validate();
        
        if (result.IsValid)
            logger.LogInformation("Order {OrderId} validated successfully", order.OrderId);
        else
            logger.LogWarning("Order {OrderId} validation failed: {Error}", order.OrderId, result.ErrorMessage);

        return result;
    }

    [Function(nameof(ProcessPayment))]
    public async Task<string> ProcessPayment([ActivityTrigger] OrderRequest order)
    {
        logger.LogInformation("Processing payment for Order {OrderId}, Amount=${Amount}",
            order.OrderId, order.Amount);

        await paymentGateway.ProcessPaymentAsync(order.OrderId, order.Amount);

        logger.LogInformation("Payment successful for Order {OrderId}", order.OrderId);
        return $"✓ Processed payment ${order.Amount} for {order.OrderId}";
    }

    [Function(nameof(ShipOrder))]
    public string ShipOrder([ActivityTrigger] OrderRequest order)
    {
        logger.LogInformation("Initiating shipment for Order {OrderId}", order.OrderId);

        // Simulate shipping failure for testing (use "noship" in OrderId)
        if (order.OrderId.Contains("noship", StringComparison.OrdinalIgnoreCase))
        {
            throw new ShippingException($"Shipping service unavailable for order {order.OrderId}");
        }

        logger.LogInformation("Shipment created for Order {OrderId}", order.OrderId);
        return $"✓ Shipped order {order.OrderId}";
    }

    [Function(nameof(RefundPayment))]
    public async Task<string> RefundPayment([ActivityTrigger] OrderRequest order)
    {
        logger.LogWarning("REFUNDING ${Amount} for Order {OrderId} due to failed shipment",
            order.Amount, order.OrderId);

        await paymentGateway.RefundPaymentAsync(order.OrderId);

        logger.LogInformation("Refund completed for Order {OrderId}", order.OrderId);
        return $"✓ Refunded ${order.Amount} for {order.OrderId}";
    }

    [Function(nameof(SendApprovalNotification))]
    public string SendApprovalNotification([ActivityTrigger] string instanceId)
    {
        var baseUrl = configuration.GetValue(FunctionBaseUrlKey, DefaultFunctionBaseUrl);
        var approvalUrl = $"{baseUrl}/api/{ApprovalEndpoint}?instanceId={instanceId}";

        // In production: integrate with SendGrid, Slack, Teams, etc.
        logger.LogWarning("╔══════════════════════════════════════════╗");
        logger.LogWarning("║  ACTION REQUIRED: Manager Approval       ║");
        logger.LogWarning("║  Approve: POST {Url}                     ║", approvalUrl);
        logger.LogWarning("║  Body: {{ \"isApproved\": true }}           ║");
        logger.LogWarning("╚══════════════════════════════════════════╝");

        return $"✓ Approval notification sent (Instance: {instanceId})";
    }
}
