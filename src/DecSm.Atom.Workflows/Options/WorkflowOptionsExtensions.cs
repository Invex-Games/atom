namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public sealed class InjectionOptions
    {
        internal static InjectionOptions Instance { get; } = new();

        public WorkflowParamInjection Param(string paramName, TextExpression injectionExpression) =>
            new(paramName, injectionExpression);

        public WorkflowParamInjection Param(ParamDefinition paramDefinition, TextExpression injectionExpression) =>
            new(paramDefinition, injectionExpression);

        public WorkflowParamInjectionFromEnvironment ParamFromWorkflowEnvironment(string name) =>
            new(name);

        public WorkflowEnvironmentVariableInjection EnvironmentVariable(string name, TextExpression value) =>
            new(name, value);

        public WorkflowSecretInjection Secret(string secretName) =>
            new(secretName);

        public WorkflowSecretInjection Secret(ParamDefinition secretDefinition) =>
            new(secretDefinition);

        public WorkflowSecretInjectionForSecretProvider SecretForSecretProvider(string secretName) =>
            new(secretName);

        public WorkflowSecretInjectionForSecretProvider SecretForSecretProvider(ParamDefinition secretDefinition) =>
            new(secretDefinition);

        public WorkflowSecretsInjectionFromEnvironment SecretFromWorkflowEnvironment(string secretName) =>
            new(secretName);

        public WorkflowSecretsInjectionFromEnvironment SecretFromWorkflowEnvironment(
            ParamDefinition secretDefinition) =>
            new(secretDefinition);
    }

    [PublicAPI]
    public sealed class ArtifactOptions
    {
        internal static ArtifactOptions Instance { get; } = new();

        public UseCustomArtifactProvider UseCustomProvider =>
            new()
            {
                Enabled = true,
            };
    }

    [PublicAPI]
    public sealed class DeploymentOptions
    {
        internal static DeploymentOptions Instance { get; } = new();

        public DeployToEnvironment ToEnvironment(TextExpression environmentName) =>
            new(environmentName);
    }

    [PublicAPI]
    public sealed class TargetOptions
    {
        internal static TargetOptions Instance { get; } = new();

        public SuppressArtifactPublishingOption SuppressArtifactPublishing =>
            new()
            {
                Enabled = true,
            };

        public SuppressArtifactPublishingOption SetSuppressedArtifactPublishing(bool value) =>
            new()
            {
                Enabled = value,
            };

        public TargetCondition RunIfWorkflowCondition(TextExpression condition) =>
            new(condition);
    }

    [PublicAPI]
    public sealed class StepsOptions
    {
        internal static StepsOptions Instance { get; } = new();

        [PublicAPI]
        public SetupDotnetOptions SetupDotnet => field ??= new();

        [PublicAPI]
        public AddNugetFeedsOptions AddNugetFeeds => field ??= new();

        [PublicAPI]
        public class SetupDotnetOptions
        {
            public SetupDotnetStep Dotnet80X(bool cache = false, string? lockFile = null) =>
                new("8.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            public SetupDotnetStep Dotnet90X(bool cache = false, string? lockFile = null) =>
                new("9.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            public SetupDotnetStep Dotnet100X(bool cache = false, string? lockFile = null) =>
                new("10.0.x")
                {
                    Cache = cache,
                    LockFile = lockFile,
                };

            public SetupDotnetStep From(
                TextExpression? dotnetVersion = null,
                SetupDotnetStep.DotnetQuality? quality = null,
                bool cache = false,
                string? lockFile = null) =>
                new(dotnetVersion, quality, cache, lockFile);
        }

        [PublicAPI]
        public sealed class AddNugetFeedsOptions
        {
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
        [PublicAPI]
        public static InjectionOptions Inject => InjectionOptions.Instance;

        [PublicAPI]
        public static ArtifactOptions Artifacts => ArtifactOptions.Instance;

        [PublicAPI]
        public static DeploymentOptions Deploy => DeploymentOptions.Instance;

        [PublicAPI]
        public static TargetOptions Target => TargetOptions.Instance;

        [PublicAPI]
        public static StepsOptions Steps => StepsOptions.Instance;
    }
}
