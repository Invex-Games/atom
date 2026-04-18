namespace DecSm.Atom.Workflows.Options;

[UnstableAPI]
public sealed record TargetStepCondition(WorkflowExpression Condition) : IWorkflowOption;
