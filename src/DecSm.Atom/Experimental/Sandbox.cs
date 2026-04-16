namespace DecSm.Atom.Experimental;

[UnstableAPI]
public sealed record RunTargetStepIf(WorkflowExpression Condition) : IWorkflowOption;
