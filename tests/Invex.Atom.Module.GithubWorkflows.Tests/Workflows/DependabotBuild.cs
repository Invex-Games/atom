namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependabotBuild : WorkflowBuildDefinition, IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        WorkflowPresets.Github.Dependabot(new()
        {
            Registries = new Dictionary<string, DependabotRegistry>
            {
                ["registry-1"] = new()
                {
                    Type = RegistryType.CargoRegistry,
                    Url = "url1",
                },
            },
            Updates =
            [
                new()
                {
                    Allow =
                    [
                        new()
                        {
                            DependencyName = "dep-name",
                            DependencyType = DependencyType.All,
                        },
                    ],
                    Assignees = ["assignee1"],
                    CommitMessage = new()
                    {
                        Include = CommitMessageInclude.Scope,
                        Prefix = "prefix",
                        PrefixDevelopment = "prefix-dev",
                    },
                    Cooldown = new()
                    {
                        DefaultDays = 1,
                        SemverMajorDays = 2,
                        SemverMinorDays = 3,
                        SemverPatchDays = 4,
                        Include = ["include1"],
                        Exclude = ["exclude1"],
                    },
                    Directories = ["directory1", "directory2"],
                    Directory = "directory",
                    ExcludePaths = ["exclude-path1", "exclude-path2"],
                    Groups = new Dictionary<string, DependabotGroup>
                    {
                        ["group-1"] = new DependabotGroup.FromPatterns
                        {
                            AppliesTo = GroupAppliesTo.SecurityUpdates,
                            DependencyType = GroupDependencyType.Production,
                            ExcludePatterns = ["exclude-pattern-1", "exclude-pattern-2"],
                            UpdateTypes = [GroupUpdateType.Major, GroupUpdateType.Minor],
                            GroupBy = GroupBy.DependencyName,
                            Patterns = ["pattern-1", "pattern-2"],
                        },
                    },
                    Ignore =
                    [
                        new()
                        {
                            DependencyName = "dep-name",
                            UpdateTypes = [SemverUpdateType.VersionUpdateSemverMajor],
                            Versions = new DependabotVersions.Multiple(["1.0.0", "2.0.0"]),
                        },
                    ],
                    InsecureExternalCodeExecution = InsecureExternalCodeExecution.Allow,
                    Labels = ["label1", "label2"],
                    Milestone = 1,
                    Name = "update-deps",
                    OpenPullRequestsLimit = 5,
                    PackageEcosystem = "package-ecosystem-1",
                    PullRequestBranchName = new()
                    {
                        Separator = BranchNameSeparator.Hyphen,
                    },
                    RebaseStrategy = RebaseStrategy.Auto,
                    Registries = new DependabotRegistries.All(),
                    Schedule = new()
                    {
                        Interval = ScheduleInterval.Daily,
                        Day = ScheduleDay.Monday,
                        Time = "02:00",
                        Timezone = "UTC",
                    },
                    TargetBranch = "main",
                    Vendor = true,
                    VersioningStrategy = VersioningStrategy.Increase,
                    Patterns = ["pattern1", "pattern2"],
                    MultiEcosystemGroup = "multi-ecosystem-group",
                },
            ],
        }),
    ];
}
