namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class PermissionsBuild : WorkflowBuildDefinition, IGithubWorkflows, IPermissionsTarget
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
                        new GithubTokenPermissionsOption(new PermissionsEvent
                        {
                            Actions = PermissionsLevel.Write,
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.Github.Action],
            Options = [BuildOptions.Github.TokenPermissions.ReadAll],
        },
    ];
}

public interface IPermissionsTarget
{
    Target PermissionsTarget => t => t;
}
