namespace EDU.Func.Durable.Models;

/// <summary>
/// Represents an incoming order request with validation.
/// </summary>
public sealed record OrderRequest
{
    public required string OrderId { get; init; }
    public required decimal Amount { get; init; }

    /// <summary>
    /// Validates the order request and returns a result.
    /// </summary>
    public ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(OrderId))
            return ValidationResult.Failure("OrderId is required.");

        if (Amount <= 0)
            return ValidationResult.Failure($"Amount must be positive, but was {Amount}.");

        return ValidationResult.Success();
    }
}
