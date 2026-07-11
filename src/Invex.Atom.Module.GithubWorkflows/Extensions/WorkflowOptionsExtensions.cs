namespace Invex.Atom.Module.GithubWorkflows.Extensions;

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
            Labels =
            [
                TextExpressions
                    .Github
                    .MatrixProperty("job-runs-on")
                    .Evaluate(),
            ],
        };

        public GithubRunsOn FromLabel(params TextExpression[] labels) =>
            new()
            {
                Labels = labels,
            };

        public GithubRunsOn FromLabel(params string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(TextExpressions.Raw)
                    .ToArray(),
            };

        public GithubRunsOn FromGroup(TextExpression group) =>
            new()
            {
                Group = group,
            };

        public GithubRunsOn FromGroup(string group) =>
            new()
            {
                Group = group,
            };

        public GithubRunsOn From(TextExpression? group, TextExpression[] labels) =>
            new()
            {
                Labels = labels,
                Group = group,
            };

        public GithubRunsOn From(string group, string[] labels) =>
            new()
            {
                Labels = labels
                    .Select(TextExpressions.Raw)
                    .ToArray(),
                Group = group,
            };
    }

    [PublicAPI]
    public sealed class GithubTokenPermissionsOptions
    {
        public GithubTokenPermissionsOption NoneAll => new(new Permissions.All(PermissionsLevel.None));

        public GithubTokenPermissionsOption ReadAll => new(new Permissions.All(PermissionsLevel.Read));

        public GithubTokenPermissionsOption WriteAll => new(new Permissions.All(PermissionsLevel.Write));

        public GithubTokenPermissionsOption Set(Permissions permissions) =>
            new(permissions);
    }

    /// <summary>
    ///     Creates GitHub Actions workflow concurrency options.
    /// </summary>
    [PublicAPI]
    public sealed class GithubConcurrencyOptions
    {
        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GithubConcurrencyOption Set(TextExpression group, TextExpression? cancelInProgress = null) =>
            new(group, cancelInProgress);

        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GithubConcurrencyOption Set(TextExpression group, bool cancelInProgress) =>
            new(group, TextExpressions.From(cancelInProgress));

        /// <summary>
        ///     Configures a workflow concurrency group.
        /// </summary>
        /// <param name="group">The concurrency group key.</param>
        /// <param name="cancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
        /// <returns>The workflow concurrency option.</returns>
        public GithubConcurrencyOption Set(string group, bool? cancelInProgress = null) =>
            new(TextExpressions.Raw(group),
                cancelInProgress.HasValue
                    ? TextExpressions.From(cancelInProgress.Value)
                    : null);
    }

    [PublicAPI]
    public sealed class GithubDependabotOptions
    {
        public DependabotConfigOption Configure(
            StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig config) =>
            new(config);
    }

    [PublicAPI]
    public sealed class GithubStepsOptions
    {
        public GithubCheckoutStep Checkout(GithubCheckoutStep step) =>
            step;
    }

    [PublicAPI]
    public sealed class GithubOptions
    {
        internal static GithubOptions Instance => field ??= new();

        public GithubRunsOnOptions RunsOn => field ??= new();

        public GithubTokenPermissionsOptions TokenPermissions => field ??= new();

        public GithubConcurrencyOptions Concurrency => field ??= new();

        public GithubDependabotOptions Dependabot => field ??= new();

        public GithubStepsOptions Steps => field ??= new();
    }

    extension(BuildOptions)
    {
        [PublicAPI]
        public static GithubOptions Github => GithubOptions.Instance;
    }
}
