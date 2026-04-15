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
            WorkflowOptions.Steps.SetupDotnet.Dotnet100X(),
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
                    WorkflowTargets.SetupBuildInfo,
                    WorkflowTargets.PackProjects.WithOptions(WorkflowOptions.Target.SuppressArtifactPublishing),
                    WorkflowTargets
                        .PackTool
                        .WithOptions(WorkflowOptions.Target.SuppressArtifactPublishing)
                        .WithGithubRunsOnMatrix(PlatformNames),
                    WorkflowTargets
                        .TestProjects
                        .WithGithubRunsOnMatrix(PlatformNames)
                        .WithMatrixDimensions(TestFrameworkMatrix)
                        .WithOptions(WorkflowOptions.Steps.SetupDotnet.Dotnet80X(),
                            WorkflowOptions.Steps.SetupDotnet.Dotnet90X()),
                    WorkflowTargets
                        .CheckPrForBreakingChanges
                        .WithGithubTokenInjection(new PermissionsEvent
                        {
                            IdTokens = PermissionsLevel.Write,
                            Contents = PermissionsLevel.Write,
                            PullRequests = PermissionsLevel.Write,
                            Checks = PermissionsLevel.Write,
                        })
                        .WithOptions(WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber,
                            WorkflowExpressions.Github.GithubEvent["number"])),
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
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
                    new GithubTrigger(new On.Release([On.Release.ReleaseType.released])),
                ],
                Targets =
                [
                    WorkflowTargets.SetupBuildInfo,
                    WorkflowTargets.PackProjects,
                    WorkflowTargets.PackTool.WithGithubRunsOnMatrix(PlatformNames),
                    WorkflowTargets
                        .TestProjects
                        .WithGithubRunsOnMatrix(PlatformNames)
                        .WithMatrixDimensions(TestFrameworkMatrix)
                        .WithOptions(WorkflowOptions.Steps.SetupDotnet.Dotnet80X(),
                            WorkflowOptions.Steps.SetupDotnet.Dotnet90X()),
                    WorkflowTargets.PushToNuget,
                    WorkflowTargets
                        .PushToRelease
                        .WithGithubTokenInjection(new PermissionsEvent
                        {
                            Contents = PermissionsLevel.Write,
                        })
                        .WithOptions(WorkflowOptions.Target.RunIfWorkflowCondition(new TargetOutputExpression
                            {
                                OutputName = ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName,
                                TargetName = nameof(WorkflowTargets.SetupBuildInfo),
                            }
                            .Contains("-")
                            .NotEqualTo(true))),
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
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
                        .WithOptions(WorkflowOptions.Steps.SetupDotnet.Dotnet80X(),
                            WorkflowOptions.Steps.SetupDotnet.Dotnet90X()),
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
            WorkflowPresets.Github.Dependabot(new()
            {
                Registries = new Dictionary<string, DependabotRegistry>
                {
                    ["nuget"] = new()
                    {
                        Type = RegistryType.NugetFeed,
                        Url = WorkflowLabels.Github.Dependabot.NugetUrl,
                    },
                },
                Updates =
                [
                    new()
                    {
                        Directory = "/",
                        PackageEcosystem = WorkflowLabels.Github.Dependabot.NugetEcosystem,
                        Registries = new DependabotRegistries.Named("nuget"),
                        Groups = new Dictionary<string, DependabotGroup>
                        {
                            ["nuget-deps"] = new DependabotGroup.FromPatterns
                            {
                                Patterns = ["*"],
                            },
                        },
                        Schedule = new()
                        {
                            Interval = ScheduleInterval.Daily,
                        },
                        TargetBranch = "main",
                        OpenPullRequestsLimit = 10,
                    },
                ],
            }),
            new("Dependabot Enable auto-merge")
            {
                Triggers = [WorkflowTriggers.PullIntoMain],
                Targets =
                [
                    WorkflowTargets.ApproveDependabotPr.WithGithubTokenInjection(new PermissionsEvent
                    {
                        IdTokens = PermissionsLevel.Write,
                        Contents = PermissionsLevel.Write,
                        PullRequests = PermissionsLevel.Write,
                    }),
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
                Options =
                [
                    WorkflowOptions.Github.TokenPermissions.NoneAll,
                    WorkflowOptions.Target.RunIfWorkflowCondition(
                        WorkflowExpressions.Github.GithubActor.EqualToString("dependabot[bot]")),
                    WorkflowOptions.Inject.Param(WorkflowParams.PullRequestNumber,
                        WorkflowExpressions.Github.GithubEvent["number"]),
                ],
            },
        ];
}
