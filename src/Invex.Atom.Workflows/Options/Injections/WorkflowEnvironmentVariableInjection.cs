namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects an environment variable with the provided value expression into
///     the workflow execution context.
/// </summary>
/// <param name="Name">The name of the environment variable.</param>
/// <param name="Value">The expression that produces the variable value in the workflow.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.EnvironmentVariable(name, value)</c>.
/// </remarks>
[PublicAPI]
public sealed record WorkflowEnvironmentVariableInjection(string Name, TextExpression Value) : IBuildOption;
