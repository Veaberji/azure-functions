namespace EDU.Func.Durable.Models;

/// <summary>
/// Represents the final status of an order processing workflow.
/// </summary>
public enum OrderStatus
{
    Completed,
    ValidationFailed,
    RejectedByManager,
    TimedOut,
    PaymentFailed,
    ShippingFailed
}
