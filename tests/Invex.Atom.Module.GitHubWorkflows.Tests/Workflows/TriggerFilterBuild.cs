namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class TriggerFilterBuild : WorkflowBuildDefinition, IGitHubWorkflows, ITriggerFilterTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Only one list per filter: written as-is (e.g. paths or paths-ignore)
        new("single-filter-workflow")
        {
            Triggers =
            [
                new GitPushTrigger
                {
                    IncludedBranches = ["main"],
                    IncludedPaths = ["src/AppA/**", "src/Shared/**"],
                    IncludedTags = ["appa/v*"],
                },
                new GitPullRequestTrigger
                {
                    ExcludedBranches = ["dependabot/**"],
                    ExcludedPaths = ["docs/**", "**.md"],
                },
            ],
            Targets = [new(nameof(ITriggerFilterTarget.TriggerFilterTarget))],
            Types = [WorkflowTypes.GitHub.Action],
        },

        // Both lists per filter: GitHub rejects e.g. paths with paths-ignore, so the exclusions become negated patterns
        new("combined-filter-workflow")
        {
            Triggers =
            [
                new GitPushTrigger
                {
                    IncludedBranches = ["main", "release/**"],
                    ExcludedBranches = ["release/old-*"],
                    IncludedPaths = ["src/AppA/**", "src/Shared/**"],
                    ExcludedPaths = ["src/AppA/docs/**"],
                    IncludedTags = ["appa/v*"],
                    ExcludedTags = ["appa/v*-rc*"],
                },
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                    ExcludedBranches = ["main-old"],
                    IncludedPaths = ["src/AppA/**"],
                    ExcludedPaths = ["src/AppA/docs/**"],
                },
            ],
            Targets = [new(nameof(ITriggerFilterTarget.TriggerFilterTarget))],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface ITriggerFilterTarget
{
    Target TriggerFilterTarget => t => t;
}
