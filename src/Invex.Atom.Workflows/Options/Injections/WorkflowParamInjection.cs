namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     Represents a workflow option that injects a parameter value into the workflow execution context.
/// </summary>
/// <param name="Name">The name of the parameter to inject.</param>
/// <param name="InjectionExpression">The value to inject for the specified parameter.</param>
/// <remarks>
///     This allows workflows to set specific parameter values that take precedence over other sources
///     like command-line arguments or environment variables.
/// </remarks>
/// <example>
///     <code>
/// // Inject a dry-run parameter
/// var dryRunInjection = new WorkflowParamInjection("NugetDryRun", "true");
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition().WithAddedOptions(dryRunInjection);
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowParamInjection(string Name, TextExpression InjectionExpression) : IBuildOption;
