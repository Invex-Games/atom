namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that is initiated manually, optionally with a predefined set of inputs.
/// </summary>
/// <param name="Inputs">A read-only list of manual inputs required when the workflow is triggered.</param>
[PublicAPI]
public sealed record ManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
