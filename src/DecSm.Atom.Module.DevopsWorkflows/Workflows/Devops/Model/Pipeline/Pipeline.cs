namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Pipeline;

/// <summary>
///     A pipeline is one or more stages that describe a CI/CD process.
/// </summary>
/// <remarks>
///     Pipeline supports four distinct implementations:
///     - Stages: Pipeline with multiple stages
///     - Extends: Pipeline that extends a template
///     - Jobs: Pipeline with jobs and one implicit stage
///     - Steps: Pipeline with steps and one implicit job
/// </remarks>
[PublicAPI]
[Union]
public partial record Pipeline
{
    /// <summary>
    ///     Pipeline with stages - the full multi-stage syntax.
    /// </summary>
    public sealed partial record PipelineWithStages
    {
        /// <summary>
        ///     Stages are groups of jobs that can run without human intervention.
        /// </summary>
        public required IReadOnlyList<Stage> Stages { get; init; }

        /// <summary>
        ///     Pool where jobs in this pipeline will run unless otherwise specified.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Pipeline run number.
        /// </summary>
        public WorkflowExpression<string>? Name { get; init; }

        /// <summary>
        ///     Append the commit message to the build number. The default is true.
        /// </summary>
        public WorkflowExpression<bool>? AppendCommitMessageToRunName { get; init; }

        /// <summary>
        ///     Continuous integration triggers.
        /// </summary>
        public Trigger? Trigger { get; init; }

        /// <summary>
        ///     Pipeline template parameters.
        /// </summary>
        public IReadOnlyList<Parameter>? Parameters { get; init; }

        /// <summary>
        ///     Pull request triggers.
        /// </summary>
        public Pr? Pr { get; init; }

        /// <summary>
        ///     Scheduled triggers.
        /// </summary>
        public IReadOnlyList<Schedule>? Schedules { get; init; }

        /// <summary>
        ///     Containers and repositories used in the build.
        /// </summary>
        public Resources.Resources? PipelineResources { get; init; }

        /// <summary>
        ///     Variables for this pipeline.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Behavior lock requests from this stage should exhibit in relation to other exclusive lock requests.
        ///     Valid values: "sequential" | "runLatest"
        /// </summary>
        public WorkflowExpression<string>? LockBehavior { get; init; }
    }

    /// <summary>
    ///     Pipeline that extends a template.
    /// </summary>
    public sealed partial record PipelineWithExtends
    {
        /// <summary>
        ///     Extends a template.
        /// </summary>
        public required Extends Extends { get; init; }

        /// <summary>
        ///     Pool where jobs in this pipeline will run unless otherwise specified.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Pipeline run number.
        /// </summary>
        public WorkflowExpression<string>? Name { get; init; }

        /// <summary>
        ///     Append the commit message to the build number. The default is true.
        /// </summary>
        public WorkflowExpression<bool>? AppendCommitMessageToRunName { get; init; }

        /// <summary>
        ///     Continuous integration triggers.
        /// </summary>
        public Trigger? Trigger { get; init; }

        /// <summary>
        ///     Pipeline template parameters.
        /// </summary>
        public IReadOnlyList<Parameter>? Parameters { get; init; }

        /// <summary>
        ///     Pull request triggers.
        /// </summary>
        public Pr? Pr { get; init; }

        /// <summary>
        ///     Scheduled triggers.
        /// </summary>
        public IReadOnlyList<Schedule>? Schedules { get; init; }

        /// <summary>
        ///     Containers and repositories used in the build.
        /// </summary>
        public Resources.Resources? PipelineResources { get; init; }

        /// <summary>
        ///     Variables for this pipeline.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Behavior lock requests from this stage should exhibit in relation to other exclusive lock requests.
        ///     Valid values: "sequential" | "runLatest"
        /// </summary>
        public WorkflowExpression<string>? LockBehavior { get; init; }
    }

