namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class GithubRunsOnOptions
    {
        // Linux x64 1x
        public GithubRunsOn Ubuntu_Slim =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_Slim],
            };

        // Linux x64 4x
        public GithubRunsOn Ubuntu_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_Latest],
            };

        public GithubRunsOn Ubuntu_24_04 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_24_04],
            };

        public GithubRunsOn Ubuntu_22_04 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_22_04],
            };

        // Linux ARM 4x
        public GithubRunsOn Ubuntu_24_04_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_24_04_Arm],
            };

        public GithubRunsOn Ubuntu_22_04_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Ubuntu_22_04_Arm],
            };

        // Windows x64 4x
        public GithubRunsOn Windows_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Windows_Latest],
            };

        public GithubRunsOn Windows_2025 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Windows_2025],
            };

        public GithubRunsOn Windows_2022 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Windows_2022],
            };

        // Windows ARM 4x
        public GithubRunsOn Windows_11_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.Windows_11_Arm],
            };

        // MacOS x64 4x
        public GithubRunsOn MacOs_13 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_13],
            };

        public GithubRunsOn MacOs_15_Intel =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_15_Intel],
            };

        // MacOS ARM 3x
        public GithubRunsOn MacOs_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_Latest],
            };

        public GithubRunsOn MacOs_26 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_26],
            };

        public GithubRunsOn MacOs_15 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_15],
            };

        public GithubRunsOn MacOs_14 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_14],
            };

        // MacOS x64 12x
        public GithubRunsOn MacOs_Latest_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_Latest_Large],
            };

        public GithubRunsOn MacOs_15_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_15_Large],
            };

        public GithubRunsOn MacOs_14_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_14_Large],
            };

        public GithubRunsOn MacOs_13_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_13_Large],
            };

        // MacOS ARM 5x
        public GithubRunsOn MacOs_Latest_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_Latest_XLarge],
            };

        public GithubRunsOn MacOs_26_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_26_XLarge],
            };

        public GithubRunsOn MacOs_15_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_15_XLarge],
            };

        public GithubRunsOn MacOs_14_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_14_XLarge],
            };

        public GithubRunsOn MacOs_13_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.Github.RunsOn.MacOs_13_XLarge],
            };

        public GithubRunsOn SetByMatrix { get; } = new()
        {
            Labels = ["${{ matrix.job-runs-on }}"],
        };

        public GithubRunsOn FromLabel(params WorkflowExpression[] labels) =>
            new()
            {
                Labels = labels,
            };

        public GithubRunsOn FromLabel(params string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(WorkflowExpressions.Literal)
                    .ToArray(),
            };

        public GithubRunsOn FromGroup(WorkflowExpression group) =>
            new()
            {
                Group = group,
            };

        public GithubRunsOn FromGroup(string group) =>
            new()
            {
                Group = group,
            };

        public GithubRunsOn From(WorkflowExpression? group, WorkflowExpression[] labels) =>
            new()
            {
                Labels = labels,
                Group = group,
            };

        public GithubRunsOn From(string group, string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(WorkflowExpressions.Literal)
                    .ToArray(),
                Group = group,
            };
    }

    [PublicAPI]
    public sealed class GithubTokenPermissionsOptions
    {
        public GithubTokenPermissionsOption NoneAll =>
            field ??= new()
            {
                Actions = GithubTokenPermission.None,
                Attestations = GithubTokenPermission.None,
                Checks = GithubTokenPermission.None,
                Contents = GithubTokenPermission.None,
                Deployments = GithubTokenPermission.None,
                IdToken = GithubTokenPermission.None,
                Issues = GithubTokenPermission.None,
                Discussions = GithubTokenPermission.None,
                Packages = GithubTokenPermission.None,
                Pages = GithubTokenPermission.None,
                PullRequests = GithubTokenPermission.None,
                SecurityEvents = GithubTokenPermission.None,
                Statuses = GithubTokenPermission.None,
            };

        public GithubTokenPermissionsOption ReadAll =>
            field ??= new()
            {
                Actions = GithubTokenPermission.Read,
                Attestations = GithubTokenPermission.Read,
                Checks = GithubTokenPermission.Read,
                Contents = GithubTokenPermission.Read,
                Deployments = GithubTokenPermission.Read,
                IdToken = GithubTokenPermission.Read,
                Issues = GithubTokenPermission.Read,
                Discussions = GithubTokenPermission.Read,
                Packages = GithubTokenPermission.Read,
                Pages = GithubTokenPermission.Read,
                PullRequests = GithubTokenPermission.Read,
                SecurityEvents = GithubTokenPermission.Read,
                Statuses = GithubTokenPermission.Read,
            };

        public GithubTokenPermissionsOption WriteAll =>
            field ??= new()
            {
                Actions = GithubTokenPermission.Write,
                Attestations = GithubTokenPermission.Write,
                Checks = GithubTokenPermission.Write,
                Contents = GithubTokenPermission.Write,
                Deployments = GithubTokenPermission.Write,
                IdToken = GithubTokenPermission.Write,
                Issues = GithubTokenPermission.Write,
                Discussions = GithubTokenPermission.Write,
                Packages = GithubTokenPermission.Write,
                Pages = GithubTokenPermission.Write,
                PullRequests = GithubTokenPermission.Write,
                SecurityEvents = GithubTokenPermission.Write,
                Statuses = GithubTokenPermission.Write,
            };

        public GithubTokenPermissionsOption Set(
            GithubTokenPermission? actions = null,
            GithubTokenPermission? attestations = null,
            GithubTokenPermission? checks = null,
            GithubTokenPermission? contents = null,
            GithubTokenPermission? deployments = null,
            GithubTokenPermission? idToken = null,
            GithubTokenPermission? issues = null,
            GithubTokenPermission? discussions = null,
            GithubTokenPermission? packages = null,
            GithubTokenPermission? pages = null,
            GithubTokenPermission? pullRequests = null,
            GithubTokenPermission? securityEvents = null,
            GithubTokenPermission? statuses = null) =>
            new()
            {
                Actions = actions,
                Attestations = attestations,
                Checks = checks,
                Contents = contents,
                Deployments = deployments,
                IdToken = idToken,
                Issues = issues,
                Discussions = discussions,
                Packages = packages,
                Pages = pages,
                PullRequests = pullRequests,
                SecurityEvents = securityEvents,
                Statuses = statuses,
            };
    }

    [PublicAPI]
    public sealed class Options
    {
        internal static Options Instance => field ??= new();

        public GithubRunsOnOptions RunsOn => field ??= new();

        public GithubTokenPermissionsOptions TokenPermissions => field ??= new();

        public GithubCheckoutOption ConfigureCheckout(
            string version = "v4",
            bool lfs = false,
            string? submodules = null,
            string? token = null) =>
            new(version, lfs, submodules, token);

        public GithubSnapshotImageOption CreateImageSnapshot(string imageName, string? version = null) =>
            new(imageName, version);
    }

    extension(WorkflowOptions)
    {
        [PublicAPI]
        public static Options Github => Options.Instance;
    }
}
