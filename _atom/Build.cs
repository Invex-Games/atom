namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal partial class Build : WorkflowBuildDefinition,
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
        WorkflowLabels.Devops.Pool.Windows_Latest,
        WorkflowLabels.Devops.Pool.Ubuntu_Latest,
        WorkflowLabels.Devops.Pool.MacOs_Latest,
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

    public override IReadOnlyList<IBuildOption> Options =>
        field ??=
        [
            BuildOptions.GitVersion.ProvideBuildId,
            BuildOptions.GitVersion.ProvideBuildVersion,
            BuildOptions.AzureKeyVault.UseAzureKeyVault,
            BuildOptions.Steps.SetupDotnet.Dotnet100X(),
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
                    new(nameof(ISetupBuildInfo.SetupBuildInfo)),
                    new(nameof(IBuildTargets.PackProjects))
                    {
                        Options = [BuildOptions.Target.SuppressArtifactPublishing],
                    },
                    new(nameof(IBuildTargets.PackTool))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = PlatformNames,
                            },
                        ],
                        Options =
                        [
                            BuildOptions.Target.SuppressArtifactPublishing, BuildOptions.Github.RunsOn.SetByMatrix,
                        ],
                    },
                    new(nameof(ITestTargets.TestProjects))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = PlatformNames,
                            },
                            new(nameof(ITestTargets.TestFramework))
                            {
                                Values = FrameworkNames,
                            },
                        ],
                        Options =
                        [
                            BuildOptions.Target.SuppressArtifactPublishing,
                            BuildOptions.Github.RunsOn.SetByMatrix,
                            BuildOptions.Steps.SetupDotnet.Dotnet80X(),
                            BuildOptions.Steps.SetupDotnet.Dotnet90X(),
                        ],
                    },
                    new(nameof(ICheckPrForBreakingChanges.CheckPrForBreakingChanges))
                    {
                        Options =
                        [
                            BuildOptions.Target.SuppressArtifactPublishing,
                            BuildOptions.Inject.Secret(nameof(IGithubHelper.GithubToken)),
                            new GithubTokenPermissionsOption(new Permissions.Exact(new()
                            {
                                IdTokens = PermissionsLevel.Write,
                                Contents = PermissionsLevel.Write,
                                PullRequests = PermissionsLevel.Write,
                                Checks = PermissionsLevel.Write,
                            })),
                            BuildOptions.Inject.Param(nameof(ICheckPrForBreakingChanges.PullRequestNumber),
                                TextExpressions.Github.GithubEvent["number"]),
                        ],
                    },
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
                WorkflowOptions = [BuildOptions.Github.TokenPermissions.NoneAll],
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
                    new(nameof(ISetupBuildInfo.SetupBuildInfo)),
                    new(nameof(IBuildTargets.PackProjects)),
                    new(nameof(IBuildTargets.PackTool))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = PlatformNames.ToList(),
                            },
                        ],
                        Options = [BuildOptions.Github.RunsOn.SetByMatrix],
                    },
                    new(nameof(ITestTargets.TestProjects))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = PlatformNames.ToList(),
                            },
                            new(nameof(ITestTargets.TestFramework))
                            {
                                Values = FrameworkNames.ToList(),
                            },
                        ],
                        Options =
                        [
                            BuildOptions.Github.RunsOn.SetByMatrix,
                            BuildOptions.Steps.SetupDotnet.Dotnet80X(),
                            BuildOptions.Steps.SetupDotnet.Dotnet90X(),
                        ],
                    },
                    new(nameof(IDeployTargets.PushToNuget)),
                    new(nameof(IDeployTargets.PushToRelease))
                    {
                        Options =
                        [
                            BuildOptions.Inject.Secret(nameof(IGithubHelper.GithubToken)),
                            new GithubTokenPermissionsOption(new Permissions.Exact(new()
                            {
                                Contents = PermissionsLevel.Write,
                            })),
                            BuildOptions.Target.RunIfWorkflowCondition(new TargetOutputExpression
                                {
                                    OutputName = ParamDefinitions[nameof(ISetupBuildInfo.BuildVersion)].ArgName,
                                    TargetName = nameof(ISetupBuildInfo.SetupBuildInfo),
                                }
                                .Contains("-")
                                .NotEqualTo(true)),
                        ],
                    },
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
                WorkflowOptions = [BuildOptions.Github.TokenPermissions.NoneAll],
            },

            // Test devops
            new("Test_Devops_Build")
            {
                Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain, WorkflowTriggers.PushToMain],
                Targets =
                [
                    new(nameof(ISetupBuildInfo.SetupBuildInfo)),
                    new(nameof(IBuildTargets.PackProjects)),
                    new(nameof(IBuildTargets.PackTool))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = DevopsPlatformNames,
                            },
                        ],
                        Options = [BuildOptions.Devops.DevopsPool.SetByMatrix],
                    },
                    new(nameof(ITestTargets.TestProjects))
                    {
                        MatrixDimensions =
                        [
                            new(nameof(IJobRunsOn.JobRunsOn))
                            {
                                Values = DevopsPlatformNames,
                            },
                            new(nameof(ITestTargets.TestFramework))
                            {
                                Values = FrameworkNames,
                            },
                        ],
                        Options =
                        [
                            BuildOptions.Devops.DevopsPool.SetByMatrix,
                            BuildOptions.Steps.SetupDotnet.Dotnet80X(),
                            BuildOptions.Steps.SetupDotnet.Dotnet90X(),
                        ],
                    },
                    new(nameof(IDeployTargets.PushToNugetDevops)),
                ],
                WorkflowTypes = [Devops.WorkflowType],
                WorkflowOptions =
                [
                    BuildOptions.Inject.Param(nameof(INugetHelper.NugetDryRun), true),
                    BuildOptions.Devops.VariableGroup.Atom,
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
                    new(nameof(IApproveDependabotPr.ApproveDependabotPr))
                    {
                        Options =
                        [
                            BuildOptions.Inject.Secret(nameof(IGithubHelper.GithubToken)),
                            new GithubTokenPermissionsOption(new Permissions.Exact(new()
                            {
                                IdTokens = PermissionsLevel.Write,
                                Contents = PermissionsLevel.Write,
                                PullRequests = PermissionsLevel.Write,
                            })),
                        ],
                    },
                ],
                WorkflowTypes = [WorkflowTypes.Github.Action],
                WorkflowOptions =
                [
                    BuildOptions.Github.TokenPermissions.NoneAll,
                    BuildOptions.Target.RunIfWorkflowCondition(
                        TextExpressions.Github.GithubActor.EqualToString("dependabot[bot]")),
                    BuildOptions.Inject.Param(nameof(ICheckPrForBreakingChanges.PullRequestNumber),
                        TextExpressions.Github.GithubEvent["number"]),
                ],
            },
        ];
}
