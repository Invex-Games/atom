namespace DecSm.StructuredText.AzureDevopsPipelines;

/// <summary>
///     Wraps a variable name to be expanded using Azure DevOps macro syntax: $(variableName)
/// </summary>
[PublicAPI]
public sealed record DevopsMacroExpression(TextExpression Variable) : TextExpression;
