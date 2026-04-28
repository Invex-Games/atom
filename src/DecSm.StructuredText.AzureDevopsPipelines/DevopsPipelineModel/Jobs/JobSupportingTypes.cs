namespace DecSm.StructuredText.AzureDevopsPipelines.DevopsPipelineModel.Jobs;

/// <summary>
///     Execution strategy for a job.
/// </summary>
[PublicAPI]
public sealed record JobStrategy
{
    /// <summary>
    ///     Specifies the execution strategy as a matrix of named variable combinations.
    ///     Each key is a combination name (e.g., "001_windows-latest"), and each value is a dictionary
    ///     of variable name to variable value for that combination.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TextExpression>>? Matrix { get; init; }

    /// <summary>
    ///     Maximum number of jobs to run in parallel.
    /// </summary>
    public WorkflowExpression<double>? MaxParallel { get; init; }

    /// <summary>
    ///     Specifies whether to run the job in parallel.
    /// </summary>
    public WorkflowExpression<double>? Parallel { get; init; }
}

/// <summary>
///     Container resource for a job.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - ContainerName: Simple string reference to a container resource
///     - ContainerSpec: Full specification with image, options, endpoint, etc.
/// </remarks>
[PublicAPI]
[Union]
public partial record JobContainer
{
    /// <summary>
    ///     Reference to a container resource by name.
    /// </summary>
    public sealed partial record ContainerName
    {
        /// <summary>
        ///     Name of the container resource.
        /// </summary>
        public required WorkflowExpression<string> Name { get; init; }
    }

    /// <summary>
    ///     Full container specification.
    /// </summary>
    public sealed partial record ContainerSpec
    {
        /// <summary>
        ///     Container image name.
        /// </summary>
        public required WorkflowExpression<string> Image { get; init; }

        /// <summary>
        ///     Container options.
        /// </summary>
        public WorkflowExpression<string>? Options { get; init; }

        /// <summary>
        ///     Service endpoint for the container registry.
        /// </summary>
        public WorkflowExpression<string>? Endpoint { get; init; }

        /// <summary>
        ///     Environment variables to set in the container.
        /// </summary>
        public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

        /// <summary>
        ///     Ports to expose on the container.
        /// </summary>
        public WorkflowExpressionCollection<string>? Ports { get; init; }

        /// <summary>
        ///     Volumes to mount in the container.
        /// </summary>
        public WorkflowExpressionCollection<string>? Volumes { get; init; }

        /// <summary>
        ///     Whether to mount the workspace as read-only.
        /// </summary>
        public WorkflowExpression<bool>? MapDockerSocket { get; init; }
    }
}

/// <summary>
///     Deployment environment specification.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - EnvironmentName: Simple string environment name
///     - EnvironmentSpec: Full specification with name, resource name, and resource type
/// </remarks>
[PublicAPI]
[Union]
public partial record DeploymentEnvironment
{
    /// <summary>
    ///     Simple environment name.
    /// </summary>
    public sealed partial record EnvironmentName
    {
        /// <summary>
        ///     Name of the environment.
        /// </summary>
        public required WorkflowExpression<string> Name { get; init; }
    }

    /// <summary>
    ///     Full environment specification.
    /// </summary>
    public sealed partial record EnvironmentSpec
    {
        /// <summary>
        ///     Name of the environment.
        /// </summary>
        public required WorkflowExpression<string> Name { get; init; }

        /// <summary>
        ///     Resource name within the environment.
        /// </summary>
        public WorkflowExpression<string>? ResourceName { get; init; }

        /// <summary>
        ///     Resource type within the environment.
        /// </summary>
        public WorkflowExpression<string>? ResourceType { get; init; }

        /// <summary>
        ///     Resource ID within the environment.
        /// </summary>
        public WorkflowExpression<string>? ResourceId { get; init; }

        /// <summary>
        ///     Tags for the environment.
        /// </summary>
        public WorkflowExpressionCollection<string>? Tags { get; init; }
    }
}

