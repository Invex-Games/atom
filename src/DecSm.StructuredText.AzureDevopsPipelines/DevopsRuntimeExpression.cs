namespace DecSm.StructuredText.AzureDevopsPipelines;

/// <summary>
///     Wraps an expression to be evaluated at runtime using Azure DevOps runtime expression syntax: $[ expression ]
/// </summary>
[PublicAPI]
[SuppressMessage("Design", "CA1067:Override Object.Equals(object) when implementing IEquatable<T>")]
public sealed record DevopsRuntimeExpression(TextExpression Expression) : TextExpression;
