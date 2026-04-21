namespace DecSm.Atom.Workflows.Options.Injections;

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
public sealed record WorkflowParamInjection(string Name, WorkflowExpression InjectionExpression) : IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating that multiple instances of this option are allowed.
    /// </summary>
    public bool AllowMultiple => true;

    /// <summary>
    ///     Merges multiple <see cref="WorkflowParamInjection" /> instances, ensuring that for each parameter name,
    ///     only the last injected value is retained.
    /// </summary>
    /// <typeparam name="T">The type of workflow option being merged.</typeparam>
    /// <param name="entries">The collection of workflow options to merge.</param>
    /// <returns>A collection of merged options with the latest value for each parameter.</returns>
    public static IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .OfType<WorkflowParamInjection>()
            .GroupBy(x => x.Name)
            .Select(x => x.Last())
            .Cast<T>();
}
