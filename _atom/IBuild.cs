namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal interface IBuild : IWorkflowBuildDefinition,
    IDotnetUserSecrets,
    IDevopsWorkflows,
    IGithubWorkflows,
    IGitVersion,
    IBuildTargets,
    ITestTargets,
    IDeployTargets,
    IApproveDependabotPr,
    ICheckPrForBreakingChanges,
    IDocTargets
{
    static readonly string[] PlatformNames =
    [
        WorkflowLabels.Github.RunsOn.Windows_Latest,
        WorkflowLabels.Github.RunsOn.Windows_11_Arm,
        WorkflowLabels.Github.RunsOn.Ubuntu_Latest,
        WorkflowLabels.Github.RunsOn.Ubuntu_24_04_Arm,
        WorkflowLabels.Github.RunsOn.MacOs_15_Intel,
        WorkflowLabels.Github.RunsOn.MacOs_Latest,
    ];

    static readonly string[] DevopsPlatformNames =
    [
        WorkflowLabels.Devops.Pool.Windows_Latest,
        WorkflowLabels.Devops.Pool.Ubuntu_Latest,
        WorkflowLabels.Devops.Pool.MacOs_Latest,
    ];

    static readonly string[] FrameworkNames =
    [
        WorkflowLabels.Dotnet.Framework.Net_8_0,
        WorkflowLabels.Dotnet.Framework.Net_9_0,
        WorkflowLabels.Dotnet.Framework.Net_10_0,
    ];

    IReadOnlyList<IBuildOption> IBuildDefinition.Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId,
        BuildOptions.GitVersion.ProvideBuildVersion,
        BuildOptions.Steps.SetupDotnet.Dotnet100X(),
    ];

    IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
    [
        // Build / validate
        new("Validate")
        {
            Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain],
            Targets =
            [
                new(nameof(SetupBuildInfo)),
                new(nameof(PackProjects))
                {
                    Options = [BuildOptions.Target.SuppressArtifactPublishing],
                },
                new(nameof(PackTool))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = PlatformNames,
                        },
                    ],
                    Options = [BuildOptions.Target.SuppressArtifactPublishing, BuildOptions.Github.RunsOn.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = PlatformNames,
                        },
                        new(nameof(TestFramework))
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
                new(nameof(BuildDocs))
                {
                    Options = [BuildOptions.Target.SuppressArtifactPublishing],
                },
                new(nameof(CheckPrForBreakingChanges))
                {
                    Options =
                    [
                        BuildOptions.Target.SuppressArtifactPublishing,
                        BuildOptions.Inject.Secret(nameof(GithubToken)),
                        BuildOptions.Github.TokenPermissions.Set(new Permissions.Exact(new()
                        {
                            IdTokens = PermissionsLevel.Write,
                            Contents = PermissionsLevel.Write,
                            PullRequests = PermissionsLevel.Write,
                            Checks = PermissionsLevel.Write,
                        })),
                        BuildOptions.Inject.Param(nameof(PullRequestNumber),
                            TextExpressions.Github.GithubEvent["number"]),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            TextExpressions.Github.GithubEventName.EqualToString("pull_request")),
                    ],
                },
            ],
            Types = [WorkflowTypes.Github.Action],
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
                new(nameof(SetupBuildInfo)),
                new(nameof(PackProjects)),
                new(nameof(PackTool))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = PlatformNames.ToList(),
                        },
                    ],
                    Options = [BuildOptions.Github.RunsOn.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = PlatformNames.ToList(),
                        },
                        new(nameof(TestFramework))
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
                new(nameof(BuildDocs)),
                new(nameof(PublishDocs))
                {
                    Options =
                    [
                        BuildOptions.Inject.Secret(nameof(GithubToken)),
                        new GithubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            Contents = PermissionsLevel.Write,
                        })),
                        BuildOptions.Target.RunIfWorkflowCondition(TextExpressions
                            .Target
                            .ParamOutput(this, nameof(SetupBuildInfo), nameof(BuildVersion))
                            .Contains("-")
                            .EqualTo(false)),
                    ],
                },
                new(nameof(PushToNuget))
                {
                    Options = [BuildOptions.Inject.Secret(nameof(NugetApiKey))],
                },
                new(nameof(PushToRelease))
                {
                    Options =
                    [
                        BuildOptions.Inject.Secret(nameof(GithubToken)),
                        new GithubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            Contents = PermissionsLevel.Write,
                        })),
                        BuildOptions.Target.RunIfWorkflowCondition(TextExpressions
                            .Target
                            .ParamOutput(this, nameof(SetupBuildInfo), nameof(BuildVersion))
                            .Contains("-")
                            .EqualTo(false)),
                    ],
                },
            ],
            Types = [WorkflowTypes.Github.Action],
        },

        // Test devops
        new("Test_Devops_Build")
        {
            Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain, WorkflowTriggers.PushToMain],
            Targets =
            [
                new(nameof(SetupBuildInfo)),
                new(nameof(PackProjects)),
                new(nameof(PackTool))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = DevopsPlatformNames,
                        },
                    ],
                    Options = [BuildOptions.Devops.DevopsPool.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = DevopsPlatformNames,
                        },
                        new(nameof(TestFramework))
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
                new(nameof(PushToNugetDevops))
                {
                    Options = [BuildOptions.Inject.Secret(nameof(NugetApiKey))],
                },
            ],
            Types = [WorkflowTypes.Devops.Pipeline],
            Options = [BuildOptions.Inject.Param(nameof(NugetDryRun), true), BuildOptions.Devops.VariableGroup.Atom],
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
                new(nameof(ApproveDependabotPr))
                {
                    Options =
                    [
                        BuildOptions.Inject.Secret(nameof(GithubToken)),
                        BuildOptions.Inject.Param(nameof(PullRequestNumber),
                            TextExpressions.Github.GithubEvent["number"]),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            TextExpressions.Github.GithubActor.EqualToString("dependabot[bot]")),
                        new GithubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            IdTokens = PermissionsLevel.Write,
                            Contents = PermissionsLevel.Write,
                            PullRequests = PermissionsLevel.Write,
                        })),
                    ],
                },
            ],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}
