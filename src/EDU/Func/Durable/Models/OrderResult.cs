namespace EDU.Func.Durable.Models;

/// <summary>
/// Rich result object returned by the orchestration.
/// </summary>
public sealed record OrderResult
{
    public required string OrderId { get; init; }
    public required OrderStatus Status { get; init; }
    public required IReadOnlyList<string> Steps { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.UtcNow;
}
