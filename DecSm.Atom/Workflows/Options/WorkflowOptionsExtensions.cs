namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public sealed class InjectionOptions
    {
        internal static InjectionOptions Instance { get; } = new();

        public WorkflowParamInjection Param(string paramName, WorkflowExpression injectionExpression) =>
            new(paramName, injectionExpression);

        public WorkflowParamInjection Param(ParamDefinition paramDefinition, WorkflowExpression injectionExpression) =>
            new(paramDefinition, injectionExpression);

        public WorkflowParamInjectionFromEnvironment ParamFromWorkflowEnvironment(string name) =>
            new(name);

        public WorkflowEnvironmentVariableInjection EnvironmentVariable(string name, WorkflowExpression value) =>
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
                Value = true,
            };
    }

    [PublicAPI]
    public sealed class DeploymentOptions
    {
        internal static DeploymentOptions Instance { get; } = new();

        public DeployToEnvironment ToEnvironment(WorkflowExpression environmentName) =>
            new(environmentName);
    }

    [PublicAPI]
    public sealed class TargetOptions
    {
        internal static TargetOptions Instance { get; } = new();

        public RunTargetIf RunIfWorkflowCondition(WorkflowExpression condition) =>
            new()
            {
                Condition = condition,
            };
    }

    [PublicAPI]
    public class SetupDotnetOptions
    {
        internal static SetupDotnetOptions Instance { get; } = new();

        public SetupDotnetStep Dotnet80X => field ??= new("8.0.x");

        public SetupDotnetStep Dotnet80XWithCache => field ??= new("8.0.x", Cache: true);

        public SetupDotnetStep Dotnet90X => field ??= new("9.0.x");

        public SetupDotnetStep Dotnet90XWithCache => field ??= new("9.0.x", Cache: true);

        public SetupDotnetStep Dotnet100X => field ??= new("10.0.x");

        public SetupDotnetStep Dotnet100XWithCache => field ??= new("10.0.x", Cache: true);

        public SetupDotnetStep Default => field ??= new();

        public SetupDotnetStep DefaultWithCache => field ??= new(Cache: true);

        public SetupDotnetStep From(
            string? dotnetVersion = null,
            SetupDotnetStep.DotnetQuality? quality = null,
            bool cache = false) =>
            new(dotnetVersion, quality, cache);
    }

    extension(WorkflowOptions)
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
        public static SetupDotnetOptions SetupDotnet => SetupDotnetOptions.Instance;
    }
}
