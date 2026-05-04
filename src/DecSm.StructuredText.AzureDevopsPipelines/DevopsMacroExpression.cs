namespace DecSm.StructuredText.AzureDevopsPipelines;

/// <summary>
///     Wraps a variable name to be expanded using Azure DevOps macro syntax: $(variableName)
/// </summary>
[PublicAPI]
[SuppressMessage("Design", "CA1067:Override Object.Equals(object) when implementing IEquatable<T>")]
public sealed record DevopsMacroExpression(TextExpression Variable) : TextExpression;