    /// <summary>
    ///     Pipeline with jobs and one implicit stage.
    /// </summary>
    public sealed partial record PipelineWithJobs
    {
        /// <summary>
        ///     Jobs represent units of work which can be assigned to a single agent or server.
        /// </summary>
        public required IReadOnlyList<Job> Jobs { get; init; }

        /// <summary>
        ///     Pool where jobs in this pipeline will run unless otherwise specified.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Pipeline run number.
        /// </summary>
        public WorkflowExpression<string>? Name { get; init; }

        /// <summary>
        ///     Append the commit message to the build number. The default is true.
        /// </summary>
        public WorkflowExpression<bool>? AppendCommitMessageToRunName { get; init; }

        /// <summary>
        ///     Continuous integration triggers.
        /// </summary>
        public Trigger? Trigger { get; init; }

        /// <summary>
        ///     Pipeline template parameters.
        /// </summary>
        public IReadOnlyList<Parameter>? Parameters { get; init; }

        /// <summary>
        ///     Pull request triggers.
        /// </summary>
        public Pr? Pr { get; init; }

        /// <summary>
        ///     Scheduled triggers.
        /// </summary>
        public IReadOnlyList<Schedule>? Schedules { get; init; }

        /// <summary>
        ///     Containers and repositories used in the build.
        /// </summary>
        public Resources.Resources? Resources { get; init; }

        /// <summary>
        ///     Variables for this pipeline.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Behavior lock requests from this stage should exhibit in relation to other exclusive lock requests.
        ///     Valid values: "sequential" | "runLatest"
        /// </summary>
        public WorkflowExpression<string>? LockBehavior { get; init; }
    }

    /// <summary>
    ///     Pipeline with steps and one implicit job.
    /// </summary>
    public sealed partial record PipelineWithSteps
    {
        /// <summary>
        ///     A list of steps to run in this job.
        /// </summary>
        public required IReadOnlyList<Step> Steps { get; init; }

        /// <summary>
        ///     Execution strategy for this job.
        /// </summary>
        public JobStrategy? Strategy { get; init; }

        /// <summary>
        ///     Continue running even on failure?
        /// </summary>
        public WorkflowExpression<bool>? ContinueOnError { get; init; }

        /// <summary>
        ///     Pool where jobs in this pipeline will run unless otherwise specified.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Container resource name.
        /// </summary>
        public JobContainer? Container { get; init; }

        /// <summary>
        ///     Container resources to run as a service container.
        /// </summary>
        public IReadOnlyDictionary<string, TextExpression>? Services { get; init; }

        /// <summary>
        ///     Workspace options on the agent.
        /// </summary>
        public Workspace? Workspace { get; init; }

        /// <summary>
        ///     Pipeline run number.
        /// </summary>
        public WorkflowExpression<string>? Name { get; init; }

        /// <summary>
        ///     Append the commit message to the build number. The default is true.
        /// </summary>
        public WorkflowExpression<bool>? AppendCommitMessageToRunName { get; init; }

        /// <summary>
        ///     Continuous integration triggers.
        /// </summary>
        public Trigger? Trigger { get; init; }

        /// <summary>
        ///     Pipeline template parameters.
        /// </summary>
        public IReadOnlyList<Parameter>? Parameters { get; init; }

        /// <summary>
        ///     Pull request triggers.
        /// </summary>
        public Pr? Pr { get; init; }

        /// <summary>
        ///     Scheduled triggers.
        /// </summary>
        public IReadOnlyList<Schedule>? Schedules { get; init; }

        /// <summary>
        ///     Containers and repositories used in the build.
        /// </summary>
        public Resources.Resources? Resources { get; init; }

        /// <summary>
        ///     Variables for this pipeline.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Behavior lock requests from this stage should exhibit in relation to other exclusive lock requests.
        ///     Valid values: "sequential" | "runLatest"
        /// </summary>
        public WorkflowExpression<string>? LockBehavior { get; init; }
    }
}
