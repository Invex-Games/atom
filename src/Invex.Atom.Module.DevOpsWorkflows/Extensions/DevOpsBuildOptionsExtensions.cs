namespace Invex.Atom.Module.DevOpsWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class DevOpsBuildOptionsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class DevOpsPoolOptions
    {
        // Windows
        public DevOpsPool Windows_2025_Vs2026 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Windows_2025_Vs2026,
            };

        public DevOpsPool Windows_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Windows_Latest,
            };

        public DevOpsPool Windows_2025 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Windows_2025,
            };

        public DevOpsPool Windows_2022 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Windows_2022,
            };

        // Linux
        public DevOpsPool Ubuntu_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Ubuntu_Latest,
            };

        public DevOpsPool Ubuntu_24_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Ubuntu_24_04,
            };

        public DevOpsPool Ubuntu_22_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.Ubuntu_22_04,
            };

        // MacOS
        public DevOpsPool MacOS_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.MacOS_Latest,
            };

        public DevOpsPool MacOS_15 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.MacOS_15,
            };

        public DevOpsPool MacOS_14 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.DevOps.Pool.MacOS_14,
            };

        public DevOpsPool SetByMatrix =>
            field ??= new()
            {
                HostedImage = "$(job-runs-on)",
            };

        public DevOpsPool FromName(TextExpression name) =>
            new()
            {
                Name = name,
            };

        public DevOpsPool FromName(string name) =>
            new()
            {
                Name = name,
            };

        public DevOpsPool FromHostedImage(TextExpression hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevOpsPool FromHostedImage(string hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevOpsPool FromDemands(params TextExpression[] demands) =>
            new()
            {
                Demands = demands,
            };

        public DevOpsPool FromDemands(params string[] demands) =>
            new()
            {
                Demands = demands
                    .Select(TextExpressions.Raw)
                    .ToArray(),
            };

        public DevOpsPool From(
            TextExpression? name = null,
            TextExpression? hostedImageName = null,
            IEnumerable<TextExpression>? demands = null) =>
            new()
            {
                Name = name,
                HostedImage = hostedImageName,
                Demands = demands?.ToArray() ?? [],
            };

        public DevOpsPool From(
            string? name = null,
            string? hostedImageName = null,
            IEnumerable<string>? demands = null) =>
            new()
            {
                Name = name is null
                    ? null
                    : TextExpressions.Raw(name),
                HostedImage = hostedImageName is null
                    ? null
                    : TextExpressions.Raw(hostedImageName),
                Demands = demands
                    ?.Select(TextExpressions.Raw)
                    .ToArray() ?? [],
            };
    }

    [PublicAPI]
    public sealed class DevOpsVariableGroupOptions
    {
        public DevOpsVariableGroup Atom => field ??= new("Atom");
    }

    [PublicAPI]
    public sealed class DevOpsStepsOptions
    {
        public DevOpsCheckoutStep Checkout(DevOpsCheckoutStep step) =>
            step;
    }

    /// <summary>
    ///     Creates Azure Pipelines concurrency options.
    /// </summary>
    [PublicAPI]
    public sealed class DevOpsConcurrencyOptions
    {
        /// <summary>
        ///     Runs only the latest pipeline waiting on an exclusive lock, cancelling earlier waiting runs.
        /// </summary>
        public DevOpsConcurrencyOption RunLatest => field ??= new(TextExpressions.Raw("runLatest"));

        /// <summary>
        ///     Runs every pipeline waiting on an exclusive lock in sequence.
        /// </summary>
        public DevOpsConcurrencyOption Sequential => field ??= new(TextExpressions.Raw("sequential"));
    }

    /// <summary>
    ///     Creates Azure Pipelines pull request options.
    /// </summary>
    [PublicAPI]
    public sealed class DevOpsPullRequestOptions
    {
        /// <summary>
        ///     Cancels an in-progress pull request run when a new commit is pushed to the pull request.
        /// </summary>
        public DevOpsPullRequestOption AutoCancel => field ??= new(true);

        /// <summary>
        ///     Allows in-progress pull request runs to continue when new commits are pushed.
        /// </summary>
        public DevOpsPullRequestOption KeepRunning => field ??= new(false);
    }

    [PublicAPI]
    public sealed class DevOpsOptions
    {
        internal static DevOpsOptions Instance => field ??= new();

        public DevOpsPoolOptions DevOpsPool => field ??= new();

        public DevOpsVariableGroupOptions VariableGroup => field ??= new();

        public DevOpsStepsOptions Steps => field ??= new();

        /// <summary>
        ///     Gets options that control how pipeline runs wait on exclusive locks.
        /// </summary>
        public DevOpsConcurrencyOptions Concurrency => field ??= new();

        /// <summary>
        ///     Gets options that control pull request pipeline behavior.
        /// </summary>
        public DevOpsPullRequestOptions PullRequest => field ??= new();

        public ProvideDevOpsRunIdAsWorkflowId ProvideDevOpsRunIdAsWorkflowId => field ??= new();
    }

    extension(BuildOptions)
    {
        [PublicAPI]
        public static DevOpsOptions DevOps => DevOpsOptions.Instance;
    }
}
