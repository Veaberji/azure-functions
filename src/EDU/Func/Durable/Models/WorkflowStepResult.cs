namespace EDU.Func.Durable.Models;

/// <summary>
/// Result of a workflow step that may or may not terminate the workflow.
/// </summary>
public readonly record struct WorkflowStepResult
{
    public bool ShouldContinue { get; init; }
    public OrderResult? TerminalResult { get; init; }

    public static WorkflowStepResult Continue() => new() { ShouldContinue = true };
    
    public static WorkflowStepResult Stop(OrderResult result) => new() 
    { 
        ShouldContinue = false, 
        TerminalResult = result 
    };
}
