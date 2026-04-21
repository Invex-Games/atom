namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Steps;

/// <summary>
///     Steps are a linear sequence of operations that make up a job.
/// </summary>
/// <remarks>
///     Supports 12 implementations:
///     - Task: Runs a task
///     - Script: Runs a script using cmd.exe (Windows) or Bash (other platforms)
///     - PowerShell: Runs a script using Windows PowerShell or pwsh
///     - Pwsh: Runs a script in PowerShell Core
///     - Bash: Runs a script in Bash
///     - Checkout: Configure how the pipeline checks out source code
///     - Download: Downloads pipeline artifacts
///     - DownloadBuild: Downloads build artifacts
///     - GetPackage: Downloads a package from a feed
///     - Publish: Publishes (uploads) a file or folder as a pipeline artifact
///     - Template: Reference to a step template
///     - ReviewApp: Creates a resource dynamically under a deploy phase provider
/// </remarks>
[PublicAPI]
[Union]
public partial record Step
{
    /// <summary>
    ///     Runs a task.
    /// </summary>
    public sealed partial record Task
    {
        /// <summary>
        ///     Name of the task to run.
        /// </summary>
        public required WorkflowExpression TaskName { get; init; }

        /// <summary>
        ///     Inputs for the task.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowExpression>? Inputs { get; init; }

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
    }

    /// <summary>
    ///     Runs a script using cmd.exe on Windows and Bash on other platforms.
    /// </summary>
    public sealed partial record Script
    {
        /// <summary>
        ///     An inline script.
        /// </summary>
        public required WorkflowExpression ScriptContent { get; init; }

        /// <summary>
        ///     Fail the task if output is sent to Stderr?
        /// </summary>
        public WorkflowExpression? FailOnStderr { get; init; }

        /// <summary>
        ///     Start the script with this working directory.
        /// </summary>
        public WorkflowExpression? WorkingDirectory { get; init; }

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
    }

    /// <summary>
    ///     Runs a script using either Windows PowerShell (on Windows) or pwsh (Linux and macOS).
    /// </summary>
    public sealed partial record PowerShell
    {
        /// <summary>
        ///     An inline PowerShell script.
        /// </summary>
        public required WorkflowExpression ScriptContent { get; init; }

        /// <summary>
        ///     Fail the task if output is sent to Stderr?
        /// </summary>
        public WorkflowExpression? FailOnStderr { get; init; }

        /// <summary>
        ///     Start the script with this working directory.
        /// </summary>
        public WorkflowExpression? WorkingDirectory { get; init; }

        /// <summary>
        ///     Prepend error output stream data to the PowerShell $Error variable.
        /// </summary>
        public WorkflowExpression? ErrorActionPreference { get; init; }

        /// <summary>
        ///     Ignore last exit code returned from the PowerShell script.
        /// </summary>
        public WorkflowExpression? IgnoreLastExitCode { get; init; }

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
    }

    /// <summary>
    ///     Runs a script in PowerShell Core on Windows, macOS, and Linux.
    /// </summary>
    public sealed partial record Pwsh
    {
        /// <summary>
        ///     An inline PowerShell Core script.
        /// </summary>
        public required WorkflowExpression ScriptContent { get; init; }

        /// <summary>
        ///     Fail the task if output is sent to Stderr?
        /// </summary>
        public WorkflowExpression? FailOnStderr { get; init; }

        /// <summary>
        ///     Start the script with this working directory.
        /// </summary>
        public WorkflowExpression? WorkingDirectory { get; init; }

        /// <summary>
        ///     Prepend error output stream data to the PowerShell $Error variable.
        /// </summary>
        public WorkflowExpression? ErrorActionPreference { get; init; }

        /// <summary>
        ///     Ignore last exit code returned from the PowerShell script.
        /// </summary>
        public WorkflowExpression? IgnoreLastExitCode { get; init; }

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
    }

    /// <summary>
    ///     Runs a script in Bash on Windows, macOS, and Linux.
    /// </summary>
    public sealed partial record Bash
    {
        /// <summary>
        ///     An inline Bash script.
        /// </summary>
        public required WorkflowExpression ScriptContent { get; init; }

        /// <summary>
        ///     Fail the task if output is sent to Stderr?
        /// </summary>
        public WorkflowExpression? FailOnStderr { get; init; }

        /// <summary>
        ///     Start the script with this working directory.
        /// </summary>
        public WorkflowExpression? WorkingDirectory { get; init; }

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
    }

