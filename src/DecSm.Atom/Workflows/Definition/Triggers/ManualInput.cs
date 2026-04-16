namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents the base definition for a manual input trigger in a workflow.
/// </summary>
/// <param name="Name">The programmatic name of the input, used as an identifier.</param>
/// <param name="Description">A user-friendly description of the input.</param>
/// <param name="Required">A value indicating whether this input is required when the workflow is manually triggered.</param>
[PublicAPI]
public abstract record ManualInput(string Name, string Description, bool? Required);
