namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ConcurrencyBuild : WorkflowBuildDefinition, IGithubWorkflows, IConcurrencyTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("concurrency-workflow")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(IConcurrencyTarget.ConcurrencyTarget))],
            Types = [WorkflowTypes.Github.Action],
            Options =
            [
                BuildOptions.Github.Concurrency.Set(TextExpressions.Concat([
                        TextExpressions.Github.GithubWorkflow.Evaluate(),
                        "-",
                        TextExpressions.Github.GithubRef.Evaluate(),
                    ]),
                    true),
            ],
        },
    ];
}

public interface IConcurrencyTarget
{
    Target ConcurrencyTarget => t => t;
}
