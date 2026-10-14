namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ConcurrencyBuild : WorkflowBuildDefinition, IGitHubWorkflows, IConcurrencyTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("concurrency-workflow")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(IConcurrencyTarget.ConcurrencyTarget))],
            Types = [WorkflowTypes.GitHub.Action],
            Options =
            [
                BuildOptions.GitHub.Concurrency.Set(TextExpressions.Concat([
                        TextExpressions.GitHub.GitHubWorkflow.Evaluate(),
                        "-",
                        TextExpressions.GitHub.GitHubRef.Evaluate(),
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
