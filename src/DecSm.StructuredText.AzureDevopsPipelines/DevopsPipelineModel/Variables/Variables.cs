namespace DecSm.StructuredText.AzureDevopsPipelines.DevopsPipelineModel.Variables;

/// <summary>
///     Define variables using name/value pairs or variable lists.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - Dictionary: Define variables using simple name/value string pairs
///     - VariableList: Define variables by name, variable group, or template reference
/// </remarks>
[PublicAPI]
[Union]
public partial record Variables
{
    /// <summary>
    ///     Define variables using name/value pairs (mapping syntax).
    /// </summary>
    public sealed partial record Dictionary
    {
        /// <summary>
        ///     Name/value pairs for variables.
        /// </summary>
        public required IReadOnlyDictionary<string, TextExpression> Values { get; init; }
    }

    /// <summary>
    ///     Define variables by name, variable group, or in a template (list syntax).
    /// </summary>
    public sealed partial record VariableList
    {
        /// <summary>
        ///     List of variable definitions (name, group, or template).
        /// </summary>
        public required IReadOnlyList<Variable> Values { get; init; }
    }
}

/// <summary>
///     A variable definition that can be a named variable, variable group, or template reference.
/// </summary>
/// <remarks>
///     Supports three implementations:
///     - Name: A single named variable with value
///     - Group: Reference to a variable group
///     - Template: Reference to a variable template file
/// </remarks>
[PublicAPI]
[Union]
public partial record Variable
{
    /// <summary>
    ///     A named variable with a value.
    /// </summary>
    public sealed partial record Name
    {
        /// <summary>
        ///     Variable name.
        /// </summary>
        public required WorkflowExpression<string> VariableName { get; init; }

        /// <summary>
        ///     Variable value.
        /// </summary>
        public required WorkflowExpression<string> Value { get; init; }

        /// <summary>
        ///     Whether the variable is read-only.
        /// </summary>
        public WorkflowExpression<bool>? ReadOnly { get; init; }
    }

    /// <summary>
    ///     Reference to a variable group.
    /// </summary>
    public sealed partial record Group
    {
        /// <summary>
        ///     Name of the variable group.
        /// </summary>
        public required WorkflowExpression<string> GroupName { get; init; }
    }

    /// <summary>
    ///     Reference to a variable template file.
    /// </summary>
    public sealed partial record Template
    {
        /// <summary>
        ///     Path to the template file.
        /// </summary>
        public required WorkflowExpression<string> TemplatePath { get; init; }

        /// <summary>
        ///     Parameters to pass to the template.
        /// </summary>
        public IReadOnlyDictionary<string, TextExpression>? Parameters { get; init; }
    }
}
