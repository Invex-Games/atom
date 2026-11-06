namespace Invex.Atom.Workflows.Options;

/// <summary>
///     Extends the <see cref="BuildOptions" /> anchor class with fluent factories for workflow build options.
/// </summary>
/// <remarks>
///     These extensions provide the primary discoverable entry points for configuring workflow options:
///     <c>BuildOptions.Inject</c>, <c>BuildOptions.Artifacts</c>, <c>BuildOptions.Deploy</c>,
///     <c>BuildOptions.Target</c>, and <c>BuildOptions.Steps</c>.
/// </remarks>
/// <example>
///     <code>
/// new WorkflowDefinition("Build")
/// {
///     Options =
///     [
///         BuildOptions.Artifacts.UseCustomProvider,
///         BuildOptions.Steps.SetupDotnet.Dotnet90X(),
///         BuildOptions.Inject.Secret(Params.MyApiKey),
///     ],
/// };
///     </code>
/// </example>
[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowBuildOptionsExtensions
{
    /// <summary>
    ///     Provides factories for injecting parameters, secrets, and environment variables into generated workflows.
    /// </summary>
    [PublicAPI]
    public sealed class InjectionBuildOptions
    {
        internal static InjectionBuildOptions Instance { get; } = new();

        /// <summary>
        ///     Injects a parameter value into the workflow using the provided expression.
        /// </summary>
        /// <param name="paramName">The name of the parameter to inject.</param>
        /// <param name="injectionExpression">The expression that produces the parameter value in the workflow.</param>
        /// <returns>A <see cref="WorkflowParamInjection" /> option.</returns>
        public WorkflowParamInjection Param(string paramName, Template injectionExpression) =>
            new(paramName, injectionExpression);

        /// <summary>
        ///     Injects a parameter value into the workflow using the provided expression.
        /// </summary>
        /// <param name="paramDefinition">The definition of the parameter to inject.</param>
        /// <param name="injectionExpression">The expression that produces the parameter value in the workflow.</param>
        /// <returns>A <see cref="WorkflowParamInjection" /> option.</returns>
        public WorkflowParamInjection Param(ParamDefinition paramDefinition, Template injectionExpression) =>
            new(paramDefinition, injectionExpression);

        /// <summary>
        ///     Injects a parameter whose value is sourced from the workflow's environment.
        /// </summary>
        /// <param name="name">The name of the parameter to inject.</param>
        /// <returns>A <see cref="WorkflowParamInjectionFromEnvironment" /> option.</returns>
        public WorkflowParamInjectionFromEnvironment ParamFromWorkflowEnvironment(string name) =>
            new(name);

        /// <summary>
        ///     Injects an environment variable into the workflow with the provided value expression.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <param name="value">The expression that produces the variable value in the workflow.</param>
        /// <returns>A <see cref="WorkflowEnvironmentVariableInjection" /> option.</returns>
        public WorkflowEnvironmentVariableInjection EnvironmentVariable(string name, Template value) =>
            new(name, value);

        /// <summary>
        ///     Injects a secret into the workflow from the platform's secret store.
        /// </summary>
        /// <param name="secretName">The name of the secret to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjection" /> option.</returns>
        public WorkflowSecretInjection Secret(string secretName) =>
            new(secretName);

        /// <summary>
        ///     Injects a secret into the workflow from the platform's secret store.
        /// </summary>
        /// <param name="secretDefinition">The definition of the secret parameter to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjection" /> option.</returns>
        public WorkflowSecretInjection Secret(ParamDefinition secretDefinition) =>
            new(secretDefinition);

        /// <summary>
        ///     Injects a secret that is resolved by a registered <see cref="ISecretsProvider" /> at build time,
        ///     rather than from the platform's secret store.
        /// </summary>
        /// <param name="secretName">The name of the secret to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjectionForSecretProvider" /> option.</returns>
        public WorkflowSecretInjectionForSecretProvider SecretForSecretProvider(string secretName) =>
            new(secretName);

        /// <summary>
        ///     Injects a secret that is resolved by a registered <see cref="ISecretsProvider" /> at build time,
        ///     rather than from the platform's secret store.
        /// </summary>
        /// <param name="secretDefinition">The definition of the secret parameter to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjectionForSecretProvider" /> option.</returns>
        public WorkflowSecretInjectionForSecretProvider SecretForSecretProvider(ParamDefinition secretDefinition) =>
            new(secretDefinition);

        /// <summary>
        ///     Injects a secret whose value is sourced from the workflow's environment.
        /// </summary>
        /// <param name="secretName">The name of the secret to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjectionFromEnvironment" /> option.</returns>
        public WorkflowSecretInjectionFromEnvironment SecretFromWorkflowEnvironment(string secretName) =>
            new(secretName);

        /// <summary>
        ///     Injects a secret whose value is sourced from the workflow's environment.
        /// </summary>
        /// <param name="secretDefinition">The definition of the secret parameter to inject.</param>
        /// <returns>A <see cref="WorkflowSecretInjectionFromEnvironment" /> option.</returns>
        public WorkflowSecretInjectionFromEnvironment SecretFromWorkflowEnvironment(
            ParamDefinition secretDefinition) =>
            new(secretDefinition);
    }

    /// <summary>
    ///     Provides factories for configuring artifact handling in generated workflows.
    /// </summary>
    [PublicAPI]
    public sealed class ArtifactBuildOptions
    {
        internal static ArtifactBuildOptions Instance { get; } = new();

        /// <summary>
        ///     Gets an option that enables the use of a custom <see cref="IArtifactProvider" /> for artifact
        ///     storage and retrieval, instead of the platform's native artifact mechanism.
        /// </summary>
        public UseCustomArtifactProvider UseCustomProvider =>
            new()
            {
                Enabled = true,
            };
    }

    /// <summary>
    ///     Provides factories for configuring deployment behavior in generated workflows.
    /// </summary>
    [PublicAPI]
    public sealed class DeploymentBuildOptions
    {
        internal static DeploymentBuildOptions Instance { get; } = new();

        /// <summary>
        ///     Associates a target with a deployment environment in the generated workflow.
        /// </summary>
        /// <param name="environmentName">The expression that resolves to the environment name.</param>
        /// <returns>A <see cref="DeployToEnvironment" /> option.</returns>
        public DeployToEnvironment ToEnvironment(Template environmentName) =>
            new(environmentName);
    }

    /// <summary>
    ///     Provides factories for configuring per-target behavior in generated workflows.
    /// </summary>
    [PublicAPI]
    public sealed class TargetBuildOptions
    {
        internal static TargetBuildOptions Instance { get; } = new();

        /// <summary>
        ///     Gets an option that suppresses publishing of artifacts produced by the target.
        /// </summary>
        public SuppressArtifactPublishingOption SuppressArtifactPublishing =>
            new()
            {
                Enabled = true,
            };

        /// <summary>
        ///     Creates an option that enables or disables suppression of artifact publishing for the target.
        /// </summary>
        /// <param name="value"><c>true</c> to suppress artifact publishing; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="SuppressArtifactPublishingOption" /> option.</returns>
        public SuppressArtifactPublishingOption SetSuppressedArtifactPublishing(bool value) =>
            new()
            {
                Enabled = value,
            };

        /// <summary>
        ///     Creates an option that gates execution of the target's job on a platform-evaluated condition.
        /// </summary>
        /// <param name="condition">The condition expression to evaluate in the workflow.</param>
        /// <returns>A <see cref="TargetCondition" /> option.</returns>
        public TargetCondition RunIfWorkflowCondition(BoolExpr condition) =>
            new(condition);

        /// <summary>
        ///     Gets an option that runs the target's job only when a job it depends on, directly or transitively, failed,
        ///     or the workflow run was cancelled.
        /// </summary>
        /// <value>
        ///     A <see cref="TargetCondition" /> rendered as <c>failure() || cancelled()</c> on GitHub Actions and
        ///     <c>or(failed(), canceled())</c> on Azure DevOps.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         Use it for a gate target: a single required check that stands for a set of jobs, some of which may be
        ///         skipped by their own conditions (for example, product jobs in a monorepo that only run when their
        ///         product is affected). Give the gate a <c>DependsOn</c> on every job it stands for, make it always
        ///         throw, and condition it with this option:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             When every dependency succeeds or is skipped, the gate is skipped. GitHub treats a skipped required
        ///             check as passed.
        ///         </item>
        ///         <item>
        ///             When any dependency fails, or the run is cancelled, the gate runs, throws and fails the check.
        ///             <c>cancelled()</c> (GitHub) and <c>canceled()</c> (Azure DevOps) test the whole run, not the
        ///             dependencies, so a cancelled run triggers the gate even if every dependency had succeeded.
        ///         </item>
        ///     </list>
        ///     <para>
        ///         Branch protection then only needs to require the gate. Like any <see cref="TargetCondition" />, the
        ///         gate's dependencies must be declared with <c>DependsOn</c> and listed in the workflow; the condition
        ///         does not add them.
        ///     </para>
        ///     <para>
        ///         On Azure DevOps, a custom condition replaces the default <c>succeeded()</c> condition, so this
        ///         option makes the job run only after a failure or a cancelled run, as intended. For the same reason, other
        ///         custom conditions there (such as the product jobs' own) usually need <c>Tex.Success()</c> added
        ///         to keep skipping after an earlier failure.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        /// Target ValidateGate => t => t
        ///     .DependsOn(nameof(PackAppA))
        ///     .DependsOn(nameof(PackAppB))
        ///     .Executes(() => throw new InvalidOperationException("A product job failed or was cancelled."));
        ///
        /// new WorkflowDefinition("Validate")
        /// {
        ///     Targets =
        ///     [
        ///         new(nameof(PackAppA)),
        ///         new(nameof(PackAppB)),
        ///         new(nameof(ValidateGate)) { Options = [BuildOptions.Target.RunIfAnyDependencyFailed] },
        ///     ],
        /// };
        ///     </code>
        /// </example>
        /// <seealso cref="RunIfWorkflowCondition" />
        /// <seealso cref="RunAlways" />
        public TargetCondition RunIfAnyDependencyFailed =>
            new(Tex.Failure() || Tex.Cancelled());

        /// <summary>
        ///     Gets an option that runs the target's job regardless of the result of the jobs it depends on, including
        ///     after a failure or cancellation.
        /// </summary>
        /// <value>
        ///     A <see cref="TargetCondition" /> rendered as <c>always()</c> on both GitHub Actions and Azure DevOps.
        /// </value>
        /// <remarks>
        ///     Use it for jobs that must run once their dependencies have finished, such as reporting or clean-up. On
        ///     Azure DevOps, the condition replaces the default <c>succeeded()</c> condition.
        /// </remarks>
        /// <seealso cref="RunIfWorkflowCondition" />
        /// <seealso cref="RunIfAnyDependencyFailed" />
        public TargetCondition RunAlways =>
            new(Tex.Always());
    }

    /// <summary>
    ///     Provides factories for adding common steps to workflow jobs.
    /// </summary>
    [PublicAPI]
    public sealed class StepsBuildOptions
    {
        internal static StepsBuildOptions Instance { get; } = new();

        /// <summary>
        ///     Gets factories for adding a .NET SDK setup step.
        /// </summary>
        [PublicAPI]
        public SetupDotnetOptions SetupDotnet => field ??= new();

        /// <summary>
        ///     Gets factories for adding a NuGet feed configuration step.
        /// </summary>
        [PublicAPI]
        public AddNugetFeedsOptions AddNugetFeeds => field ??= new();

        /// <summary>
        ///     Provides factories for creating <see cref="SetupDotnetStep" /> options for common .NET SDK versions.
        /// </summary>
        [PublicAPI]
        public sealed class SetupDotnetOptions
        {
            /// <summary>
            ///     Creates a step that installs the latest .NET 8.0 SDK.
            /// </summary>
            /// <param name="cache">Whether to enable NuGet package caching.</param>
            /// <param name="lockFile">An optional path to a lock file used as the cache key.</param>
            /// <returns>A <see cref="SetupDotnetStep" /> option.</returns>
            public SetupDotnetStep Dotnet80X(bool cache = false, string? lockFile = null) =>
                new("8.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            /// <summary>
            ///     Creates a step that installs the latest .NET 9.0 SDK.
            /// </summary>
            /// <param name="cache">Whether to enable NuGet package caching.</param>
            /// <param name="lockFile">An optional path to a lock file used as the cache key.</param>
            /// <returns>A <see cref="SetupDotnetStep" /> option.</returns>
            public SetupDotnetStep Dotnet90X(bool cache = false, string? lockFile = null) =>
                new("9.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            /// <summary>
            ///     Creates a step that installs the latest .NET 10.0 SDK.
            /// </summary>
            /// <param name="cache">Whether to enable NuGet package caching.</param>
            /// <param name="lockFile">An optional path to a lock file used as the cache key.</param>
            /// <returns>A <see cref="SetupDotnetStep" /> option.</returns>
            public SetupDotnetStep Dotnet100X(bool cache = false, string? lockFile = null) =>
                new("10.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            /// <summary>
            ///     Creates a step that installs a custom .NET SDK version.
            /// </summary>
            /// <param name="dotnetVersion">An optional expression that resolves to the SDK version to install.</param>
            /// <param name="quality">An optional SDK quality channel (e.g., preview, GA).</param>
            /// <param name="cache">Whether to enable NuGet package caching.</param>
            /// <param name="lockFile">An optional path to a lock file used as the cache key.</param>
            /// <returns>A <see cref="SetupDotnetStep" /> option.</returns>
            public SetupDotnetStep From(
                Template? dotnetVersion = null,
                SetupDotnetStep.DotnetQuality? quality = null,
                bool cache = false,
                string? lockFile = null) =>
                new(dotnetVersion, quality, cache, lockFile);
        }

        /// <summary>
        ///     Provides factories for creating <see cref="AddNugetFeedsStep" /> options.
        /// </summary>
        [PublicAPI]
        public sealed class AddNugetFeedsOptions
        {
            /// <summary>
            ///     Creates a step that adds the specified NuGet feeds to the workflow job.
            /// </summary>
            /// <param name="feedsToAdd">The NuGet feeds to add.</param>
            /// <param name="syncAtomToolVersionToLibraryVersion">
            ///     Whether to keep the Atom tool version in sync with the referenced library version.
            /// </param>
            /// <returns>An <see cref="AddNugetFeedsStep" /> option.</returns>
            public AddNugetFeedsStep AddNugetFeeds(
                IEnumerable<NugetFeedOptions> feedsToAdd,
                bool syncAtomToolVersionToLibraryVersion = true) =>
                new()
                {
                    FeedsToAdd = feedsToAdd.ToList(),
                    SyncAtomToolVersionToLibraryVersion = syncAtomToolVersionToLibraryVersion,
                };
        }
    }

    extension(BuildOptions)
    {
        /// <summary>
        ///     Gets factories for injecting parameters, secrets, and environment variables into workflows.
        /// </summary>
        [PublicAPI]
        public static InjectionBuildOptions Inject => InjectionBuildOptions.Instance;

        /// <summary>
        ///     Gets factories for configuring artifact handling in workflows.
        /// </summary>
        [PublicAPI]
        public static ArtifactBuildOptions Artifacts => ArtifactBuildOptions.Instance;

        /// <summary>
        ///     Gets factories for configuring deployment behavior in workflows.
        /// </summary>
        [PublicAPI]
        public static DeploymentBuildOptions Deploy => DeploymentBuildOptions.Instance;

        /// <summary>
        ///     Gets factories for configuring per-target behavior in workflows.
        /// </summary>
        [PublicAPI]
        public static TargetBuildOptions Target => TargetBuildOptions.Instance;

        /// <summary>
        ///     Gets factories for adding common steps to workflow jobs.
        /// </summary>
        [PublicAPI]
        public static StepsBuildOptions Steps => StepsBuildOptions.Instance;
    }
}
