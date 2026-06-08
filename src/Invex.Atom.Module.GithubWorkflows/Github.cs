using Environment = System.Environment;

namespace Invex.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides utility methods and properties for interacting with GitHub Actions workflows.
/// </summary>
/// <remarks>
///     This static class offers convenient access to GitHub Actions environment variables,
///     defines standard pipeline directories, and provides helper methods for generating
///     Dependabot workflows.
/// </remarks>
[PublicAPI]
public static class Github
{
    /// <summary>
    ///     Gets a value indicating whether the current execution environment is GitHub Actions.
    /// </summary>
    public static bool IsGithubActions => Variables.Actions.Equals("true", StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     Contains constant strings for all known GitHub Actions environment variable names.
    /// </summary>
    [PublicAPI]
    public static class VariableNames
    {
        /// <summary>
        ///     The name of the environment variable indicating if the workflow is running on GitHub Actions.
        /// </summary>
        public const string Actions = "GITHUB_ACTIONS";

        /// <summary>
        ///     The ID of the actor that triggered the workflow.
        /// </summary>
        public const string ActorId = "GITHUB_ACTOR_ID";

        /// <summary>
        ///     The name of the actor that triggered the workflow.
        /// </summary>
        public const string Actor = "GITHUB_ACTOR";

        /// <summary>
        ///     The URL of the GitHub API.
        /// </summary>
        public const string ApiUrl = "GITHUB_API_URL";

        /// <summary>
        ///     The base ref or target branch of the pull request in a `pull_request` event.
        /// </summary>
        public const string BaseRef = "GITHUB_BASE_REF";

        /// <summary>
        ///     The path to a file that contains environment variables to set.
        /// </summary>
        public const string Env = "GITHUB_ENV";

        /// <summary>
        ///     The name of the event that triggered the workflow.
        /// </summary>
        public const string EventName = "GITHUB_EVENT_NAME";

        /// <summary>
        ///     The path to the file that contains the complete event webhook payload.
        /// </summary>
        public const string EventPath = "GITHUB_EVENT_PATH";

        /// <summary>
        ///     The URL of the GitHub GraphQL API.
        /// </summary>
        public const string GraphqlUrl = "GITHUB_GRAPHQL_URL";

        /// <summary>
        ///     The head ref or source branch of the pull request in a `pull_request` event.
        /// </summary>
        public const string HeadRef = "GITHUB_HEAD_REF";

        /// <summary>
        ///     The name of the current job.
        /// </summary>
        public const string Job = "GITHUB_JOB";

        /// <summary>
        ///     The path to a file that contains workflow commands to set outputs.
        /// </summary>
        public const string Output = "GITHUB_OUTPUT";

        /// <summary>
        ///     The path to a file that contains paths to add to the system PATH.
        /// </summary>
        public const string Path = "GITHUB_PATH";

        /// <summary>
        ///     The short name of the Git ref that triggered the workflow.
        /// </summary>
        public const string RefName = "GITHUB_REF_NAME";

        /// <summary>
        ///     A boolean indicating if the ref is protected.
        /// </summary>
        public const string RefProtected = "GITHUB_REF_PROTECTED";

        /// <summary>
        ///     The type of the Git ref (e.g., `branch`, `tag`).
        /// </summary>
        public const string RefType = "GITHUB_REF_TYPE";

        /// <summary>
        ///     The Git ref that triggered the workflow (e.g., `refs/heads/main`).
        /// </summary>
        public const string Ref = "GITHUB_REF";

        /// <summary>
        ///     The ID of the repository.
        /// </summary>
        public const string RepositoryId = "GITHUB_REPOSITORY_ID";

        /// <summary>
        ///     The ID of the repository owner.
        /// </summary>
        public const string RepositoryOwnerId = "GITHUB_REPOSITORY_OWNER_ID";

        /// <summary>
        ///     The owner of the repository.
        /// </summary>
        public const string RepositoryOwner = "GITHUB_REPOSITORY_OWNER";

        /// <summary>
        ///     The owner and repository name (e.g., `octocat/Hello-World`).
        /// </summary>
        public const string Repository = "GITHUB_REPOSITORY";

        /// <summary>
        ///     The number of days to retain workflow runs and artifacts.
        /// </summary>
        public const string RetentionDays = "GITHUB_RETENTION_DAYS";

        /// <summary>
        ///     The attempt number of the current workflow run.
        /// </summary>
        public const string RunAttempt = "GITHUB_RUN_ATTEMPT";

        /// <summary>
        ///     The unique ID of the workflow run.
        /// </summary>
        public const string RunId = "GITHUB_RUN_ID";

        /// <summary>
        ///     The unique number of the workflow run within the repository.
        /// </summary>
        public const string RunNumber = "GITHUB_RUN_NUMBER";

        /// <summary>
        ///     The URL of the GitHub server (e.g., `https://github.com`).
        /// </summary>
        public const string ServerUrl = "GITHUB_SERVER_URL";

        /// <summary>
        ///     The commit SHA that triggered the workflow.
        /// </summary>
        public const string Sha = "GITHUB_SHA";

        /// <summary>
        ///     The path to a file that contains workflow commands to save state.
        /// </summary>
        public const string State = "GITHUB_STATE";

        /// <summary>
        ///     The path to a file that contains content to append to the job summary.
        /// </summary>
        public const string StepSummary = "GITHUB_STEP_SUMMARY";

        /// <summary>
        ///     The actor that triggered the workflow, even if it was a bot.
        /// </summary>
        public const string TriggeringActor = "GITHUB_TRIGGERING_ACTOR";

        /// <summary>
        ///     The full ref of the workflow file (e.g., `octocat/Hello-World/.github/workflows/my-workflow.yml@refs/heads/main`).
        /// </summary>
        public const string WorkflowRef = "GITHUB_WORKFLOW_REF";

        /// <summary>
        ///     The commit SHA of the workflow file.
        /// </summary>
        public const string WorkflowSha = "GITHUB_WORKFLOW_SHA";

        /// <summary>
        ///     The name of the workflow.
        /// </summary>
        public const string Workflow = "GITHUB_WORKFLOW";

        /// <summary>
        ///     The path to the GitHub workspace directory.
        /// </summary>
        public const string Workspace = "GITHUB_WORKSPACE";

        /// <summary>
        ///     The architecture of the runner machine (e.g., `X64`).
        /// </summary>
        public const string RunnerArch = "RUNNER_ARCH";

        /// <summary>
        ///     A boolean indicating if runner debugging is enabled.
        /// </summary>
        public const string RunnerDebug = "RUNNER_DEBUG";

        /// <summary>
        ///     The environment of the runner machine (e.g., `github-hosted`).
        /// </summary>
        public const string RunnerEnvironment = "RUNNER_ENVIRONMENT";

        /// <summary>
        ///     The name of the runner machine.
        /// </summary>
        public const string RunnerName = "RUNNER_NAME";

        /// <summary>
        ///     The operating system of the runner machine (e.g., `Linux`, `Windows`).
        /// </summary>
        public const string RunnerOs = "RUNNER_OS";

        /// <summary>
        ///     The path to the runner performance log.
        /// </summary>
        public const string RunnerPerfLog = "RUNNER_PERFLOG";

        /// <summary>
        ///     The path to the runner's temporary directory.
        /// </summary>
        public const string RunnerTemp = "RUNNER_TEMP";

        /// <summary>
        ///     The path to the runner's tool cache directory.
        /// </summary>
        public const string RunnerToolCache = "RUNNER_TOOL_CACHE";

        /// <summary>
        ///     The tracking ID of the runner.
        /// </summary>
        public const string RunnerTrackingId = "RUNNER_TRACKING_ID";

        /// <summary>
        ///     The user account that the runner is running as.
        /// </summary>
        public const string RunnerUser = "RUNNER_USER";

        /// <summary>
        ///     The path to the runner's workspace directory.
        /// </summary>
        public const string RunnerWorkspace = "RUNNER_WORKSPACE";
    }

    /// <summary>
    ///     Provides access to GitHub Actions environment variables as strongly-typed properties.
    /// </summary>
    /// <remarks>
    ///     Each property corresponds to a GitHub Actions environment variable,
    ///     returning its value or an empty string if the variable is not set.
    /// </remarks>
    [PublicAPI]
    public static class Variables
    {
        /// <summary>
        ///     Gets the value of the <c>GITHUB_ACTIONS</c> environment variable.
        /// </summary>
        /// <example>
        ///     true
        /// </example>
        public static string Actions { get; } =
            Environment.GetEnvironmentVariable(VariableNames.Actions) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_ACTOR_ID</c> environment variable.
        /// </summary>
        /// <example>
        ///     1234567
        /// </example>
        public static string ActorId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.ActorId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_ACTOR</c> environment variable.
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string Actor { get; } = Environment.GetEnvironmentVariable(VariableNames.Actor) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_API_URL</c> environment variable.
        /// </summary>
        /// <example>
        ///     https://api.github.com
        /// </example>
        public static string ApiUrl { get; } = Environment.GetEnvironmentVariable(VariableNames.ApiUrl) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_BASE_REF</c> environment variable.
        /// </summary>
        /// <example>
        ///     main
        /// </example>
        public static string BaseRef { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BaseRef) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_ENV</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/set_env_[GUID]
        /// </example>
        public static string Env { get; } = Environment.GetEnvironmentVariable(VariableNames.Env) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_EVENT_NAME</c> environment variable.
        /// </summary>
        /// <example>
        ///     pull_request
        /// </example>
        public static string EventName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EventName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_EVENT_PATH</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_github_workflow/event.json
        /// </example>
        public static string EventPath { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EventPath) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_GRAPHQL_URL</c> environment variable.
        /// </summary>
        /// <example>
        ///     https://api.github.com/graphql
        /// </example>
        public static string GraphqlUrl { get; } =
            Environment.GetEnvironmentVariable(VariableNames.GraphqlUrl) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_HEAD_REF</c> environment variable.
        /// </summary>
        /// <example>
        ///     my-branch
        /// </example>
        public static string HeadRef { get; } =
            Environment.GetEnvironmentVariable(VariableNames.HeadRef) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_JOB</c> environment variable.
        /// </summary>
        /// <example>
        ///     BuildApp
        /// </example>
        public static string Job { get; } = Environment.GetEnvironmentVariable(VariableNames.Job) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_OUTPUT</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/set_output_[GUID]
        /// </example>
        public static string Output { get; } = Environment.GetEnvironmentVariable(VariableNames.Output) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_PATH</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/add_path_[GUID]
        /// </example>
        public static string Path { get; } = Environment.GetEnvironmentVariable(VariableNames.Path) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REF_NAME</c> environment variable.
        /// </summary>
        /// <example>
        ///     4/my-branch
        /// </example>
        public static string RefName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RefName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REF_PROTECTED</c> environment variable.
        /// </summary>
        /// <example>
        ///     false
        /// </example>
        public static string RefProtected { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RefProtected) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REF_TYPE</c> environment variable.
        /// </summary>
        /// <example>
        ///     branch
        /// </example>
        public static string RefType { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RefType) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REF</c> environment variable.
        /// </summary>
        /// <example>
        ///     refs/pull/4/merge
        /// </example>
        public static string Ref { get; } = Environment.GetEnvironmentVariable(VariableNames.Ref) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REPOSITORY_ID</c> environment variable.
        /// </summary>
        /// <example>
        ///     123456789
        /// </example>
        public static string RepositoryId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RepositoryId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REPOSITORY_OWNER_ID</c> environment variable.
        /// </summary>
        /// <example>
        ///     1234567
        /// </example>
        public static string RepositoryOwnerId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RepositoryOwnerId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REPOSITORY_OWNER</c> environment variable.
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string RepositoryOwner { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RepositoryOwner) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_REPOSITORY</c> environment variable.
        /// </summary>
        /// <example>
        ///     ArthurDent/my-project
        /// </example>
        public static string Repository { get; } =
            Environment.GetEnvironmentVariable(VariableNames.Repository) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_RETENTION_DAYS</c> environment variable.
        /// </summary>
        /// <example>
        ///     90
        /// </example>
        public static string RetentionDays { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RetentionDays) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_RUN_ATTEMPT</c> environment variable.
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        public static string RunAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunAttempt) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_RUN_ID</c> environment variable.
        /// </summary>
        /// <example>
        ///     9876543210
        /// </example>
        public static string RunId { get; } = Environment.GetEnvironmentVariable(VariableNames.RunId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_RUN_NUMBER</c> environment variable.
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        public static string RunNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunNumber) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_SERVER_URL</c> environment variable.
        /// </summary>
        /// <example>
        ///     https://github.com
        /// </example>
        public static string ServerUrl { get; } =
            Environment.GetEnvironmentVariable(VariableNames.ServerUrl) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_SHA</c> environment variable.
        /// </summary>
        /// <example>
        ///     [SHA]
        /// </example>
        public static string Sha { get; } = Environment.GetEnvironmentVariable(VariableNames.Sha) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_STATE</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/save_state_[GUID]
        /// </example>
        public static string State { get; } = Environment.GetEnvironmentVariable(VariableNames.State) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_STEP_SUMMARY</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp/_runner_file_commands/step_summary_[GUID]
        /// </example>
        public static string StepSummary { get; } =
            Environment.GetEnvironmentVariable(VariableNames.StepSummary) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_TRIGGERING_ACTOR</c> environment variable.
        /// </summary>
        /// <example>
        ///     ArthurDent
        /// </example>
        public static string TriggeringActor { get; } =
            Environment.GetEnvironmentVariable(VariableNames.TriggeringActor) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_WORKFLOW_REF</c> environment variable.
        /// </summary>
        /// <example>
        ///     ArthurDent/my-project/.github/workflows/BuildApp.yml@refs/pull/4/merge
        /// </example>
        public static string WorkflowRef { get; } =
            Environment.GetEnvironmentVariable(VariableNames.WorkflowRef) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_WORKFLOW_SHA</c> environment variable.
        /// </summary>
        /// <example>
        ///     [SHA]
        /// </example>
        public static string WorkflowSha { get; } =
            Environment.GetEnvironmentVariable(VariableNames.WorkflowSha) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_WORKFLOW</c> environment variable.
        /// </summary>
        /// <example>
        ///     build
        /// </example>
        public static string Workflow { get; } =
            Environment.GetEnvironmentVariable(VariableNames.Workflow) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>GITHUB_WORKSPACE</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/my-project/my-project
        /// </example>
        public static string Workspace { get; } =
            Environment.GetEnvironmentVariable(VariableNames.Workspace) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_ARCH</c> environment variable.
        /// </summary>
        /// <example>
        ///     X64
        /// </example>
        public static string RunnerArch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerArch) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_DEBUG</c> environment variable.
        /// </summary>
        /// <example>
        ///     1
        /// </example>
        public static string RunnerDebug { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerDebug) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_ENVIRONMENT</c> environment variable.
        /// </summary>
        /// <example>
        ///     github-hosted
        /// </example>
        public static string RunnerEnvironment { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerEnvironment) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_NAME</c> environment variable.
        /// </summary>
        /// <example>
        ///     GitHubActions 6
        /// </example>
        public static string RunnerName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_OS</c> environment variable.
        /// </summary>
        /// <example>
        ///     Linux
        /// </example>
        public static string RunnerOs { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerOs) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_PERFLOG</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/perflog
        /// </example>
        public static string RunnerPerfLog { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerPerfLog) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_TEMP</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/_temp
        /// </example>
        public static string RunnerTemp { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerTemp) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_TOOL_CACHE</c> environment variable.
        /// </summary>
        /// <example>
        ///     /opt/hostedtoolcache
        /// </example>
        public static string RunnerToolCache { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerToolCache) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_TRACKING_ID</c> environment variable.
        /// </summary>
        /// <example>
        ///     github_3cf77bfa-2ce5-4b4c-b736-d6a7b5c16537
        /// </example>
        public static string RunnerTrackingId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerTrackingId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_USER</c> environment variable.
        /// </summary>
        /// <example>
        ///     runner
        /// </example>
        public static string RunnerUser { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerUser) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>RUNNER_WORKSPACE</c> environment variable.
        /// </summary>
        /// <example>
        ///     /home/runner/work/my-project
        /// </example>
        public static string RunnerWorkspace { get; } =
            Environment.GetEnvironmentVariable(VariableNames.RunnerWorkspace) ?? string.Empty;
    }
}
