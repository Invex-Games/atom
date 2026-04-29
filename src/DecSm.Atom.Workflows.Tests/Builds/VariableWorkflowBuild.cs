namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class VariableWorkflowBuild : WorkflowBuildDefinition, IVarProducerTarget, IVarConsumerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("variable-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets =
            [
                new(nameof(IVarProducerTarget.VarProducerTarget)),
                new(nameof(IVarConsumerTarget.VarConsumerTarget)),
            ],
            Types = [new TestWorkflowType()],
        },
    ];
}

public interface IVarProducerTarget
{
    Target VarProducerTarget =>
        t => t
            .ProducesVariable("MyVar")
            .Executes(() => Task.CompletedTask);
}

public interface IVarConsumerTarget
{
    Target VarConsumerTarget =>
        t => t
            .ConsumesVariable(nameof(IVarProducerTarget.VarProducerTarget), "MyVar")
            .Executes(() => Task.CompletedTask);
}
