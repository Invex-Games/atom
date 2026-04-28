namespace DecSm.StructuredText.AzureDevopsPipelines;

/// <summary>
///     Wraps an expression to be evaluated at runtime using Azure DevOps runtime expression syntax: $[ expression ]
/// </summary>
[PublicAPI]
public sealed record DevopsRuntimeExpression(TextExpression Expression) : TextExpression;
