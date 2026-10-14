namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class PermissionsBuild : WorkflowBuildDefinition, IGitHubWorkflows, IPermissionsTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("permissions-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                new(nameof(IPermissionsTarget.PermissionsTarget))
                {
                    Options =
                    [
                        new GitHubTokenPermissionsOption(new PermissionsEvent
                        {
                            Actions = PermissionsLevel.Write,
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
            Options = [BuildOptions.GitHub.TokenPermissions.ReadAll],
        },
    ];
}

public interface IPermissionsTarget
{
    Target PermissionsTarget => t => t;
}
