namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record TargetCondition(WorkflowExpression Value) : IWorkflowOption;
