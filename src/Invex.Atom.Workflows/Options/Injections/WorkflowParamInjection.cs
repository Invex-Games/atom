namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a parameter value, produced by a workflow expression, into a target's step.
/// </summary>
/// <param name="ParamName">The name of the parameter to inject.</param>
/// <param name="Value">The expression that produces the parameter value in the workflow.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.Param(paramName, value)</c>. The injected value takes precedence
///     over a value injected from the workflow environment for the same parameter.
/// </remarks>
/// <example>
///     <code>
/// new WorkflowTargetDefinition(nameof(PushToNuget))
/// {
///     Options = [BuildOptions.Inject.Param(nameof(NugetDryRun), "true")],
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowParamInjection(string ParamName, TextExpression Value) : IBuildOption;
