namespace EDU.Func.Durable.Models;

/// <summary>
/// Result of a validation operation.
/// </summary>
public readonly record struct ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}
