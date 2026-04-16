namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     An abstract base record for defining a custom step within a workflow.
/// </summary>
/// <remarks>
///     Inherit from this record to create specific custom workflow steps.
///     <code>
/// public sealed record MyCustomActionStep(string ActionSetting) : CustomStep;
///     </code>
/// </remarks>
[PublicAPI]
public abstract record CustomStep : IWorkflowOption
{
    /// <summary>
    ///     Gets the optional name of the custom step, used for identification or logging.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Gets a value indicating whether multiple instances of this step are allowed in a workflow. Defaults to <c>true</c>.
    /// </summary>
    public bool AllowMultiple => true;
}
