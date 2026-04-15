namespace DecSm.Atom.Module.GithubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowExpressionExtensions
{
    [PublicAPI]
    public sealed class Expressions
    {
        internal static Expressions Instance => field ??= new();

        // Github

        /// <summary>
        ///     The github context contains information about the workflow run and the event that triggered the run. You can read
        ///     most of the github context data in environment variables. For more information about environment variables, see
        ///     Store information in variables.
        /// </summary>
        public RawExpression Github => field ??= new("github");

        /// <summary>
        ///     The name of the action currently running, or the
        ///     <see href="https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepsid">
        ///         <c>id</c>
        ///     </see>
        ///     of a step. GitHub removes
        ///     special characters, and uses the name <c>__run</c> when the current step runs a script without an <c>id</c>. If you
        ///     use the
        ///     same action more than once in the same job, the name will include a suffix with the sequence number with underscore
        ///     before it. For example, the first script you run will have the name <c>__run</c>, and the second script will be
        ///     named
        ///     <c>__run_2</c>. Similarly, the second invocation of <c>actions/checkout</c> will be <c>actionscheckout2</c>.
        /// </summary>
        public RawExpression GithubAction => field ??= new("github.action");

        /// <summary>
        ///     The path where an action is located. This property is only supported in composite actions. You can use this path to
        ///     access files located in the same repository as the action, for example by changing directories to the path (using
        ///     the corresponding environment variable): <c>cd "$GITHUB_ACTION_PATH"</c>. For more information on environment
        ///     variables,
        ///     see
        ///     <see
        ///         href="https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions#use-an-intermediate-environment-variable">
        ///         Secure
        ///         use reference
        ///     </see>
        ///     .
        /// </summary>
        public RawExpression GithubActionPath => field ??= new("github.action_path");

        /// <summary>
        ///     For a step executing an action, this is the ref of the action being executed. For example, <c>v2</c>.
        /// </summary>
        public RawExpression GithubActionRef => field ??= new("github.action_ref");

        /// <summary>
        ///     For a step executing an action, this is the owner and repository name of the action. For example,
        ///     <c>actions/checkout</c>.
        /// </summary>
        public RawExpression GithubActionRepository => field ??= new("github.action_repository");

        /// <summary>
        ///     For a composite action, the current result of the composite action.
        /// </summary>
        public RawExpression GithubActionStatus => field ??= new("github.action_status");

        /// <summary>
        ///     The username of the user that triggered the initial workflow run. If the workflow run is a re-run, this value may
        ///     differ from <c>github.triggering_actor</c>. Any workflow re-runs will use the privileges of <c>github.actor</c>,
        ///     even if the
        ///     actor initiating the re-run (<c>github.triggering_actor</c>) has different privileges.
        /// </summary>
        public RawExpression GithubActor => field ??= new("github.actor");

        /// <summary>
        ///     The account ID of the person or app that triggered the initial workflow run. For example, <c>1234567</c>. Note that
        ///     this
        ///     is different from the actor username.
        /// </summary>
        public RawExpression GithubActorId => field ??= new("github.actor_id");

        /// <summary>
        ///     The URL of the GitHub REST API.
        /// </summary>
        public RawExpression GithubApiUrl => field ??= new("github.api_url");

        /// <summary>
        ///     The <c>base_ref</c> or target branch of the pull request in a workflow run. This property is only available when
        ///     the
        ///     event that triggers a workflow run is either <c>pull_request</c> or <c>pull_request_target</c>.
        /// </summary>
        public RawExpression GithubBaseRef => field ??= new("github.base_ref");

        /// <summary>
        ///     Path on the runner to the file that sets environment variables from workflow commands. This file is unique to the
        ///     current step and is a different file for each step in a job.
        /// </summary>
        public RawExpression GithubEnv => field ??= new("github.env");

        /// <summary>
        ///     The full event webhook payload. You can access individual properties of the event using this context. This object
        ///     is identical to the webhook payload of the event that triggered the workflow run, and is different for each event.
        /// </summary>
        public RawExpression GithubEvent => field ??= new("github.event");

        /// <summary>
        ///     The name of the event that triggered the workflow run.
        /// </summary>
        public RawExpression GithubEventName => field ??= new("github.event_name");

        /// <summary>
        ///     The path to the file on the runner that contains the full event webhook payload.
        /// </summary>
        public RawExpression GithubEventPath => field ??= new("github.event_path");

        /// <summary>
        ///     The URL of the GitHub GraphQL API.
        /// </summary>
        public RawExpression GithubGraphqlUrl => field ??= new("github.graphql_url");

        /// <summary>
        ///     The <c>head_ref</c> or source branch of the pull request in a workflow run. This property is only available when
        ///     the
        ///     event that triggers a workflow run is either <c>pull_request</c> or <c>pull_request_target</c>.
        /// </summary>
        public RawExpression GithubHeadRef => field ??= new("github.head_ref");

        /// <summary>
        ///     The job_id of the current job. Note: This context property is set by the Actions runner, and is only available
        ///     within the execution <c>steps</c> of a job. Otherwise, the value of this property will be <c>null</c>.
        /// </summary>
        public RawExpression GithubJob => field ??= new("github.job");

        /// <summary>
        ///     Path on the runner to the file that sets system <c>PATH</c> variables from workflow commands. This file is unique
        ///     to the
        ///     current step and is a different file for each step in a job.
        /// </summary>
        public RawExpression GithubPath => field ??= new("github.path");

        /// <summary>
        ///     The fully-formed ref of the branch or tag that triggered the workflow run. For branches the format is
        ///     <c>refs/heads/&lt;branch_name&gt;</c>.
        ///     For pull requests events except <c>pull_request_target</c> that were not merged, it is
        ///     <c>refs/pull/&lt;pr_number&gt;/merge</c>.
        ///     For tags it is <c>refs/tags/&lt;tag_name&gt;</c>. For example, <c>refs/heads/feature-branch-1</c>.
        /// </summary>
        public RawExpression GithubRef => field ??= new("github.ref");

        /// <summary>
        ///     The short ref name of the branch or tag that triggered the workflow run. This value matches the branch or tag name
        ///     shown on GitHub. For example, <c>feature-branch-1</c>. For pull requests that were not merged, the format is
        ///     <c>&lt;pr_number&gt;/merge</c>.
        /// </summary>
        public RawExpression GithubRefName => field ??= new("github.ref_name");

        /// <summary>
        ///     <c>true</c> if branch protections or rulesets are configured for the ref that triggered the workflow run.
        /// </summary>
        public RawExpression GithubRefProtected => field ??= new("github.ref_protected");

        /// <summary>
        ///     The type of ref that triggered the workflow run. Valid values are <c>branch</c> or <c>tag</c>.
        /// </summary>
        public RawExpression GithubRefType => field ??= new("github.ref_type");

        /// <summary>
        ///     The owner and repository name. For example, <c>octocat/Hello-World</c>.
        /// </summary>
        public RawExpression GithubRepository => field ??= new("github.repository");

        /// <summary>
        ///     The ID of the repository. For example, <c>123456789</c>. Note that this is different from the repository name.
        /// </summary>
        public RawExpression GithubRepositoryId => field ??= new("github.repository_id");

        /// <summary>
        ///     The repository owner's username. For example, <c>octocat</c>.
        /// </summary>
        public RawExpression GithubRepositoryOwner => field ??= new("github.repository_owner");

        /// <summary>
        ///     The repository owner's account ID. For example, <c>1234567</c>. Note that this is different from the owner's name.
        /// </summary>
        public RawExpression GithubRepositoryOwnerId => field ??= new("github.repository_owner_id");

        /// <summary>
        ///     The Git URL to the repository. For example, <c>git://github.com/octocat/hello-world.git</c>.
        /// </summary>
        public RawExpression GithubRepositoryUrl => field ??= new("github.repositoryUrl");

        /// <summary>
        ///     The number of days that workflow run logs and artifacts are kept.
        /// </summary>
        public RawExpression GithubRetentionDays => field ??= new("github.retention_days");

        /// <summary>
        ///     A unique number for each workflow run within a repository. This number does not change if you re-run the workflow
        ///     run.
        /// </summary>
        public RawExpression GithubRunId => field ??= new("github.run_id");

        /// <summary>
        ///     A unique number for each run of a particular workflow in a repository. This number begins at 1 for the workflow's
        ///     first run, and increments with each new run. This number does not change if you re-run the workflow run.
        /// </summary>
        public RawExpression GithubRunNumber => field ??= new("github.run_number");

        /// <summary>
        ///     A unique number for each attempt of a particular workflow run in a repository. This number begins at 1 for the
        ///     workflow run's first attempt, and increments with each re-run.
        /// </summary>
        public RawExpression GithubRunAttempt => field ??= new("github.run_attempt");

        /// <summary>
        ///     The source of a secret used in a workflow. Possible values are <c>None</c>, <c>Actions</c>, <c>Codespaces</c>, or
        ///     <c>Dependabot</c>.
        /// </summary>
        public RawExpression GithubSecretSource => field ??= new("github.secret_source");

        /// <summary>
        ///     The URL of the GitHub server. For example: <c>https://github.com</c>.
        /// </summary>
        public RawExpression GithubServerUrl => field ??= new("github.server_url");

        /// <summary>
        ///     The commit SHA that triggered the workflow. The value of this commit SHA depends on the event that triggered the
        ///     workflow. For example, <c>ffac537e6cbbf934b08745a378932722df287a53</c>.
        /// </summary>
        public RawExpression GithubSha => field ??= new("github.sha");

        /// <summary>
        ///     A token to authenticate on behalf of the GitHub App installed on your repository. This is functionally equivalent
        ///     to the <c>GITHUB_TOKEN</c> secret. Note: This context property is set by the Actions runner, and is only available
        ///     within the execution <c>steps</c> of a job. Otherwise, the value of this property will be <c>null</c>.
        /// </summary>
        public RawExpression GithubToken => field ??= new("github.token");

        /// <summary>
        ///     The username of the user that initiated the workflow run. If the workflow run is a re-run, this value may differ
        ///     from <c>github.actor</c>. Any workflow re-runs will use the privileges of <c>github.actor</c>, even if the actor
        ///     initiating
        ///     the re-run (<c>github.triggering_actor</c>) has different privileges.
        /// </summary>
        public RawExpression GithubTriggeringActor => field ??= new("github.triggering_actor");

        /// <summary>
        ///     The name of the workflow. If the workflow file doesn't specify a <c>name</c>, the value of this property is the
        ///     full
        ///     path of the workflow file in the repository.
        /// </summary>
        public RawExpression GithubWorkflow => field ??= new("github.workflow");

        /// <summary>
        ///     The ref path to the workflow. For example,
        ///     <c>octocat/hello-world/.github/workflows/my-workflow.yml@refs/heads/my_branch</c>.
        /// </summary>
        public RawExpression GithubWorkflowRef => field ??= new("github.workflow_ref");

        /// <summary>
        ///     The commit SHA for the workflow file.
        /// </summary>
        public RawExpression GithubWorkflowSha => field ??= new("github.workflow_sha");

        /// <summary>
        ///     The default working directory on the runner for steps, and the default location of your repository when using the
        ///     <c>checkout</c> action.
        /// </summary>
        public RawExpression GithubWorkspace => field ??= new("github.workspace");

        // Env

        /// <summary>
        ///     The env context contains variables that have been set in a workflow, job, or step. It does not contain variables
        ///     inherited by the runner process.
        /// </summary>
        public RawExpression Env => field ??= new("env");

        // Vars

        /// <summary>
        ///     The vars context contains custom configuration variables set at the organization, repository, and environment
        ///     levels.
        /// </summary>
        public RawExpression Vars => field ??= new("vars");

        // Job

        /// <summary>
        ///     This context changes for each job in a workflow run. You can access this context from any step in a job.
        ///     This object contains all the properties listed below.
        /// </summary>
        public RawExpression Job => field ??= new("job");

        /// <summary>
        ///     The check run ID of the current job.
        /// </summary>
        public RawExpression JobCheckRunId => field ??= new("job.check_run_id");

        /// <summary>
        ///     Information about the job's container.
        /// </summary>
        public RawExpression JobContainer => field ??= new("job.container");

        /// <summary>
        ///     The ID of the container.
        /// </summary>
        public RawExpression JobContainerId => field ??= new("job.container.id");

        /// <summary>
        ///     The ID of the container network. The runner creates the network used by all containers in a job.
        /// </summary>
        public RawExpression JobContainerNetwork => field ??= new("job.container.network");

        /// <summary>
        ///     The service containers created for a job.
        /// </summary>
        public RawExpression JobServices => field ??= new("job.services");

        /// <summary>
        ///     The current status of the job. Possible values are <c>success</c>, <c>failure</c>, or <c>cancelled</c>.
        /// </summary>
        public RawExpression JobStatus => field ??= new("job.status");

        // Jobs (reusable workflows only)

        /// <summary>
        ///     This is only available in reusable workflows, and can only be used to set outputs for a reusable workflow.
        ///     This object contains all the properties listed below.
        /// </summary>
        public RawExpression Jobs => field ??= new("jobs");

        // Steps

        /// <summary>
        ///     This context changes for each step in a job. You can access this context from any step in a job.
        ///     This object contains all the properties listed below.
        /// </summary>
        public RawExpression Steps => field ??= new("steps");

        // Runner

        /// <summary>
        ///     This context changes for each job in a workflow run. This object contains all the properties listed below.
        /// </summary>
        public RawExpression Runner => field ??= new("runner");

        /// <summary>
        ///     The name of the runner executing the job. This name may not be unique in a workflow run as runners at the
        ///     repository and organization levels could use the same name.
        /// </summary>
        public RawExpression RunnerName => field ??= new("runner.name");

        /// <summary>
        ///     The operating system of the runner executing the job. Possible values are <c>Linux</c>, <c>Windows</c>, or
        ///     <c>macOS</c>.
        /// </summary>
        public RawExpression RunnerOs => field ??= new("runner.os");

        /// <summary>
        ///     The architecture of the runner executing the job. Possible values are <c>X86</c>, <c>X64</c>, <c>ARM</c>, or
        ///     <c>ARM64</c>.
        /// </summary>
        public RawExpression RunnerArch => field ??= new("runner.arch");

        /// <summary>
        ///     The path to a temporary directory on the runner. This directory is emptied at the beginning and end of each job.
        ///     Note that files will not be removed if the runner's user account does not have permission to delete them.
        /// </summary>
        public RawExpression RunnerTemp => field ??= new("runner.temp");

        /// <summary>
        ///     The path to the directory containing preinstalled tools for GitHub-hosted runners.
        /// </summary>
        public RawExpression RunnerToolCache => field ??= new("runner.tool_cache");

        /// <summary>
        ///     This is set only if debug logging is enabled, and always has the value of <c>1</c>. It can be useful as an
        ///     indicator
        ///     to enable additional debugging or verbose logging in your own job steps.
        /// </summary>
        public RawExpression RunnerDebug => field ??= new("runner.debug");

        /// <summary>
        ///     The environment of the runner executing the job. Possible values are: <c>github-hosted</c> for GitHub-hosted
        ///     runners
        ///     provided by GitHub, and <c>self-hosted</c> for self-hosted runners configured by the repository owner.
        /// </summary>
        public RawExpression RunnerEnvironment => field ??= new("runner.environment");

        // Secrets

        /// <summary>
        ///     This context is the same for each job in a workflow run. You can access this context from any step in a job.
        ///     This object contains all the properties listed below.
        /// </summary>
        public RawExpression Secrets => field ??= new("secrets");

        /// <summary>
        ///     Automatically created token for each workflow run.
        /// </summary>
        public RawExpression SecretsGithubToken => field ??= new("secrets.GITHUB_TOKEN");

        // Strategy

        /// <summary>
        ///     This context changes for each job in a workflow run. You can access this context from any job or step in a
        ///     workflow.
        ///     This object contains all the properties listed below.
        /// </summary>
        public RawExpression Strategy => field ??= new("strategy");

        /// <summary>
        ///     When this evaluates to <c>true</c>, all in-progress jobs are canceled if any job in a matrix fails.
        /// </summary>
        public RawExpression StrategyFailFast => field ??= new("strategy.fail-fast");

        /// <summary>
        ///     The index of the current job in the matrix. Note: This number is a zero-based number.
        ///     The first job's index in the matrix is <c>0</c>.
        /// </summary>
        public RawExpression StrategyJobIndex => field ??= new("strategy.job-index");

        /// <summary>
        ///     The total number of jobs in the matrix. Note: This number is not a zero-based number.
        ///     For example, for a matrix with four jobs, the value of <c>job-total</c> is <c>4</c>.
        /// </summary>
        public RawExpression StrategyJobTotal => field ??= new("strategy.job-total");

        /// <summary>
        ///     The maximum number of jobs that can run simultaneously when using a <c>matrix</c> job strategy.
        /// </summary>
        public RawExpression StrategyMaxParallel => field ??= new("strategy.max-parallel");

        // Matrix

        /// <summary>
        ///     This context is only available for jobs in a matrix, and changes for each job in a workflow run.
        ///     You can access this context from any job or step in a workflow. This object contains the properties listed below.
        /// </summary>
        public RawExpression Matrix => field ??= new("matrix");

        // Needs

        /// <summary>
        ///     This context is only populated for workflow runs that have dependent jobs, and changes for each job in a workflow
        ///     run.
        ///     You can access this context from any job or step in a workflow. This object contains all the properties listed
        ///     below.
        /// </summary>
        public RawExpression Needs => field ??= new("needs");

        // Inputs

        /// <summary>
        ///     This context is only available in a reusable workflow or in a workflow triggered by the <c>workflow_dispatch</c>
        ///     event.
        ///     You can access this context from any job or step in a workflow. This object contains the properties listed below.
        /// </summary>
        public RawExpression Inputs => field ??= new("inputs");

        /// <summary>
        ///     The value of a specific environment variable.
        /// </summary>
        /// <param name="envName">The name of the environment variable.</param>
        public RawExpression EnvVar(string envName) =>
            new($"env.{envName}");

        /// <summary>
        ///     The value of a specific configuration variable.
        /// </summary>
        /// <param name="varName">The name of the configuration variable.</param>
        public RawExpression VarsVar(string varName) =>
            new($"vars.{varName}");

        /// <summary>
        ///     The ID of the service container.
        /// </summary>
        /// <param name="serviceId">The service container ID.</param>
        public RawExpression JobServicesId(string serviceId) =>
            new($"job.services.{serviceId}.id");

        /// <summary>
        ///     The ID of the service container network. The runner creates the network used by all containers in a job.
        /// </summary>
        /// <param name="serviceId">The service container ID.</param>
        public RawExpression JobServicesNetwork(string serviceId) =>
            new($"job.services.{serviceId}.network");

        /// <summary>
        ///     The exposed ports of the service container.
        /// </summary>
        /// <param name="serviceId">The service container ID.</param>
        public RawExpression JobServicesPorts(string serviceId) =>
            new($"job.services.{serviceId}.ports");

        /// <summary>
        ///     A specific exposed port of the service container.
        /// </summary>
        /// <param name="serviceId">The service container ID.</param>
        /// <param name="port">The port number.</param>
        public RawExpression JobServicesPort(string serviceId, int port) =>
            new($"job.services.{serviceId}.ports[{port}]");

        /// <summary>
        ///     The result of a job in the reusable workflow. Possible values are <c>success</c>, <c>failure</c>, <c>cancelled</c>,
        ///     or <c>skipped</c>.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public RawExpression JobsResult(string jobId) =>
            new($"jobs.{jobId}.result");

        /// <summary>
        ///     The set of outputs of a job in a reusable workflow.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public RawExpression JobsOutputs(string jobId) =>
            new($"jobs.{jobId}.outputs");

        /// <summary>
        ///     The value of a specific output for a job in a reusable workflow.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="outputName">The output name.</param>
        public RawExpression JobsOutput(string jobId, string outputName) =>
            new($"jobs.{jobId}.outputs.{outputName}");

        /// <summary>
        ///     The set of outputs defined for the step.
        /// </summary>
        /// <param name="stepId">The step ID.</param>
        public RawExpression StepsOutputs(string stepId) =>
            new($"steps.{stepId}.outputs");

        /// <summary>
        ///     The value of a specific output for the step.
        /// </summary>
        /// <param name="stepId">The step ID.</param>
        /// <param name="outputName">The output name.</param>
        public RawExpression StepsOutput(string stepId, string outputName) =>
            new($"steps.{stepId}.outputs.{outputName}");

        /// <summary>
        ///     The result of a completed step after <c>continue-on-error</c> is applied.
        ///     Possible values are <c>success</c>, <c>failure</c>, <c>cancelled</c>, or <c>skipped</c>.
        ///     When a <c>continue-on-error</c> step fails, the <c>outcome</c> is <c>failure</c>, but the final <c>conclusion</c>
        ///     is <c>success</c>.
        /// </summary>
        /// <param name="stepId">The step ID.</param>
        public RawExpression StepsConclusion(string stepId) =>
            new($"steps.{stepId}.conclusion");

        /// <summary>
        ///     The result of a completed step before <c>continue-on-error</c> is applied.
        ///     Possible values are <c>success</c>, <c>failure</c>, <c>cancelled</c>, or <c>skipped</c>.
        ///     When a <c>continue-on-error</c> step fails, the <c>outcome</c> is <c>failure</c>, but the final <c>conclusion</c>
        ///     is <c>success</c>.
        /// </summary>
        /// <param name="stepId">The step ID.</param>
        public RawExpression StepsOutcome(string stepId) =>
            new($"steps.{stepId}.outcome");

        /// <summary>
        ///     The value of a specific secret.
        /// </summary>
        /// <param name="secretName">The name of the secret.</param>
        public RawExpression SecretsSecret(string secretName) =>
            new($"secrets.{secretName}");

        /// <summary>
        ///     The value of a matrix property.
        /// </summary>
        /// <param name="propertyName">The name of the matrix property.</param>
        public RawExpression MatrixProperty(string propertyName) =>
            new($"matrix.{propertyName}");

        /// <summary>
        ///     A single job that the current job depends on.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public RawExpression NeedsJob(string jobId) =>
            new($"needs.{jobId}");

        /// <summary>
        ///     The set of outputs of a job that the current job depends on.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public RawExpression NeedsOutputs(string jobId) =>
            new($"needs.{jobId}.outputs");

        /// <summary>
        ///     The value of a specific output for a job that the current job depends on.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="outputName">The output name.</param>
        public RawExpression NeedsOutput(string jobId, string outputName) =>
            new($"needs.{jobId}.outputs.{outputName}");

        /// <summary>
        ///     The result of a job that the current job depends on. Possible values are <c>success</c>, <c>failure</c>,
        ///     <c>cancelled</c>, or <c>skipped</c>.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        public RawExpression NeedsResult(string jobId) =>
            new($"needs.{jobId}.result");

        /// <summary>
        ///     Each input value passed from an external workflow.
        /// </summary>
        /// <param name="inputName">The name of the input.</param>
        public RawExpression InputsInput(string inputName) =>
            new($"inputs.{inputName}");
    }

    extension(WorkflowExpressions)
    {
        [PublicAPI]
        public static Expressions Github => Expressions.Instance;
    }
}
