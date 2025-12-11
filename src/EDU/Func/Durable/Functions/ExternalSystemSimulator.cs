using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using EDU.Func.Durable.Models;
using static EDU.Func.Durable.OrderProcessingConstants;

namespace EDU.Func.Durable.Functions;

/// <summary>
/// Simulates an external system (Manager's Dashboard, Email Link, etc.)
/// that triggers order approval.
/// 
/// This demonstrates how an external system would integrate with
/// the Durable Function's approval endpoint.
/// </summary>
public class ExternalSystemSimulator(
    ILogger<ExternalSystemSimulator> logger, 
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    /// <summary>
    /// SIMULATION: Manager clicks "Approve" button on their dashboard.
    /// POST /api/SimulateManagerApproval?instanceId=...&approve=true
    /// </summary>
    [Function("SimulateManagerApproval")]
    public async Task<HttpResponseData> SimulateApproval(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var instanceId = req.Query["instanceId"];
        if (string.IsNullOrEmpty(instanceId))
        {
            var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
            await badReq.WriteStringAsync("Missing instanceId query parameter.");
            return badReq;
        }

        // Optional: allow rejecting via query param (default: approve)
        var shouldApprove = !string.Equals(req.Query["approve"], "false", StringComparison.OrdinalIgnoreCase);

        logger.LogInformation("Manager is reviewing Order {InstanceId}... (Approve={Approve})", 
            instanceId, shouldApprove);

        // 1. Simulate Manager thinking time
        await Task.Delay(1000);

        // 2. Prepare the approval payload
        var approvalPayload = new ApprovalRequest(shouldApprove);
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(approvalPayload),
            Encoding.UTF8,
            "application/json");

        // 3. Call the Durable Function Approval Endpoint
        //    In real world: this would be a React/Angular app calling the backend
        var baseUrl = configuration.GetValue(FunctionBaseUrlKey, DefaultFunctionBaseUrl);
        var approvalUrl = $"{baseUrl}/api/{ApprovalEndpoint}?instanceId={instanceId}";

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(approvalUrl, jsonContent);

        // 4. Return result to the "Manager"
        var res = req.CreateResponse(response.StatusCode);
        res.Headers.Add("Content-Type", "application/json");
        
        if (response.IsSuccessStatusCode)
        {
            await res.WriteStringAsync(JsonSerializer.Serialize(new
            {
                message = shouldApprove 
                    ? $"Successfully approved order {instanceId}. Workflow should continue now."
                    : $"Successfully rejected order {instanceId}. Workflow will terminate.",
                instanceId,
                approved = shouldApprove
            }));
        }
        else
        {
            await res.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error = $"Failed to process approval. Status: {response.StatusCode}",
                instanceId
            }));
        }

        return res;
    }
}
