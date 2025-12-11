namespace EDU.Func.Durable.Models;

/// <summary>
/// Response DTO for approval endpoint.
/// </summary>
public sealed record ApprovalResponse
{
    public required string Message { get; init; }
    public required string InstanceId { get; init; }
    public required bool Approved { get; init; }
}
