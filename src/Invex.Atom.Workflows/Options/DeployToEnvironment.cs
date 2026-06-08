namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that specifies the name of the environment to deploy to.
/// </summary>
[PublicAPI]
public sealed record DeployToEnvironment(TextExpression EnvironmentName) : IBuildOption;
