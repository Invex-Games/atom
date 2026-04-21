namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Jobs;

/// <summary>
///     A job represents a unit of work which can be assigned to a single agent or server.
/// </summary>
/// <remarks>
///     Supports three implementations:
///     - RegularJob: A collection of steps run by an agent or on a server
///     - Deployment: A special type of job for deployment scenarios
///     - Template: Reference to a job template
/// </remarks>
[PublicAPI]
[Union]
public partial record Job
{
    /// <summary>
    ///     A job is a collection of steps run by an agent or on a server.
    /// </summary>
    public sealed partial record RegularJob
    {
        /// <summary>
        ///     ID of the job.
        ///     Valid names may only contain alphanumeric characters and '_' and may not start with a number.
        /// </summary>
        public required WorkflowExpression JobId { get; init; }

        /// <summary>
        ///     Human-readable name for the job.
        /// </summary>
        public WorkflowExpression? DisplayName { get; init; }

        /// <summary>
        ///     Any jobs which must complete before this one.
        /// </summary>
        public WorkflowExpressionCollection? DependsOn { get; init; }

        /// <summary>
        ///     Evaluate this condition expression to determine whether to run this job.
        /// </summary>
        public WorkflowExpression? Condition { get; init; }

        /// <summary>
        ///     Continue running even on failure?
        /// </summary>
        public WorkflowExpression? ContinueOnError { get; init; }

        /// <summary>
        ///     Time to wait for this job to complete before the server kills it.
        /// </summary>
        public WorkflowExpression? TimeoutInMinutes { get; init; }

        /// <summary>
        ///     Time to wait for the job to cancel before forcibly terminating it.
        /// </summary>
        public WorkflowExpression? CancelTimeoutInMinutes { get; init; }

        /// <summary>
        ///     Job-specific variables.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Execution strategy for this job.
        /// </summary>
        public JobStrategy? Strategy { get; init; }

        /// <summary>
        ///     Pool where this job will run.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Container resource name.
        /// </summary>
        public JobContainer? Container { get; init; }

        /// <summary>
        ///     Container resources to run as a service container.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? Services { get; init; }

        /// <summary>
        ///     Workspace options on the agent.
        /// </summary>
        public Workspace? Workspace { get; init; }

        /// <summary>
        ///     Any resources required by this job that are not already referenced.
        /// </summary>
        public ExplicitResources? Uses { get; init; }

        /// <summary>
        ///     A list of steps to run.
        /// </summary>
        public IReadOnlyList<Step>? Steps { get; init; }

        /// <summary>
        ///     Job related information passed from a pipeline when extending a template.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? TemplateContext { get; init; }
    }

    /// <summary>
    ///     A deployment job is a special type of job for deployment scenarios.
    /// </summary>
    public sealed partial record Deployment
    {
        /// <summary>
        ///     ID of the deployment job.
        ///     Valid names may only contain alphanumeric characters and '_' and may not start with a number.
        /// </summary>
        public required WorkflowExpression DeploymentId { get; init; }

        /// <summary>
        ///     Human-readable name for the deployment.
        /// </summary>
        public WorkflowExpression? DisplayName { get; init; }

        /// <summary>
        ///     Any jobs which must complete before this one.
        /// </summary>
        public WorkflowExpressionCollection? DependsOn { get; init; }

        /// <summary>
        ///     Evaluate this condition expression to determine whether to run this deployment.
        /// </summary>
        public WorkflowExpression? Condition { get; init; }

        /// <summary>
        ///     Continue running even on failure?
        /// </summary>
        public WorkflowExpression? ContinueOnError { get; init; }

        /// <summary>
        ///     Time to wait for this job to complete before the server kills it.
        /// </summary>
        public WorkflowExpression? TimeoutInMinutes { get; init; }

        /// <summary>
        ///     Time to wait for the job to cancel before forcibly terminating it.
        /// </summary>
        public WorkflowExpression? CancelTimeoutInMinutes { get; init; }

        /// <summary>
        ///     Deployment-specific variables.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Pool where this deployment job will run.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Target environment for the deployment.
        /// </summary>
        public required DeploymentEnvironment Environment { get; init; }

        /// <summary>
        ///     Deployment strategy (runOnce, rolling, or canary).
        /// </summary>
        public required DeploymentStrategy Strategy { get; init; }

        /// <summary>
        ///     Container resource name.
        /// </summary>
        public JobContainer? Container { get; init; }

        /// <summary>
        ///     Container resources to run as a service container.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? Services { get; init; }

        /// <summary>
        ///     Workspace options on the agent.
        /// </summary>
        public Workspace? Workspace { get; init; }

        /// <summary>
        ///     Any resources required by this job that are not already referenced.
        /// </summary>
        public ExplicitResources? Uses { get; init; }

        /// <summary>
        ///     Deployment related information passed from a pipeline when extending a template.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? TemplateContext { get; init; }
    }

    /// <summary>
    ///     Reference to a job template.
    /// </summary>
    public sealed partial record Template
    {
        /// <summary>
        ///     Path to the template file.
        /// </summary>
        public required WorkflowExpression TemplatePath { get; init; }

        /// <summary>
        ///     Parameters to pass to the template.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? Parameters { get; init; }
    }
}
