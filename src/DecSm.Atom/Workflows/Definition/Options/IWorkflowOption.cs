namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option that can be applied to control workflow behavior and configuration.
/// </summary>
/// <remarks>
///     This interface is the base for all workflow configuration options, including custom steps and parameter injections.
///     It provides built-in support for option merging and deduplication.
/// </remarks>
[PublicAPI]
public interface IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating whether multiple instances of this option are allowed in a workflow.
    /// </summary>
    /// <remarks>
    ///     If <c>false</c> (default), only the last occurrence of this option type is retained during merging.
    ///     If <c>true</c>, all instances are preserved.
    /// </remarks>
    bool AllowMultiple => false;

    /// <summary>
    ///     Retrieves and merges all workflow options applicable to the currently running workflows.
    /// </summary>
    /// <param name="buildDefinition">The build definition containing global options and active workflows.</param>
    /// <returns>A merged collection of workflow options from global and active workflow configurations.</returns>
    static IEnumerable<IWorkflowOption> GetOptionsForCurrentTarget(IBuildDefinition buildDefinition) =>
        Merge(buildDefinition.GlobalWorkflowOptions.Concat(buildDefinition
            .Workflows
            .Where(workflow => workflow.WorkflowTypes.Any(workflowType => workflowType.IsRunning))
            .SelectMany(workflow => workflow.Options)));

    /// <summary>
    ///     Merges a collection of workflow options, handling deduplication based on the <see cref="AllowMultiple" /> property.
    /// </summary>
    /// <typeparam name="T">The type of workflow option being merged.</typeparam>
    /// <param name="entries">The collection of workflow options to merge.</param>
    /// <returns>A collection of merged options with duplicates resolved.</returns>
    static IEnumerable<T> Merge<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .GroupBy(x => x.GetType())
            .SelectMany(x => x
                .First()
                .MergeWith(x.Skip(1)));

    /// <summary>
    ///     Merges the current option with additional instances of the same type, respecting the <see cref="AllowMultiple" />
    ///     configuration.
    /// </summary>
    /// <typeparam name="T">The type of workflow option being merged.</typeparam>
    /// <param name="entries">Additional instances of the same option type to merge.</param>
    /// <returns>
    ///     A collection containing either all instances (if <see cref="AllowMultiple" /> is <c>true</c>)
    ///     or only the last instance (if <c>false</c>).
    /// </returns>
    private IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries.ToArray() is { Length: > 0 } entriesArray
            ? AllowMultiple
                ? entriesArray.Prepend((T)this)
                : [entriesArray[^1]]
            : [(T)this];
}
