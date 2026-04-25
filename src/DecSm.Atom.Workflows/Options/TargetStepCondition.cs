namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record TargetStepCondition(TextExpression Condition) : IWorkflowOption;
