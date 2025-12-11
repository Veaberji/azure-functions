namespace EDU.Func.Durable;

/// <summary>
/// Constants for Order Processing workflow.
/// </summary>
public static class OrderProcessingConstants
{
    // Event names
    public const string ManagerApprovalEvent = "ManagerApproval";
    
    // Instance ID prefix
    public const string OrderInstancePrefix = "order-";
    
    // Configuration keys
    public const string ApprovalThresholdKey = "ApprovalThreshold";
    public const string ApprovalTimeoutMinutesKey = "ApprovalTimeoutMinutes";
    public const string FunctionBaseUrlKey = "FunctionBaseUrl";
    
    // Default values
    public const decimal DefaultApprovalThreshold = 1000m;
    public const int DefaultApprovalTimeoutMinutes = 5;
    public const string DefaultFunctionBaseUrl = "http://localhost:7071";
    
    // Endpoints
    public const string ApprovalEndpoint = "OrderProcessing_Approve";
}
