namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manually-triggered workflow input that accepts a boolean value.
/// </summary>
/// <param name="Name">The name of the input parameter.</param>
/// <param name="Description">A human-readable description of the input parameter.</param>
/// <param name="Required">A value indicating whether this input parameter must be provided.</param>
/// <param name="DefaultValue">The default value for the input parameter if no value is explicitly provided.</param>
[PublicAPI]
public sealed record ManualBoolInput(string Name, string Description, bool? Required, bool? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a new <see cref="ManualBoolInput" /> instance from a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to create the input from.</param>
    /// <param name="required">
    ///     Overrides the required status. If null, the required status is inferred from the parameter's default value.
    /// </param>
    /// <param name="defaultValue">Overrides the default value. If null, the default value is inferred from the parameter.</param>
    /// <returns>A new <see cref="ManualBoolInput" /> instance.</returns>
    public static ManualBoolInput ForParam(
        ParamDefinition paramDefinition,
        bool? required = null,
        bool? defaultValue = null) =>
        new(paramDefinition.ArgName, paramDefinition.Description, required, defaultValue);
}
