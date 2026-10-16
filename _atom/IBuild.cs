namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
[GenerateSolutionModel]
internal interface IBuild : IWorkflowBuildDefinition,
    IDotnetUserSecrets,
    IDevOpsWorkflows,
    IGitHubWorkflows,
    IGitVersion,
    IBuildTargets,
    ITestTargets,
    IDeployTargets,
    IApproveDependabotPr,
    ICheckPrForBreakingChanges,
    IDocTargets,
    IWaitForCopilotReview
{
    static readonly string[] PlatformNames =
    [
        WorkflowLabels.GitHub.RunsOn.Windows_Latest,
        WorkflowLabels.GitHub.RunsOn.Windows_11_Arm,
        WorkflowLabels.GitHub.RunsOn.Ubuntu_Latest,
        WorkflowLabels.GitHub.RunsOn.Ubuntu_24_04_Arm,
        WorkflowLabels.GitHub.RunsOn.MacOS_15_Intel,
        WorkflowLabels.GitHub.RunsOn.MacOS_Latest,
    ];

    static readonly string[] DevOpsPlatformNames =
    [
        WorkflowLabels.DevOps.Pool.Windows_Latest,
        WorkflowLabels.DevOps.Pool.Ubuntu_Latest,
        WorkflowLabels.DevOps.Pool.MacOS_Latest,
    ];

    static readonly string[] FrameworkNames =
    [
        WorkflowLabels.Dotnet.Framework.Net_10_0,
    ];

    static readonly IBuildOption CancelSupersededWorkflowRuns = BuildOptions.GitHub.Concurrency.Set(
        $"{Tex.GitHub.Workflow}-{Tex.GitHub.Ref}",
        true);

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
                            Values = [..PlatformNames],
                        },
                    ],
                    Options = [BuildOptions.Target.SuppressArtifactPublishing, BuildOptions.GitHub.RunsOn.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = [..PlatformNames],
                        },
                        new(nameof(TestFramework))
                        {
                            Values = [..FrameworkNames],
                        },
                    ],
                    Options =
                    [
                        BuildOptions.Target.SuppressArtifactPublishing,
                        BuildOptions.GitHub.RunsOn.SetByMatrix,
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
                        BuildOptions.Inject.Secret(nameof(GitHubToken)),
                        BuildOptions.GitHub.TokenPermissions.Set(new Permissions.Exact(new()
                        {
                            IdTokens = PermissionsLevel.Write,
                            Contents = PermissionsLevel.Write,
                            PullRequests = PermissionsLevel.Write,
                            Checks = PermissionsLevel.Write,
                        })),
                        BuildOptions.Inject.Param(nameof(PullRequestNumber),
                            Tex.GitHub.Event["number"]),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            Tex.GitHub.EventName.Eq("pull_request")),
                    ],
                },
                new(nameof(WaitForCopilotReview))
                {
                    Options =
                    [
                        BuildOptions.Inject.Secret(nameof(GitHubToken)),
                        BuildOptions.Inject.Param(nameof(PullRequestNumber),
                            Tex.GitHub.Event["number"]),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
            Options = [CancelSupersededWorkflowRuns],
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
                new GitHubTrigger(new On.Release([On.Release.ReleaseType.released])),
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
                            Values = [..PlatformNames],
                        },
                    ],
                    Options = [BuildOptions.GitHub.RunsOn.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = [..PlatformNames],
                        },
                        new(nameof(TestFramework))
                        {
                            Values = [..FrameworkNames],
                        },
                    ],
                    Options =
                    [
                        BuildOptions.GitHub.RunsOn.SetByMatrix,
                    ],
                },
                new(nameof(BuildDocs)),
                new(nameof(PublishDocs))
                {
                    Options =
                    [
                        BuildOptions.Inject.Secret(nameof(GitHubToken)),
                        new GitHubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            Contents = PermissionsLevel.Write,
                        })),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            !Tex.ParamOutput(this, nameof(SetupBuildInfo), nameof(BuildVersion)).Contains("-")),
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
                        BuildOptions.Inject.Secret(nameof(GitHubToken)),
                        new GitHubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            Contents = PermissionsLevel.Write,
                        })),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            !Tex.ParamOutput(this, nameof(SetupBuildInfo), nameof(BuildVersion)).Contains("-")),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
            Options = [CancelSupersededWorkflowRuns],
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
                            Values = [..DevOpsPlatformNames],
                        },
                    ],
                    Options = [BuildOptions.DevOps.DevOpsPool.SetByMatrix],
                },
                new(nameof(TestProjects))
                {
                    MatrixDimensions =
                    [
                        new(nameof(JobRunsOn))
                        {
                            Values = [..DevOpsPlatformNames],
                        },
                        new(nameof(TestFramework))
                        {
                            Values = [..FrameworkNames],
                        },
                    ],
                    Options =
                    [
                        BuildOptions.DevOps.DevOpsPool.SetByMatrix,
                    ],
                },
                new(nameof(PushToNugetDevops))
                {
                    Options = [BuildOptions.Inject.Secret(nameof(NugetApiKey))],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
            Options =
            [
                BuildOptions.Inject.Param(nameof(NugetDryRun), true),
                BuildOptions.DevOps.VariableGroup.Atom,
                BuildOptions.DevOps.PullRequest.AutoCancel,
            ],
        },

        // Dependabot
        WorkflowPresets.GitHub.Dependabot(new()
        {
            Registries = new Dictionary<string, DependabotRegistry>
            {
                ["nuget"] = new()
                {
                    Type = RegistryType.NugetFeed,
                    Url = WorkflowLabels.GitHub.Dependabot.NugetUrl,
                },
            },
            Updates =
            [
                new()
                {
                    Directory = "/",
                    PackageEcosystem = WorkflowLabels.GitHub.Dependabot.NugetEcosystem,
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
                        Interval = ScheduleInterval.Weekly,
                        Day = ScheduleDay.Saturday,
                        Time = "00:00",
                        Timezone = "Australia/Brisbane",
                    },
                    TargetBranch = "main",
                    OpenPullRequestsLimit = 10,
                    Ignore =
                    [
                        // Verify 33 adds a maintenance-fee SponsorCheck that fails test builds (SC021).
                        new()
                        {
                            DependencyName = "Verify.NUnit",
                            UpdateTypes = [SemverUpdateType.VersionUpdateSemverMajor],
                            Versions = null,
                        },
                    ],
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
                        BuildOptions.Inject.Secret(nameof(GitHubToken)),
                        BuildOptions.Inject.Secret(nameof(DependabotEnableAutoMergePat)),
                        BuildOptions.Inject.Param(nameof(PullRequestNumber),
                            Tex.GitHub.Event["number"]),
                        BuildOptions.Target.RunIfWorkflowCondition(
                            Tex.GitHub.Actor.Eq("dependabot[bot]")),
                        new GitHubTokenPermissionsOption(new Permissions.Exact(new()
                        {
                            IdTokens = PermissionsLevel.Write,
                            Contents = PermissionsLevel.Write,
                            PullRequests = PermissionsLevel.Write,
                        })),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
            Options = [CancelSupersededWorkflowRuns],
        },
    ];
}
