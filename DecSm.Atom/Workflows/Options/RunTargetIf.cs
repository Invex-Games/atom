namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record RunTargetIf : IWorkflowOption
{
    public required WorkflowExpression Condition { get; init; }

    public static RunTargetIf Expression(WorkflowExpression condition) =>
        new()
        {
            Condition = condition,
        };
}
