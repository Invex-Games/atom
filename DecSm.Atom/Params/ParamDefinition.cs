namespace DecSm.Atom.Params;

/// <summary>
///     Represents the definition of a parameter used at runtime, including its name, source, and other metadata.
/// </summary>
/// <param name="Name">The programmatic (C#) name of the parameter.</param>
[PublicAPI]
public sealed record ParamDefinition(string Name)
{
    /// <summary>
    ///     Gets the name used to specify the parameter on the command line (e.g., "my-param").
    /// </summary>
    public required string ArgName { get; init; }

    [JsonIgnore]
    public string EnvVarName =>
        ArgName
            .Trim()
            .ToUpperInvariant()
            .Replace('-', '_')
            .Replace('.', '_')
            .Replace(' ', '_');

    /// <summary>
    ///     Gets the description of the parameter, used for generating help text.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets the sources from which this parameter can be resolved. Defaults to <see cref="ParamSource.All" />.
    /// </summary>
    public required ParamSource Sources { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the parameter is a secret.
    /// </summary>
    /// <remarks>
    ///     Secret values are masked in logs and can be resolved from <see cref="ISecretsProvider" /> implementations.
    /// </remarks>
    public required bool IsSecret { get; init; }

    /// <summary>
    ///     Gets a list of other parameters that this parameter depends on for its resolution.
    /// </summary>
    public required IReadOnlyList<string> ChainedParams { get; init; }

    public static implicit operator string(ParamDefinition definition) =>
        definition.Name;
}
