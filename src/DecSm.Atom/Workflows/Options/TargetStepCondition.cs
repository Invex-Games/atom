namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record TargetStepCondition(WorkflowExpression Condition) : IWorkflowOption;
