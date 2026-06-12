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
        public WorkflowParamInjection Param(string paramName, TextExpression injectionExpression) =>
            new(paramName, injectionExpression);

        /// <summary>
        ///     Injects a parameter value into the workflow using the provided expression.
        /// </summary>
        /// <param name="paramDefinition">The definition of the parameter to inject.</param>
        /// <param name="injectionExpression">The expression that produces the parameter value in the workflow.</param>
        /// <returns>A <see cref="WorkflowParamInjection" /> option.</returns>
        public WorkflowParamInjection Param(ParamDefinition paramDefinition, TextExpression injectionExpression) =>
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
        public WorkflowEnvironmentVariableInjection EnvironmentVariable(string name, TextExpression value) =>
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
        /// <returns>A <see cref="WorkflowSecretsInjectionFromEnvironment" /> option.</returns>
        public WorkflowSecretsInjectionFromEnvironment SecretFromWorkflowEnvironment(string secretName) =>
            new(secretName);

        /// <summary>
        ///     Injects a secret whose value is sourced from the workflow's environment.
        /// </summary>
        /// <param name="secretDefinition">The definition of the secret parameter to inject.</param>
        /// <returns>A <see cref="WorkflowSecretsInjectionFromEnvironment" /> option.</returns>
        public WorkflowSecretsInjectionFromEnvironment SecretFromWorkflowEnvironment(
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
        public DeployToEnvironment ToEnvironment(TextExpression environmentName) =>
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
        public TargetCondition RunIfWorkflowCondition(TextExpression condition) =>
            new(condition);
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
        public class SetupDotnetOptions
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
                TextExpression? dotnetVersion = null,
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
