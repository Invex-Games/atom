namespace Invex.Atom.Module.GitHubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class GitHubRunsOnOptions
    {
        // Linux x64 1x
        public GitHubRunsOn Ubuntu_Slim =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_Slim],
            };

        // Linux x64 4x
        public GitHubRunsOn Ubuntu_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_Latest],
            };

        public GitHubRunsOn Ubuntu_24_04 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_24_04],
            };

        public GitHubRunsOn Ubuntu_22_04 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_22_04],
            };

        // Linux ARM 4x
        public GitHubRunsOn Ubuntu_24_04_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_24_04_Arm],
            };

        public GitHubRunsOn Ubuntu_22_04_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Ubuntu_22_04_Arm],
            };

        // Windows x64 4x
        public GitHubRunsOn Windows_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Windows_Latest],
            };

        public GitHubRunsOn Windows_2025 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Windows_2025],
            };

        public GitHubRunsOn Windows_2022 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Windows_2022],
            };

        // Windows ARM 4x
        public GitHubRunsOn Windows_11_Arm =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.Windows_11_Arm],
            };

        // MacOS x64 4x
        public GitHubRunsOn MacOS_15_Intel =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_15_Intel],
            };

        // MacOS ARM 3x
        public GitHubRunsOn MacOS_Latest =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_Latest],
            };

        public GitHubRunsOn MacOS_26 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_26],
            };

        public GitHubRunsOn MacOS_15 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_15],
            };

        public GitHubRunsOn MacOS_14 =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_14],
            };

        // MacOS x64 12x
        public GitHubRunsOn MacOS_Latest_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_Latest_Large],
            };

        public GitHubRunsOn MacOS_15_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_15_Large],
            };

        public GitHubRunsOn MacOS_14_Large =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_14_Large],
            };

        // MacOS ARM 5x
        public GitHubRunsOn MacOS_Latest_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_Latest_XLarge],
            };

        public GitHubRunsOn MacOS_26_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_26_XLarge],
            };

        public GitHubRunsOn MacOS_15_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_15_XLarge],
            };

        public GitHubRunsOn MacOS_14_XLarge =>
            field ??= new()
            {
                Labels = [WorkflowLabels.GitHub.RunsOn.MacOS_14_XLarge],
            };

        public GitHubRunsOn SetByMatrix { get; } = new()
        {
            Labels =
            [
                TextExpressions
                    .GitHub
                    .MatrixProperty("job-runs-on")
                    .Evaluate(),
            ],
        };

        public GitHubRunsOn FromLabel(params TextExpression[] labels) =>
            new()
            {
                Labels = labels,
            };

        public GitHubRunsOn FromLabel(params string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(TextExpressions.Raw)
                    .ToArray(),
            };

        public GitHubRunsOn FromGroup(TextExpression group) =>
            new()
            {
                Group = group,
            };

        public GitHubRunsOn FromGroup(string group) =>
            new()
            {
                Group = group,
            };

        public GitHubRunsOn From(TextExpression? group, TextExpression[] labels) =>
            new()
            {
                Labels = labels,
                Group = group,
            };

        public GitHubRunsOn From(string group, string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(TextExpressions.Raw)
                    .ToArray(),
                Group = group,
            };
    }

    [PublicAPI]
    public sealed class GitHubTokenPermissionsOptions
    {
        public GitHubTokenPermissionsOption NoneAll => new(new Permissions.All(PermissionsLevel.None));

        public GitHubTokenPermissionsOption ReadAll => new(new Permissions.All(PermissionsLevel.Read));

        public GitHubTokenPermissionsOption WriteAll => new(new Permissions.All(PermissionsLevel.Write));

        public GitHubTokenPermissionsOption Set(Permissions permissions) =>
            new(permissions);
    }

    /// <summary>
    ///     Creates GitHub Actions workflow concurrency options.
    /// </summary>
    [PublicAPI]
    public sealed class GitHubConcurrencyOptions
    {
        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GitHubConcurrencyOption Set(TextExpression group, TextExpression? cancelInProgress = null) =>
            new(group, cancelInProgress);

        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GitHubConcurrencyOption Set(TextExpression group, bool cancelInProgress) =>
            new(group, TextExpressions.From(cancelInProgress));

        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GitHubConcurrencyOption Set(string group, bool? cancelInProgress = null) =>
            new(TextExpressions.Raw(group),
                cancelInProgress.HasValue
                    ? TextExpressions.From(cancelInProgress.Value)
                    : null);
    }

    [PublicAPI]
    public sealed class GitHubDependabotOptions
    {
        public DependabotConfigOption Configure(
            StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig config) =>
            new(config);
    }

    [PublicAPI]
    public sealed class GitHubStepsOptions
    {
        public GitHubCheckoutStep Checkout(GitHubCheckoutStep step) =>
            step;
    }

    [PublicAPI]
    public sealed class GitHubOptions
    {
        internal static GitHubOptions Instance => field ??= new();

        public GitHubRunsOnOptions RunsOn => field ??= new();

        public GitHubTokenPermissionsOptions TokenPermissions => field ??= new();

        public GitHubConcurrencyOptions Concurrency => field ??= new();

        public GitHubDependabotOptions Dependabot => field ??= new();

        public GitHubStepsOptions Steps => field ??= new();
    }

    extension(BuildOptions)
    {
        [PublicAPI]
        public static GitHubOptions GitHub => GitHubOptions.Instance;
    }
}
