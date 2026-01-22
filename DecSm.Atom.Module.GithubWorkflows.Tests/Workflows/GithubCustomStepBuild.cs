namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

public sealed record GithubTestStep(string Text, GithubCustomStepOrder Order, int Priority = 0)
    : GithubCustomStepOption(Order, Priority)
{
    public override void WriteStep(GithubStepWriter writer)
    {
        using (writer.WriteSection("- name: Test step"))
            writer.WriteLine($"run: echo \"{Text}\"");
    }
}

[BuildDefinition]
public partial class GithubCustomStepBuild : MinimalBuildDefinition, IGithubWorkflows, ICustomStepTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("github-custom-step-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets = [WorkflowTargets.CustomStepTarget],
            Options =
            [
                new GithubTestStep("Pre step 1", GithubCustomStepOrder.BeforeTarget, 1),
                new GithubTestStep("Pre step 3", GithubCustomStepOrder.BeforeTarget, 3),
                new GithubTestStep("Pre step 2", GithubCustomStepOrder.BeforeTarget, 2),
                new GithubTestStep("Post step 3", GithubCustomStepOrder.AfterTarget, 3),
                new GithubTestStep("Post step 1", GithubCustomStepOrder.AfterTarget, 1),
                new GithubTestStep("Post step 2", GithubCustomStepOrder.AfterTarget, 2),
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

public interface ICustomStepTarget
{
    Target CustomStepTarget => t => t;
}
