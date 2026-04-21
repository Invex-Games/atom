namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Steps;

/// <summary>
///     Represents a workflow option for configuring the checkout step in Azure DevOps Pipelines.
/// </summary>
[PublicAPI]
public sealed record DevopsCheckoutStep : CheckoutStep, IDevopsAdditionalStepOption
{
    /// <summary>
    ///     Repository to check out. Valid values: "self" | "none" or repository resource name.
    /// </summary>
    public required WorkflowExpression Repository { get; init; }

    /// <summary>
    ///     Whether to clean the repository before checkout.
    /// </summary>
    public WorkflowExpression? Clean { get; init; }

    /// <summary>
    ///     Number of commits to fetch (depth). 0 indicates all history.
    /// </summary>
    public WorkflowExpression? FetchDepth { get; init; }

    /// <summary>
    ///     Whether to download Git-LFS files.
    /// </summary>
    public WorkflowExpression? Lfs { get; init; }

    /// <summary>
    ///     Whether to checkout submodules.
    /// </summary>
    public WorkflowExpression? Submodules { get; init; }

    /// <summary>
    ///     Path to check out source code, relative to $(Agent.BuildDirectory).
    /// </summary>
    public WorkflowExpression? Path { get; init; }

    /// <summary>
    ///     Persist credentials for later use by the Git command-line tool.
    /// </summary>
    public WorkflowExpression? PersistCredentials { get; init; }

    /// <summary>
    ///     Evaluate this condition expression to determine whether to run this task.
    /// </summary>
    public WorkflowExpression? Condition { get; init; }

    /// <summary>
    ///     Continue running even on failure?
    /// </summary>
    public WorkflowExpression? ContinueOnError { get; init; }

    /// <summary>
    ///     Human-readable name for the task.
    /// </summary>
    public WorkflowExpression? DisplayName { get; init; }

    /// <summary>
    ///     Environment in which to run this task.
    /// </summary>
    public StepTarget? Target { get; init; }

    /// <summary>
    ///     Run this task when the job runs?
    /// </summary>
    public WorkflowExpression? Enabled { get; init; }

    /// <summary>
    ///     Variables to map into the process's environment.
    /// </summary>
    public IReadOnlyDictionary<string, WorkflowExpression>? Env { get; init; }

    /// <summary>
    ///     ID of the step.
    /// </summary>
    public WorkflowExpression? Name { get; init; }

    /// <summary>
    ///     Time to wait for this task to complete before the server kills it.
    /// </summary>
    public WorkflowExpression? TimeoutInMinutes { get; init; }

    /// <summary>
    ///     Number of retries if the task fails.
    /// </summary>
    public WorkflowExpression? RetryCountOnTaskFailure { get; init; }

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
            Enabled = Enabled,
            Env = Env,
            Name = Name,
            TimeoutInMinutes = TimeoutInMinutes,
            RetryCountOnTaskFailure = RetryCountOnTaskFailure,
        };
}
