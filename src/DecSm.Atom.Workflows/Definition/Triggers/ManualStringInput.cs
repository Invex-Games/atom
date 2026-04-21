namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manually provided string input for a workflow.
/// </summary>
/// <param name="Name">The programmatic name or identifier for the input.</param>
/// <param name="Description">A human-readable description of the input.</param>
/// <param name="Required">A flag indicating whether this input must be provided by the user.</param>
/// <param name="DefaultValue">An optional default value for the string input.</param>
[PublicAPI]
public sealed record ManualStringInput(string Name, string Description, bool? Required, string? DefaultValue)
    : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a <see cref="ManualStringInput" /> instance based on a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to base the input on.</param>
    /// <param name="required">An optional override for the required status.</param>
    /// <param name="defaultValue">An optional override for the default value.</param>
    /// <returns>A new instance of <see cref="ManualStringInput" />.</returns>
    public static ManualStringInput ForParam(
        ParamDefinition paramDefinition,
        bool? required = null,
        string? defaultValue = null) =>
        new(paramDefinition.ArgName, paramDefinition.Description, required, defaultValue);
}
