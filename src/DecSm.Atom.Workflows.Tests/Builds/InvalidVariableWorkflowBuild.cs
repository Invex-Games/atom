namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class InvalidVariableWorkflowBuild : WorkflowBuildDefinition, IVarProducerTarget, IBadConsumerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("invalid-variable-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets =
            [
                new(nameof(IVarProducerTarget.VarProducerTarget)),
                new(nameof(IBadConsumerTarget.BadConsumerTarget)),
            ],
            Types = [new TestWorkflowType()],
        },
    ];
}

public interface IBadConsumerTarget
{
    Target BadConsumerTarget =>
        t => t
            .ConsumesVariable(nameof(IVarProducerTarget.VarProducerTarget), "NonExistentVar")
            .Executes(() => Task.CompletedTask);
}
