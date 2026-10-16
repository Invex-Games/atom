namespace Invex.Atom.Module.DevOpsWorkflows.Steps;

/// <summary>
///     Represents a workflow option for configuring the checkout step in Azure DevOps Pipelines.
/// </summary>
[PublicAPI]
public sealed record DevOpsCheckoutStep : CheckoutStep, IDevOpsAdditionalStepOption
{
    /// <summary>
    ///     Repository to check out. Valid values: "self" | "none" or repository resource name.
    /// </summary>
    public Template Repository { get; init; } = "self";

    /// <summary>
    ///     Whether to clean the repository before checkout.
    /// </summary>
    public Template? Clean { get; init; }

    /// <summary>
    ///     Number of commits to fetch (depth). 0 indicates all history.
    /// </summary>
    public Template? FetchDepth { get; init; }

    /// <summary>
    ///     Whether to download Git-LFS files.
    /// </summary>
    public Template? Lfs { get; init; }

    /// <summary>
    ///     Whether to checkout submodules.
    /// </summary>
    public Template? Submodules { get; init; }

    /// <summary>
    ///     Path to check out source code, relative to $(Agent.BuildDirectory).
    /// </summary>
    public Template? Path { get; init; }

    /// <summary>
    ///     Persist credentials for later use by the Git command-line tool.
    /// </summary>
    public Template? PersistCredentials { get; init; }

    /// <summary>
    ///     Evaluate this condition expression to determine whether to run this task.
    /// </summary>
    public BoolExpr? Condition { get; init; }

    /// <summary>
    ///     Continue running even on failure?
    /// </summary>
    public Template? ContinueOnError { get; init; }

    /// <summary>
    ///     Human-readable name for the task.
    /// </summary>
    public Template? DisplayName { get; init; }

    /// <summary>
    ///     Environment in which to run this task.
    /// </summary>
    public StepTarget? Target { get; init; }

    /// <summary>
    ///     Run this task when the job runs? Written as the task's <c>enabled</c> property. Not to be confused with
    ///     <see cref="CheckoutStep.Enabled" />, which controls whether a checkout step is added at all.
    /// </summary>
    public Template? TaskEnabled { get; init; }

    /// <summary>
    ///     Variables to map into the process's environment.
    /// </summary>
    public IReadOnlyDictionary<string, Template>? Env { get; init; }

    /// <summary>
    ///     ID of the step.
    /// </summary>
    public Template? Name { get; init; }

    /// <summary>
    ///     Time to wait for this task to complete before the server kills it.
    /// </summary>
    public Template? TimeoutInMinutes { get; init; }

    /// <summary>
    ///     Number of retries if the task fails.
    /// </summary>
    public Template? RetryCountOnTaskFailure { get; init; }

    public Step Build() =>
        new Step.Checkout
        {
            Repository = Repository,
            Clean = Clean,
            FetchDepth = FetchDepth,
            Lfs = Lfs,
            Submodules = Submodules,
            Path = Path,
            PersistCredentials = PersistCredentials,
            Condition = Condition,
            ContinueOnError = ContinueOnError,
            DisplayName = DisplayName,
            Target = Target,
            Enabled = TaskEnabled,
            Env = Env,
            Name = Name,
            TimeoutInMinutes = TimeoutInMinutes,
            RetryCountOnTaskFailure = RetryCountOnTaskFailure,
        };
}
