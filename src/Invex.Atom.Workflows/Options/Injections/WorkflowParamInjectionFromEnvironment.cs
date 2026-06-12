namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a parameter whose value is sourced from the workflow's environment.
/// </summary>
/// <param name="Value">The name of the parameter to inject from the workflow environment.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.ParamFromWorkflowEnvironment(name)</c>.
///     For sensitive values, use <see cref="WorkflowSecretsInjectionFromEnvironment" /> instead.
/// </remarks>
[PublicAPI]
public sealed record WorkflowParamInjectionFromEnvironment(string Value) : IBuildOption;

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