/// <summary>
///     Deployment strategy for a deployment job.
/// </summary>
/// <remarks>
///     Supports three implementations:
///     - RunOnce: Simple run-once deployment strategy
///     - Rolling: Rolling deployment strategy
///     - Canary: Canary deployment strategy
/// </remarks>
[PublicAPI]
[Union]
public partial record DeploymentStrategy
{
    /// <summary>
    ///     Run-once deployment strategy.
    /// </summary>
    public sealed partial record RunOnce
    {
        /// <summary>
        ///     Pre-deployment steps.
        /// </summary>
        public DeploymentHook? PreDeploy { get; init; }

        /// <summary>
        ///     Deployment steps.
        /// </summary>
        public DeploymentHook? Deploy { get; init; }

        /// <summary>
        ///     Route traffic steps.
        /// </summary>
        public DeploymentHook? RouteTraffic { get; init; }

        /// <summary>
        ///     Post-route traffic steps.
        /// </summary>
        public DeploymentHook? PostRouteTraffic { get; init; }

        /// <summary>
        ///     Steps to run on success.
        /// </summary>
        public DeploymentHook? OnSuccess { get; init; }

        /// <summary>
        ///     Steps to run on failure.
        /// </summary>
        public DeploymentHook? OnFailure { get; init; }
    }

    /// <summary>
    ///     Rolling deployment strategy.
    /// </summary>
    public sealed partial record Rolling
    {
        /// <summary>
        ///     Maximum percentage or number of targets to deploy to in parallel.
        /// </summary>
        public WorkflowExpression<double>? MaxParallel { get; init; }

        /// <summary>
        ///     Pre-deployment steps.
        /// </summary>
        public DeploymentHook? PreDeploy { get; init; }

        /// <summary>
        ///     Deployment steps.
        /// </summary>
        public DeploymentHook? Deploy { get; init; }

        /// <summary>
        ///     Route traffic steps.
        /// </summary>
        public DeploymentHook? RouteTraffic { get; init; }

        /// <summary>
        ///     Post-route traffic steps.
        /// </summary>
        public DeploymentHook? PostRouteTraffic { get; init; }

        /// <summary>
        ///     Steps to run on success.
        /// </summary>
        public DeploymentHook? OnSuccess { get; init; }

        /// <summary>
        ///     Steps to run on failure.
        /// </summary>
        public DeploymentHook? OnFailure { get; init; }
    }

    /// <summary>
    ///     Canary deployment strategy.
    /// </summary>
    public sealed partial record Canary
    {
        /// <summary>
        ///     Increments in which to roll out the canary deployment.
        /// </summary>
        public required WorkflowExpressionCollection<double> Increments { get; init; }

        /// <summary>
        ///     Pre-deployment steps.
        /// </summary>
        public DeploymentHook? PreDeploy { get; init; }

        /// <summary>
        ///     Deployment steps.
        /// </summary>
        public DeploymentHook? Deploy { get; init; }

        /// <summary>
        ///     Route traffic steps.
        /// </summary>
        public DeploymentHook? RouteTraffic { get; init; }

        /// <summary>
        ///     Post-route traffic steps.
        /// </summary>
        public DeploymentHook? PostRouteTraffic { get; init; }

        /// <summary>
        ///     Steps to run on success.
        /// </summary>
        public DeploymentHook? OnSuccess { get; init; }

        /// <summary>
        ///     Steps to run on failure.
        /// </summary>
        public DeploymentHook? OnFailure { get; init; }
    }
}

/// <summary>
///     Deployment lifecycle hook containing steps.
/// </summary>
[PublicAPI]
public sealed record DeploymentHook
{
    /// <summary>
    ///     Steps to run in this hook.
    /// </summary>
    public IReadOnlyList<Step>? Steps { get; init; }

    /// <summary>
    ///     Pool where steps in this hook will run.
    /// </summary>
    public Pool? Pool { get; init; }
}

/// <summary>
///     Explicit resource references for a job.
/// </summary>
[PublicAPI]
public sealed record ExplicitResources
{
    /// <summary>
    ///     Repository references.
    /// </summary>
    public WorkflowExpressionCollection<string>? Repositories { get; init; }

    /// <summary>
    ///     Pool references.
    /// </summary>
    public WorkflowExpressionCollection<string>? Pools { get; init; }
}
