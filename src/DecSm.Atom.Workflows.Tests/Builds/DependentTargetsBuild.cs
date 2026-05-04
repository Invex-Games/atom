namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class DependentTargetsBuild : WorkflowBuildDefinition, IFirstTarget, ISecondTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("dependent-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [new(nameof(IFirstTarget.FirstTarget)), new(nameof(ISecondTarget.SecondTarget))],
            Types = [new TestWorkflowType()],
        },
    ];
}

public interface IFirstTarget
{
    Target FirstTarget => t => t.Executes(() => Task.CompletedTask);
}

public interface ISecondTarget
{
    Target SecondTarget =>
        t => t
            .DependsOn(nameof(IFirstTarget.FirstTarget))
            .Executes(() => Task.CompletedTask);
}
