namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IBuildTargets,
    ITestTargets,
    IDeployTargets,
    IApproveDependabotPr,
    ICheckPrForBreakingChanges
{
    public static readonly string[] PlatformNames =
    [
        WorkflowLabels.Github.RunsOn.Windows_Latest,
        WorkflowLabels.Github.RunsOn.Windows_11_Arm,
        WorkflowLabels.Github.RunsOn.Ubuntu_Latest,
        WorkflowLabels.Github.RunsOn.Ubuntu_24_04_Arm,
        WorkflowLabels.Github.RunsOn.MacOs_15_Intel,
        WorkflowLabels.Github.RunsOn.MacOs_Latest,
    ];

    public static readonly string[] DevopsPlatformNames =
    [
        WorkflowLabels.Devops.DevopsPool.Windows_Latest,
        WorkflowLabels.Devops.DevopsPool.Ubuntu_Latest,
        WorkflowLabels.Devops.DevopsPool.MacOs_Latest,
    ];

    public static readonly string[] FrameworkNames =
    [
        WorkflowLabels.Dotnet.Framework.Net_8_0,
        WorkflowLabels.Dotnet.Framework.Net_9_0,
        WorkflowLabels.Dotnet.Framework.Net_10_0,
    ];

    private static readonly MatrixDimension TestFrameworkMatrix = new(nameof(ITestTargets.TestFramework))
    {
        Values = FrameworkNames,
    };

    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
        field ??=
        [
            WorkflowOptions.AzureKeyVault.Use,
            WorkflowOptions.UseGitVersionForBuildId.Enabled,
            WorkflowOptions.SetupDotnet.Dotnet100XWithCache,
        ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
        field ??=
        [
            // Build / validate
            new("Validate")
            {
                Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain],
                Targets =
                [
                    WorkflowTargets
                        .SetupBuildInfo
                        .WithGithubTokenInjection(new()
                        {
                            IdToken = GithubTokenPermission.Write,
                            Contents = GithubTokenPermission.Write,
                            PullRequests = GithubTokenPermission.Write,
                        })
                        .WithOptions(WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber,
                            "github.event.number")),
                    WorkflowTargets.PackProjects.WithSuppressedArtifactPublishing,
                    WorkflowTargets.PackTool.WithSuppressedArtifactPublishing.WithGithubRunsOnMatrix(PlatformNames),
                    WorkflowTargets
                        .TestProjects
                        .WithGithubRunsOnMatrix(PlatformNames)
                        .WithMatrixDimensions(TestFrameworkMatrix)
                        .WithOptions(WorkflowOptions.SetupDotnet.Dotnet80X, WorkflowOptions.SetupDotnet.Dotnet90X),
                    WorkflowTargets
                        .CheckPrForBreakingChanges
                        .WithGithubTokenInjection(new()
                        {
                            IdToken = GithubTokenPermission.Write,
                            Contents = GithubTokenPermission.Write,
                            PullRequests = GithubTokenPermission.Write,
                            Checks = GithubTokenPermission.Write,
                        })
                        .WithOptions(WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber,
                            "github.event.number")),
                ],
                WorkflowTypes = [Github.WorkflowType],
                Options = [WorkflowOptions.Github.TokenPermissions.NoneAll],
            },
            new("Build")
            {
                Triggers =
                [
                    WorkflowTriggers.Manual,
                    new GitPushTrigger
                    {
                        IncludedBranches = ["main", "feature/**", "patch/**"],
                    },
                    WorkflowTriggers.Github.OnReleased,
                ],
                Targets =
                [
                    WorkflowTargets
                        .SetupBuildInfo
                        .WithGithubTokenInjection(new()
                        {
                            IdToken = GithubTokenPermission.Write,
                            Contents = GithubTokenPermission.Write,
                            PullRequests = GithubTokenPermission.Write,
                        })
                        .WithOptions(WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber,
                            "github.event.number")),
                    WorkflowTargets.PackProjects,
                    WorkflowTargets.PackTool.WithGithubRunsOnMatrix(PlatformNames),
                    WorkflowTargets
                        .TestProjects
                        .WithGithubRunsOnMatrix(PlatformNames)
                        .WithMatrixDimensions(TestFrameworkMatrix)
                        .WithOptions(WorkflowOptions.SetupDotnet.Dotnet80X, WorkflowOptions.SetupDotnet.Dotnet90X),
                    WorkflowTargets.PushToNuget,
                    WorkflowTargets
                        .PushToRelease
                        .WithGithubTokenInjection(new()
                        {
                            Contents = GithubTokenPermission.Write,
                        })
                        .WithOptions(WorkflowOptions.Target.RunIfWorkflowCondition(new TargetOutputExpression
                            {
                                OutputName = ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName,
                                TargetName = nameof(WorkflowTargets.SetupBuildInfo),
                            }
                            .ContainsString("-")
                            .NotEqualTo(true))),
                ],
                WorkflowTypes = [Github.WorkflowType],
                Options = [WorkflowOptions.Github.TokenPermissions.NoneAll],
            },

            // Test devops
            new("Test_Devops_Build")
            {
                Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain, WorkflowTriggers.PushToMain],
                Targets =
                [
                    WorkflowTargets.SetupBuildInfo,
                    WorkflowTargets.PackProjects,
                    WorkflowTargets.PackTool.WithDevopsPoolMatrix(DevopsPlatformNames),
                    WorkflowTargets
                        .TestProjects
                        .WithDevopsPoolMatrix(DevopsPlatformNames)
                        .WithMatrixDimensions(TestFrameworkMatrix)
                        .WithOptions(WorkflowOptions.SetupDotnet.Dotnet80X, WorkflowOptions.SetupDotnet.Dotnet90X),
                    WorkflowTargets.PushToNugetDevops,
                ],
                WorkflowTypes = [Devops.WorkflowType],
                Options =
                [
                    WorkflowOptions.Inject.Param(WorkflowParams.NugetDryRun, true),
                    WorkflowOptions.Devops.VariableGroup.Atom,
                ],
            },

            // Dependabot
            Github.DependabotWorkflow(new()
            {
                Registries =
                [
                    new("nuget",
                        WorkflowLabels.Github.Dependabot.NugetType,
                        WorkflowLabels.Github.Dependabot.NugetUrl),
                ],
                Updates =
                [
                    new(DependabotValues.NugetEcosystem)
                    {
                        Registries = ["nuget"],
                        Groups =
                        [
                            new("nuget-deps")
                            {
                                Patterns = ["*"],
                            },
                        ],
                        Schedule = DependabotSchedule.Daily,
                    },
                ],
            }),
            new("Dependabot Enable auto-merge")
            {
                Triggers = [WorkflowTriggers.PullIntoMain],
                Targets =
                [
                    WorkflowTargets.ApproveDependabotPr.WithGithubTokenInjection(new()
                    {
                        IdToken = GithubTokenPermission.Write,
                        Contents = GithubTokenPermission.Write,
                        PullRequests = GithubTokenPermission.Write,
                    }),
                ],
                WorkflowTypes = [Github.WorkflowType],
                Options =
                [
                    WorkflowOptions.Github.TokenPermissions.NoneAll,
                    WorkflowOptions.Target.RunIfWorkflowCondition(WorkflowExpressions
                        .Literal("github.actor")
                        .EqualToString("dependabot[bot]")),
                    WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber, "github.event.number"),
                ],
            },
        ];
}
