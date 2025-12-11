namespace EDU.Func.Durable.Services;

/// <summary>
/// Interface for payment gateway operations.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Process a payment with idempotency support.
    /// </summary>
    /// <param name="idempotencyKey">Unique key for deduplication (typically OrderId).</param>
    /// <param name="amount">Amount to charge.</param>
    Task ProcessPaymentAsync(string idempotencyKey, decimal amount);
    
    /// <summary>
    /// Refund a previously processed payment.
    /// </summary>
    /// <param name="idempotencyKey">The original payment's idempotency key.</param>
    Task RefundPaymentAsync(string idempotencyKey);
}
