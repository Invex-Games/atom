namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manual input in a workflow where the user selects a value from a predefined list of choices.
/// </summary>
/// <param name="Name">The name of the input field.</param>
/// <param name="Description">A human-readable description of the input.</param>
/// <param name="Required">A value indicating whether this input is mandatory.</param>
/// <param name="Choices">A read-only list of string options the user can choose from.</param>
/// <param name="DefaultValue">An optional default value that will be pre-selected.</param>
[PublicAPI]
public sealed record ManualChoiceInput(
    string Name,
    string Description,
    bool? Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a <see cref="ManualChoiceInput" /> instance based on a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to create the input from.</param>
    /// <param name="choices">The list of choices for this input.</param>
    /// <param name="required">An optional boolean to override the required status.</param>
    /// <param name="defaultValue">The default value for this input.</param>
    /// <returns>A new instance of <see cref="ManualChoiceInput" />.</returns>
    public static ManualChoiceInput ForParam(
        ParamDefinition paramDefinition,
        IReadOnlyList<string> choices,
        bool? required = null,
        string? defaultValue = null) =>
        new(paramDefinition.ArgName, paramDefinition.Description, required, choices, defaultValue);
}