    /// <summary>
    ///     Configure how the pipeline checks out source code.
    /// </summary>
    public sealed partial record Checkout
    {
        /// <summary>
        ///     Repository to checkout. Valid values: "self" | "none" or repository resource name.
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
    }

    /// <summary>
    ///     Downloads artifacts associated with the current run or from another Azure Pipeline.
    /// </summary>
    public sealed partial record Download
    {
        /// <summary>
        ///     Pipeline resource identifier. Valid values: "current" | "specific" | pipeline resource name.
        /// </summary>
        public required WorkflowExpression Pipeline { get; init; }

        /// <summary>
        ///     Artifact name to download. If not specified, all artifacts are downloaded.
        /// </summary>
        public WorkflowExpression? Artifact { get; init; }

        /// <summary>
        ///     Pattern to match against artifact names.
        /// </summary>
        public WorkflowExpressionCollection? Patterns { get; init; }

        /// <summary>
        ///     Download path.
        /// </summary>
        public WorkflowExpression? Path { get; init; }

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
    }

    /// <summary>
    ///     Downloads build artifacts.
    /// </summary>
    public sealed partial record DownloadBuild
    {
        /// <summary>
        ///     Build resource identifier.
        /// </summary>
        public required WorkflowExpression Build { get; init; }

        /// <summary>
        ///     Artifact name to download.
        /// </summary>
        public WorkflowExpression? Artifact { get; init; }

        /// <summary>
        ///     Pattern to match against artifact names.
        /// </summary>
        public WorkflowExpressionCollection? Patterns { get; init; }

        /// <summary>
        ///     Download path.
        /// </summary>
        public WorkflowExpression? Path { get; init; }

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
    }

    /// <summary>
    ///     Downloads a package from a package management feed in Azure Artifacts or Azure DevOps Server.
    /// </summary>
    public sealed partial record GetPackage
    {
        /// <summary>
        ///     Package resource identifier.
        /// </summary>
        public required WorkflowExpression Package { get; init; }

        /// <summary>
        ///     Package version to download.
        /// </summary>
        public WorkflowExpression? Version { get; init; }

        /// <summary>
        ///     Download path.
        /// </summary>
        public WorkflowExpression? Path { get; init; }

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
    }

    /// <summary>
    ///     Publishes (uploads) a file or folder as a pipeline artifact.
    /// </summary>
    public sealed partial record Publish
    {
        /// <summary>
        ///     Path to the file or folder to publish.
        /// </summary>
        public required WorkflowExpression PublishPath { get; init; }

        /// <summary>
        ///     Artifact name.
        /// </summary>
        public WorkflowExpression? Artifact { get; init; }

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
    }

    /// <summary>
    ///     Reference to a step template.
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

    /// <summary>
    ///     Creates a resource dynamically under a deploy phase provider.
    /// </summary>
    public sealed partial record ReviewApp
    {
        /// <summary>
        ///     Review app type.
        /// </summary>
        public required WorkflowExpression ReviewAppType { get; init; }

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
    }
}

/// <summary>
///     Environment in which to run a step.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - TargetName: Simple string target name
///     - TargetSpec: Full specification with container, commands, and settable variables
/// </remarks>
[PublicAPI]
[Union]
public partial record StepTarget
{
    /// <summary>
    ///     Simple target name.
    /// </summary>
    public sealed partial record TargetName
    {
        /// <summary>
        ///     Name of the target environment.
        /// </summary>
        public required WorkflowExpression Name { get; init; }
    }

    /// <summary>
    ///     Full target specification.
    /// </summary>
    public sealed partial record TargetSpec
    {
        /// <summary>
        ///     Container resource name to target.
        /// </summary>
        public WorkflowExpression? Container { get; init; }

        /// <summary>
        ///     Commands to run in the target.
        /// </summary>
        public WorkflowExpression? Commands { get; init; }

        /// <summary>
        ///     Restrictions on which variables can be set from this step.
        /// </summary>
        public SettableVariables? SettableVariables { get; init; }
    }
}

/// <summary>
///     Restrictions on which variables can be set from a step.
/// </summary>
[PublicAPI]
public sealed record SettableVariables
{
    /// <summary>
    ///     Whether variables can be set.
    /// </summary>
    public WorkflowExpression? Allowed { get; init; }

    /// <summary>
    ///     List of variable names that are allowed to be set.
    /// </summary>
    public WorkflowExpressionCollection? AllowedVariables { get; init; }
}
