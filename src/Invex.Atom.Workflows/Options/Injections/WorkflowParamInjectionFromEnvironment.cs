namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a parameter whose value is sourced from the workflow's environment
///     (GitHub Actions <c>vars</c>, Azure DevOps variables).
/// </summary>
/// <param name="ParamName">The name of the parameter to inject from the workflow environment.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.ParamFromWorkflowEnvironment(name)</c>.
///     For sensitive values, use <see cref="WorkflowSecretInjectionFromEnvironment" /> instead.
/// </remarks>
[PublicAPI]
public sealed record WorkflowParamInjectionFromEnvironment(string ParamName) : IBuildOption;
