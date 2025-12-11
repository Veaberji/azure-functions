using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDU.Func.Durable.Models;
using static EDU.Func.Durable.OrderProcessingConstants;

namespace EDU.Func.Durable.Functions;

/// <summary>
/// HTTP entry points for the Order Processing workflow.
/// </summary>
public sealed class OrderHttpTriggers(ILogger<OrderHttpTriggers> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    /// <summary>
    /// Start a new order processing workflow.
    /// POST /api/OrderProcessing_HttpStart
    /// </summary>
    [Function("OrderProcessing_HttpStart")]
    public async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        OrderRequest? orderRequest;
        try
        {
            orderRequest = await JsonSerializer.DeserializeAsync<OrderRequest>(req.Body, JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize order request");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest,
                "Invalid JSON format. Please provide a valid order request.");
        }

        if (orderRequest is null)
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body cannot be empty.");
        }

        var validation = orderRequest.Validate();
        if (!validation.IsValid)
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, validation.ErrorMessage!);
        }

        var instanceId = $"{OrderInstancePrefix}{orderRequest.OrderId}";

        if (await TryGetExistingActiveInstance(client, instanceId, cancellationToken))
        {
            logger.LogInformation("Order {OrderId} already has an active workflow {InstanceId}",
                orderRequest.OrderId, instanceId);
            return client.CreateCheckStatusResponse(req, instanceId);
        }

        await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(OrderOrchestrator.RunOrchestrator),
            orderRequest,
            new StartOrchestrationOptions { InstanceId = instanceId },
            cancellationToken);

        logger.LogInformation("Started orchestration for Order {OrderId} with InstanceId={InstanceId}",
            orderRequest.OrderId, instanceId);

        return client.CreateCheckStatusResponse(req, instanceId);
    }

    /// <summary>
    /// Manager approval/rejection endpoint.
    /// POST /api/OrderProcessing_Approve?instanceId=...
    /// </summary>
    [Function(ApprovalEndpoint)]
    public async Task<HttpResponseData> ApproveOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        CancellationToken cancellationToken)
    {
        var instanceId = req.Query["instanceId"];
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Query parameter 'instanceId' is required.");
        }

        var validationError = await ValidateOrchestrationInstance(req, client, instanceId, cancellationToken);
        if (validationError is not null)
        {
            return validationError;
        }

        ApprovalRequest? approval;
        try
        {
            approval = await JsonSerializer.DeserializeAsync<ApprovalRequest>(req.Body, JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to deserialize approval request");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest,
                "Invalid JSON. Expected: { \"isApproved\": true/false }");
        }

        if (approval is null)
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body is required.");
        }

        await client.RaiseEventAsync(instanceId, ManagerApprovalEvent, approval.IsApproved, cancellationToken);

        logger.LogInformation("Approval signal sent for {InstanceId}: Approved={IsApproved}",
            instanceId, approval.IsApproved);

        return await CreateJsonResponse(req, HttpStatusCode.OK, new ApprovalResponse
        {
            Message = "Approval signal sent successfully",
            InstanceId = instanceId,
            Approved = approval.IsApproved
        }, cancellationToken);
    }


    private static async Task<bool> TryGetExistingActiveInstance(
        DurableTaskClient client,
        string instanceId,
        CancellationToken cancellationToken)
    {
        var existing = await client.GetInstanceAsync(instanceId, cancellationToken);
        return existing is not null &&
               existing.RuntimeStatus is OrchestrationRuntimeStatus.Running or OrchestrationRuntimeStatus.Pending;
    }

    private static async Task<HttpResponseData?> ValidateOrchestrationInstance(
        HttpRequestData req,
        DurableTaskClient client,
        string instanceId,
        CancellationToken cancellationToken)
    {
        var instance = await client.GetInstanceAsync(instanceId, cancellationToken);

        if (instance is null)
        {
            return await CreateErrorResponse(req, HttpStatusCode.NotFound,
                $"No orchestration found with ID '{instanceId}'.");
        }

        if (instance.RuntimeStatus is not OrchestrationRuntimeStatus.Running)
        {
            return await CreateErrorResponse(req, HttpStatusCode.Conflict,
                $"Orchestration is not running (status: {instance.RuntimeStatus}).");
        }

        return null;
    }


    private static async Task<HttpResponseData> CreateJsonResponse<T>(
        HttpRequestData req,
        HttpStatusCode status,
        T data,
        CancellationToken cancellationToken)
    {
        var response = req.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(data, JsonOptions), cancellationToken);
        return response;
    }

    private static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req,
        HttpStatusCode status,
        string message)
    {
        var response = req.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new { error = message }));
        return response;
    }


}
