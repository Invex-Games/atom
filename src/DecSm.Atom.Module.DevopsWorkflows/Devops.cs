namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Provides utility methods and properties for interacting with Azure DevOps Pipelines.
/// </summary>
/// <remarks>
///     This static class offers convenient access to Azure DevOps environment variables,
///     and defines standard pipeline directories for artifact staging and binary output.
/// </remarks>
[PublicAPI]
public static class Devops
{
    /// <summary>
    ///     Gets a value indicating whether the current execution environment is Azure DevOps Pipelines.
    /// </summary>
    public static bool IsDevopsPipelines => Variables.TfBuild.Equals("true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets the default pipeline publish directory path for Azure DevOps Pipelines.
    /// </summary>
    /// <remarks>
    ///     This path typically corresponds to `$(Build.BinariesDirectory)` in Azure DevOps,
    ///     where compiled binaries and other build outputs are placed.
    /// </remarks>
    public static string PipelinePublishDirectory => "$(Build.BinariesDirectory)";

    /// <summary>
    ///     Gets the default pipeline artifact directory path for Azure DevOps Pipelines.
    /// </summary>
    /// <remarks>
    ///     This path typically corresponds to `$(Build.ArtifactStagingDirectory)` in Azure DevOps,
    ///     used for staging artifacts before publishing them.
    /// </remarks>
    public static string PipelineArtifactDirectory => "$(Build.ArtifactStagingDirectory)";

    /// <summary>
    ///     Gets an instance of <see cref="DevopsWorkflowType" /> for defining Azure DevOps-specific workflow types.
    /// </summary>
    public static DevopsWorkflowType WorkflowType { get; } = new();

    /// <summary>
    ///     Contains constant strings for all known Azure DevOps environment variable names.
    /// </summary>
    [PublicAPI]
    public static class VariableNames
    {
        /// <summary>
        ///     Indicates if debug logging is enabled for the build.
        /// </summary>
        public const string SystemDebug = "SYSTEM_DEBUG";

        /// <summary>
        ///     The local path on the agent where the build artifacts are copied to before being published.
        /// </summary>
        public const string AgentBuildDirectory = "AGENT_BUILDDIRECTORY";

        /// <summary>
        ///     The path to the agent's home directory.
        /// </summary>
        public const string AgentHomeDirectory = "AGENT_HOMEDIRECTORY";

        /// <summary>
        ///     The ID of the agent.
        /// </summary>
        public const string AgentId = "AGENT_ID";

        /// <summary>
        ///     The name of the agent job.
        /// </summary>
        public const string AgentJobName = "AGENT_JOBNAME";

        /// <summary>
        ///     The machine name of the agent.
        /// </summary>
        public const string AgentMachineName = "AGENT_MACHINENAME";

        /// <summary>
        ///     The name of the agent.
        /// </summary>
        public const string AgentName = "AGENT_NAME";

        /// <summary>
        ///     The operating system of the agent.
        /// </summary>
        public const string AgentOs = "AGENT_OS";

        /// <summary>
        ///     The architecture of the agent's operating system.
        /// </summary>
        public const string AgentOsArchitecture = "AGENT_OSARCHITECTURE";

        /// <summary>
        ///     The path to the agent's temporary directory.
        /// </summary>
        public const string AgentTempDirectory = "AGENT_TEMPDIRECTORY";

        /// <summary>
        ///     The path to the agent's tools directory.
        /// </summary>
        public const string AgentToolsDirectory = "AGENT_TOOLSDIRECTORY";

        /// <summary>
        ///     The path to the agent's work folder.
        /// </summary>
        public const string AgentWorkFolder = "AGENT_WORKFOLDER";

        /// <summary>
        ///     The local path on the agent where the build artifacts are copied to before being published.
        /// </summary>
        public const string BuildArtifactStagingDirectory = "BUILD_ARTIFACTSTAGINGDIRECTORY";

        /// <summary>
        ///     The ID of the current build.
        /// </summary>
        public const string BuildBuildId = "BUILD_BUILDID";

        /// <summary>
        ///     The build number of the current build.
        /// </summary>
        public const string BuildBuildNumber = "BUILD_BUILDNUMBER";

        /// <summary>
        ///     The URI of the current build.
        /// </summary>
        public const string BuildBuildUri = "BUILD_BUILDURI";

        /// <summary>
        ///     The local path on the agent where compiled binaries are placed.
        /// </summary>
        public const string BuildBinariesDirectory = "BUILD_BINARIESDIRECTORY";

        /// <summary>
        ///     The ID of the container that ran the build.
        /// </summary>
        public const string BuildContainerId = "BUILD_CONTAINERID";

        /// <summary>
        ///     The display name of the cron schedule that triggered the build.
        /// </summary>
        public const string BuildCronScheduleDisplayName = "BUILD_CRONSCHEDULE_DISPLAYNAME";

        /// <summary>
        ///     The name of the build definition.
        /// </summary>
        public const string BuildDefinitionName = "BUILD_DEFINITIONNAME";

        /// <summary>
        ///     The name of the user who queued the build.
        /// </summary>
        public const string BuildQueuedBy = "BUILD_QUEUEDBY";

        /// <summary>
        ///     The ID of the user who queued the build.
        /// </summary>
        public const string BuildQueuedById = "BUILD_QUEUEDBYID";

        /// <summary>
        ///     The reason the build was triggered.
        /// </summary>
        public const string BuildReason = "BUILD_REASON";

        /// <summary>
        ///     Indicates if the repository was cleaned before the build.
        /// </summary>
        public const string BuildRepositoryClean = "BUILD_REPOSITORY_CLEAN";

        /// <summary>
        ///     The local path on the agent where the repository is cloned.
        /// </summary>
        public const string BuildRepositoryLocalPath = "BUILD_REPOSITORY_LOCALPATH";

        /// <summary>
        ///     The ID of the repository.
        /// </summary>
        public const string BuildRepositoryId = "BUILD_REPOSITORY_ID";

        /// <summary>
        ///     The name of the repository.
        /// </summary>
        public const string BuildRepositoryName = "BUILD_REPOSITORY_NAME";

        /// <summary>
        ///     The type of the repository (e.g., `TfsGit`, `GitHub`).
        /// </summary>
        public const string BuildRepositoryProvider = "BUILD_REPOSITORY_PROVIDER";

        /// <summary>
        ///     The TFVC workspace name.
        /// </summary>
        public const string BuildRepositoryTfvcWorkspace = "BUILD_REPOSITORY_TFVC_WORKSPACE";

        /// <summary>
        ///     The URI of the repository.
        /// </summary>
        public const string BuildRepositoryUri = "BUILD_REPOSITORY_URI";

        /// <summary>
        ///     The name of the user who requested the build.
        /// </summary>
        public const string BuildRequestedFor = "BUILD_REQUESTEDFOR";

        /// <summary>
        ///     The email of the user who requested the build.
        /// </summary>
        public const string BuildRequestedForEmail = "BUILD_REQUESTEDFOREMAIL";

        /// <summary>
        ///     The ID of the user who requested the build.
        /// </summary>
        public const string BuildRequestedForId = "BUILD_REQUESTEDFORID";

        /// <summary>
        ///     The full Git ref of the source branch (e.g., `refs/heads/main`).
        /// </summary>
        public const string BuildSourceBranch = "BUILD_SOURCEBRANCH";

        /// <summary>
        ///     The short name of the source branch (e.g., `main`).
        /// </summary>
        public const string BuildSourceBranchName = "BUILD_SOURCEBRANCHNAME";

        /// <summary>
        ///     The local path on the agent where the source code is downloaded.
        /// </summary>
        public const string BuildSourcesDirectory = "BUILD_SOURCESDIRECTORY";

        /// <summary>
        ///     The commit ID of the source version.
        /// </summary>
        public const string BuildSourceVersion = "BUILD_SOURCEVERSION";

        /// <summary>
        ///     The commit message of the source version.
        /// </summary>
        public const string BuildSourceVersionMessage = "BUILD_SOURCEVERSIONMESSAGE";

        /// <summary>
        ///     The local path on the agent where staging files are placed.
        /// </summary>
        public const string BuildStagingDirectory = "BUILD_STAGINGDIRECTORY";

        /// <summary>
        ///     Indicates if Git submodules were checked out.
        /// </summary>
        public const string BuildRepositoryGitSubmoduleCheckout = "BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT";

        /// <summary>
        ///     The TFVC shelveset name.
        /// </summary>
        public const string BuildSourceTfvcShelveset = "BUILD_SOURCETFVC_SHELVESET";

        /// <summary>
        ///     The ID of the build that triggered the current build.
        /// </summary>
        public const string BuildTriggeredByBuildId = "BUILD_TRIGGEREDBY_BUILDID";

        /// <summary>
        ///     The definition ID of the build that triggered the current build.
        /// </summary>
        public const string BuildTriggeredByDefinitionId = "BUILD_TRIGGEREDBY_DEFINITIONID";

        /// <summary>
        ///     The definition name of the build that triggered the current build.
        /// </summary>
        public const string BuildTriggeredByDefinitionName = "BUILD_TRIGGEREDBY_DEFINITIONNAME";

        /// <summary>
        ///     The build number of the build that triggered the current build.
        /// </summary>
        public const string BuildTriggeredByBuildNumber = "BUILD_TRIGGEREDBY_BUILDNUMBER";

        /// <summary>
        ///     The project ID of the build that triggered the current build.
        /// </summary>
        public const string BuildTriggeredByProjectId = "BUILD_TRIGGEREDBY_PROJECTID";

        /// <summary>
        ///     The common directory for test results.
        /// </summary>
        public const string CommonTestResultsDirectory = "COMMON_TESTRESULTSDIRECTORY";

        /// <summary>
        ///     The name of the environment.
        /// </summary>
        public const string EnvironmentName = "ENVIRONMENT_NAME";

        /// <summary>
        ///     The ID of the environment.
        /// </summary>
        public const string EnvironmentId = "ENVIRONMENT_ID";

        /// <summary>
        ///     The name of the environment resource.
        /// </summary>
        public const string EnvironmentResourceName = "ENVIRONMENT_RESOURCENAME";

        /// <summary>
        ///     The ID of the environment resource.
        /// </summary>
        public const string EnvironmentResourceId = "ENVIRONMENT_RESOURCEID";

        /// <summary>
        ///     The name of the deployment strategy.
        /// </summary>
        public const string StrategyName = "STRATEGY_NAME";

        /// <summary>
        ///     The cycle name of the deployment strategy.
        /// </summary>
        public const string StrategyCycleName = "STRATEGY_CYCLENAME";

        /// <summary>
        ///     The OAuth token used for accessing Azure DevOps REST APIs.
        /// </summary>
        public const string SystemAccessToken = "SYSTEM_ACCESSTOKEN";

        /// <summary>
        ///     The ID of the Azure DevOps collection.
        /// </summary>
        public const string SystemCollectionId = "SYSTEM_COLLECTIONID";

        /// <summary>
        ///     The URI of the Azure DevOps collection.
        /// </summary>
        public const string SystemCollectionUri = "SYSTEM_COLLECTIONURI";

        /// <summary>
        ///     The default working directory for the build.
        /// </summary>
        public const string SystemDefaultWorkingDirectory = "SYSTEM_DEFAULTWORKINGDIRECTORY";

        /// <summary>
        ///     The ID of the build definition.
        /// </summary>
        public const string SystemDefinitionId = "SYSTEM_DEFINITIONID";

        /// <summary>
        ///     The host type of the system.
        /// </summary>
        public const string SystemHostType = "SYSTEM_HOSTTYPE";

        /// <summary>
        ///     The attempt number of the current job.
        /// </summary>
        public const string SystemJobAttempt = "SYSTEM_JOBATTEMPT";

        /// <summary>
        ///     The display name of the current job.
        /// </summary>
        public const string SystemJobDisplayName = "SYSTEM_JOBDISPLAYNAME";

        /// <summary>
        ///     The ID of the current job.
        /// </summary>
        public const string SystemJobId = "SYSTEM_JOBID";

        /// <summary>
        ///     The name of the current job.
        /// </summary>
        public const string SystemJobName = "SYSTEM_JOBNAME";

        /// <summary>
        ///     The OIDC request URI.
        /// </summary>
        public const string SystemOidcRequestUri = "SYSTEM_OIDCREQUESTURI";

        /// <summary>
        ///     The attempt number of the current phase.
        /// </summary>
        public const string SystemPhaseAttempt = "SYSTEM_PHASEATTEMPT";

        /// <summary>
        ///     The display name of the current phase.
        /// </summary>
        public const string SystemPhaseDisplayName = "SYSTEM_PHASEDISPLAYNAME";

        /// <summary>
        ///     The name of the current phase.
        /// </summary>
        public const string SystemPhaseName = "SYSTEM_PHASENAME";

        /// <summary>
        ///     The ID of the current plan.
        /// </summary>
        public const string SystemPlanId = "SYSTEM_PLANID";

        /// <summary>
        ///     Indicates if the pull request is from a fork.
        /// </summary>
        public const string SystemPullRequestIsFork = "SYSTEM_PULLREQUEST_ISFORK";

        /// <summary>
        ///     The ID of the pull request.
        /// </summary>
        public const string SystemPullRequestPullRequestId = "SYSTEM_PULLREQUEST_PULLREQUESTID";

        /// <summary>
        ///     The number of the pull request.
        /// </summary>
        public const string SystemPullRequestPullRequestNumber = "SYSTEM_PULLREQUEST_PULLREQUESTNUMBER";

        /// <summary>
        ///     The name of the target branch for the pull request.
        /// </summary>
        public const string SystemPullRequestTargetBranchName = "SYSTEM_PULLREQUEST_TARGETBRANCHNAME";

        /// <summary>
        ///     The source branch of the pull request.
        /// </summary>
        public const string SystemPullRequestSourceBranch = "SYSTEM_PULLREQUEST_SOURCEBRANCH";

        /// <summary>
        ///     The commit ID of the source branch for the pull request.
        /// </summary>
        public const string SystemPullRequestSourceCommitId = "SYSTEM_PULLREQUEST_SOURCECOMMITID";

        /// <summary>
        ///     The URI of the source repository for the pull request.
        /// </summary>
        public const string SystemPullRequestSourceRepositoryUri = "SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI";

        /// <summary>
        ///     The target branch of the pull request.
        /// </summary>
        public const string SystemPullRequestTargetBranch = "SYSTEM_PULLREQUEST_TARGETBRANCH";

        /// <summary>
        ///     The attempt number of the current pull request stage.
        /// </summary>
        public const string SystemPullRequestStageAttempt = "SYSTEM_PULLREQUEST_STAGEATTEMPT";

        /// <summary>
        ///     The display name of the current pull request stage.
        /// </summary>
        public const string SystemPullRequestStageDisplayName = "SYSTEM_PULLREQUEST_STAGEDISPLAYNAME";

        /// <summary>
        ///     The name of the current pull request stage.
        /// </summary>
        public const string SystemPullRequestStageName = "SYSTEM_PULLREQUEST_STAGENAME";

        /// <summary>
        ///     The URI of the Team Foundation Server collection.
        /// </summary>
        public const string SystemTeamFoundationCollectionUri = "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI";

        /// <summary>
        ///     The name of the Team Project.
        /// </summary>
        public const string SystemTeamProject = "SYSTEM_TEAMPROJECT";

        /// <summary>
        ///     The ID of the Team Project.
        /// </summary>
        public const string SystemTeamProjectId = "SYSTEM_TEAMPROJECTID";

        /// <summary>
        ///     The ID of the timeline.
        /// </summary>
        public const string SystemTeamTimelineId = "SYSTEM_TIMELINEID";

        /// <summary>
        ///     Indicates if the build is running in Azure DevOps.
        /// </summary>
        public const string TfBuild = "TF_BUILD";

        /// <summary>
        ///     The attempt number of the checks stage.
        /// </summary>
        public const string ChecksStageAttempt = "CHECKS_STAGEATTEMPT";
    }

    /// <summary>
    ///     Provides access to Azure DevOps environment variables as strongly-typed properties.
    /// </summary>
    /// <remarks>
    ///     Each property corresponds to an Azure DevOps environment variable,
    ///     returning its value or an empty string if the variable is not set.
    /// </remarks>
    [PublicAPI]
    public static class Variables
    {
        /// <summary>
        ///     Gets the value of the <c>SYSTEM_DEBUG</c> environment variable.
        /// </summary>
        public static string SystemDebug { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDebug) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_BUILDDIRECTORY</c> environment variable.
        /// </summary>
        public static string AgentBuildDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentBuildDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_HOMEDIRECTORY</c> environment variable.
        /// </summary>
        public static string AgentHomeDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentHomeDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_ID</c> environment variable.
        /// </summary>
        public static string AgentId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_JOBNAME</c> environment variable.
        /// </summary>
        public static string AgentJobName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentJobName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_MACHINENAME</c> environment variable.
        /// </summary>
        public static string AgentMachineName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentMachineName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_NAME</c> environment variable.
        /// </summary>
        public static string AgentName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_OS</c> environment variable.
        /// </summary>
        public static string AgentOs { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentOs) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_OSARCHITECTURE</c> environment variable.
        /// </summary>
        public static string AgentOsArchitecture { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentOsArchitecture) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_TEMPDIRECTORY</c> environment variable.
        /// </summary>
        public static string AgentTempDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentTempDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_TOOLSDIRECTORY</c> environment variable.
        /// </summary>
        public static string AgentToolsDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentToolsDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>AGENT_WORKFOLDER</c> environment variable.
        /// </summary>
        public static string AgentWorkFolder { get; } =
            Environment.GetEnvironmentVariable(VariableNames.AgentWorkFolder) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_ARTIFACTSTAGINGDIRECTORY</c> environment variable.
        /// </summary>
        public static string BuildArtifactStagingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildArtifactStagingDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_BUILDID</c> environment variable.
        /// </summary>
        public static string BuildBuildId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_BUILDNUMBER</c> environment variable.
        /// </summary>
        public static string BuildBuildNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildNumber) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_BUILDURI</c> environment variable.
        /// </summary>
        public static string BuildBuildUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBuildUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_BINARIESDIRECTORY</c> environment variable.
        /// </summary>
        public static string BuildBinariesDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildBinariesDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_CONTAINERID</c> environment variable.
        /// </summary>
        public static string BuildContainerId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildContainerId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_CRONSCHEDULE_DISPLAYNAME</c> environment variable.
        /// </summary>
        public static string BuildCronScheduleDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildCronScheduleDisplayName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_DEFINITIONNAME</c> environment variable.
        /// </summary>
        public static string BuildDefinitionName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildDefinitionName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_QUEUEDBY</c> environment variable.
        /// </summary>
        public static string BuildQueuedBy { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildQueuedBy) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_QUEUEDBYID</c> environment variable.
        /// </summary>
        public static string BuildQueuedById { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildQueuedById) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REASON</c> environment variable.
        /// </summary>
        public static string BuildReason { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildReason) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_CLEAN</c> environment variable.
        /// </summary>
        public static string BuildRepositoryClean { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryClean) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_LOCALPATH</c> environment variable.
        /// </summary>
        public static string BuildRepositoryLocalPath { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryLocalPath) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_ID</c> environment variable.
        /// </summary>
        public static string BuildRepositoryId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_NAME</c> environment variable.
        /// </summary>
        public static string BuildRepositoryName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_PROVIDER</c> environment variable.
        /// </summary>
        public static string BuildRepositoryProvider { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryProvider) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_TFVC_WORKSPACE</c> environment variable.
        /// </summary>
        public static string BuildRepositoryTfvcWorkspace { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryTfvcWorkspace) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_URI</c> environment variable.
        /// </summary>
        public static string BuildRepositoryUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REQUESTEDFOR</c> environment variable.
        /// </summary>
        public static string BuildRequestedFor { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedFor) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REQUESTEDFOREMAIL</c> environment variable.
        /// </summary>
        public static string BuildRequestedForEmail { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedForEmail) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REQUESTEDFORID</c> environment variable.
        /// </summary>
        public static string BuildRequestedForId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRequestedForId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCEBRANCH</c> environment variable.
        /// </summary>
        public static string BuildSourceBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceBranch) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCEBRANCHNAME</c> environment variable.
        /// </summary>
        public static string BuildSourceBranchName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceBranchName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCESDIRECTORY</c> environment variable.
        /// </summary>
        public static string BuildSourcesDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourcesDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCEVERSION</c> environment variable.
        /// </summary>
        public static string BuildSourceVersion { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceVersion) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCEVERSIONMESSAGE</c> environment variable.
        /// </summary>
        public static string BuildSourceVersionMessage { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceVersionMessage) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_STAGINGDIRECTORY</c> environment variable.
        /// </summary>
        public static string BuildStagingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildStagingDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_REPOSITORY_GIT_SUBMODULECHECKOUT</c> environment variable.
        /// </summary>
        public static string BuildRepositoryGitSubmoduleCheckout { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildRepositoryGitSubmoduleCheckout) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_SOURCETFVC_SHELVESET</c> environment variable.
        /// </summary>
        public static string BuildSourceTfvcShelveset { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildSourceTfvcShelveset) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_TRIGGEREDBY_BUILDID</c> environment variable.
        /// </summary>
        public static string BuildTriggeredByBuildId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByBuildId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_TRIGGEREDBY_DEFINITIONID</c> environment variable.
        /// </summary>
        public static string BuildTriggeredByDefinitionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByDefinitionId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_TRIGGEREDBY_DEFINITIONNAME</c> environment variable.
        /// </summary>
        public static string BuildTriggeredByDefinitionName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByDefinitionName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_TRIGGEREDBY_BUILDNUMBER</c> environment variable.
        /// </summary>
        public static string BuildTriggeredByBuildNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByBuildNumber) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>BUILD_TRIGGEREDBY_PROJECTID</c> environment variable.
        /// </summary>
        public static string BuildTriggeredByProjectId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.BuildTriggeredByProjectId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>COMMON_TESTRESULTSDIRECTORY</c> environment variable.
        /// </summary>
        public static string CommonTestResultsDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.CommonTestResultsDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>ENVIRONMENT_NAME</c> environment variable.
        /// </summary>
        public static string EnvironmentName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>ENVIRONMENT_ID</c> environment variable.
        /// </summary>
        public static string EnvironmentId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>ENVIRONMENT_RESOURCENAME</c> environment variable.
        /// </summary>
        public static string EnvironmentResourceName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentResourceName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>ENVIRONMENT_RESOURCEID</c> environment variable.
        /// </summary>
        public static string EnvironmentResourceId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.EnvironmentResourceId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>STRATEGY_NAME</c> environment variable.
        /// </summary>
        public static string StrategyName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.StrategyName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>STRATEGY_CYCLENAME</c> environment variable.
        /// </summary>
        public static string StrategyCycleName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.StrategyCycleName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_ACCESSTOKEN</c> environment variable.
        /// </summary>
        public static string SystemAccessToken { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemAccessToken) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_COLLECTIONID</c> environment variable.
        /// </summary>
        public static string SystemCollectionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemCollectionId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_COLLECTIONURI</c> environment variable.
        /// </summary>
        public static string SystemCollectionUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemCollectionUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_DEFAULTWORKINGDIRECTORY</c> environment variable.
        /// </summary>
        public static string SystemDefaultWorkingDirectory { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDefaultWorkingDirectory) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_DEFINITIONID</c> environment variable.
        /// </summary>
        public static string SystemDefinitionId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemDefinitionId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_HOSTTYPE</c> environment variable.
        /// </summary>
        public static string SystemHostType { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemHostType) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_JOBATTEMPT</c> environment variable.
        /// </summary>
        public static string SystemJobAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobAttempt) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_JOBDISPLAYNAME</c> environment variable.
        /// </summary>
        public static string SystemJobDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobDisplayName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_JOBID</c> environment variable.
        /// </summary>
        public static string SystemJobId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_JOBNAME</c> environment variable.
        /// </summary>
        public static string SystemJobName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemJobName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_OIDCREQUESTURI</c> environment variable.
        /// </summary>
        public static string SystemOidcRequestUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemOidcRequestUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PHASEATTEMPT</c> environment variable.
        /// </summary>
        public static string SystemPhaseAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseAttempt) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PHASEDISPLAYNAME</c> environment variable.
        /// </summary>
        public static string SystemPhaseDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseDisplayName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PHASENAME</c> environment variable.
        /// </summary>
        public static string SystemPhaseName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPhaseName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PLANID</c> environment variable.
        /// </summary>
        public static string SystemPlanId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPlanId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_ISFORK</c> environment variable.
        /// </summary>
        public static string SystemPullRequestIsFork { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestIsFork) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_PULLREQUESTID</c> environment variable.
        /// </summary>
        public static string SystemPullRequestPullRequestId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestPullRequestId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_PULLREQUESTNUMBER</c> environment variable.
        /// </summary>
        public static string SystemPullRequestPullRequestNumber { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestPullRequestNumber) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_TARGETBRANCHNAME</c> environment variable.
        /// </summary>
        public static string SystemPullRequestTargetBranchName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestTargetBranchName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_SOURCEBRANCH</c> environment variable.
        /// </summary>
        public static string SystemPullRequestSourceBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceBranch) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_SOURCECOMMITID</c> environment variable.
        /// </summary>
        public static string SystemPullRequestSourceCommitId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceCommitId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_SOURCEREPOSITORYURI</c> environment variable.
        /// </summary>
        public static string SystemPullRequestSourceRepositoryUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestSourceRepositoryUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_TARGETBRANCH</c> environment variable.
        /// </summary>
        public static string SystemPullRequestTargetBranch { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestTargetBranch) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_STAGEATTEMPT</c> environment variable.
        /// </summary>
        public static string SystemPullRequestStageAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageAttempt) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_STAGEDISPLAYNAME</c> environment variable.
        /// </summary>
        public static string SystemPullRequestStageDisplayName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageDisplayName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_PULLREQUEST_STAGENAME</c> environment variable.
        /// </summary>
        public static string SystemPullRequestStageName { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemPullRequestStageName) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_TEAMFOUNDATIONCOLLECTIONURI</c> environment variable.
        /// </summary>
        public static string SystemTeamFoundationCollectionUri { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamFoundationCollectionUri) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_TEAMPROJECT</c> environment variable.
        /// </summary>
        public static string SystemTeamProject { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamProject) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_TEAMPROJECTID</c> environment variable.
        /// </summary>
        public static string SystemTeamProjectId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamProjectId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>SYSTEM_TIMELINEID</c> environment variable.
        /// </summary>
        public static string SystemTeamTimelineId { get; } =
            Environment.GetEnvironmentVariable(VariableNames.SystemTeamTimelineId) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>TF_BUILD</c> environment variable.
        /// </summary>
        public static string TfBuild { get; } =
            Environment.GetEnvironmentVariable(VariableNames.TfBuild) ?? string.Empty;

        /// <summary>
        ///     Gets the value of the <c>CHECKS_STAGEATTEMPT</c> environment variable.
        /// </summary>
        public static string ChecksStageAttempt { get; } =
            Environment.GetEnvironmentVariable(VariableNames.ChecksStageAttempt) ?? string.Empty;
    }
}
