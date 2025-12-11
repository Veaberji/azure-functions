using EDU.Func.Durable.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EDU.Func.Durable.Services;

/// <summary>
/// Simulated payment gateway for educational purposes.
/// Demonstrates idempotency via provider-side deduplication.
/// 
/// In production, replace with real gateway (Stripe, PayPal, etc.)
/// </summary>
public sealed class SimulatedPaymentGateway(ILogger<SimulatedPaymentGateway> logger) : IPaymentGateway
{
    // Thread-safe ledger (in production: Redis, database, or external service state)
    private static readonly ConcurrentDictionary<string, bool> Ledger = new();

    public async Task ProcessPaymentAsync(string idempotencyKey, decimal amount)
    {
        // Idempotency: Provider-side deduplication
        if (Ledger.ContainsKey(idempotencyKey))
        {
            logger.LogInformation("Payment already processed for {Key}, returning cached result", idempotencyKey);
            return;
        }

        // Simulate payment failure for testing (use "fail" in OrderId)
        if (idempotencyKey.Contains("fail", StringComparison.OrdinalIgnoreCase))
        {
            throw new PaymentException($"Payment gateway rejected transaction for {idempotencyKey}");
        }

        // Simulate network latency
        await Task.Delay(Random.Shared.Next(100, 500));

        // Atomic commit
        if (!Ledger.TryAdd(idempotencyKey, true))
        {
            logger.LogInformation("Concurrent payment detected for {Key}, already processed", idempotencyKey);
            return;
        }

        logger.LogInformation("Payment Provider: Successfully charged ${Amount}", amount);
    }

    public Task RefundPaymentAsync(string idempotencyKey)
    {
        Ledger.TryRemove(idempotencyKey, out _);
        logger.LogInformation("Payment Provider: Refunded payment for {Key}", idempotencyKey);
        return Task.CompletedTask;
    }
}
