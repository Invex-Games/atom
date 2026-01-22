namespace DecSm.Atom.Params;

/// <summary>
///     Represents the resolved metadata for a parameter, used for reflection and help generation.
/// </summary>
/// <param name="Name">The programmatic (C#) name of the parameter.</param>
[PublicAPI]
public sealed record ParamModel(string Name)
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
    ///     Gets the description of the parameter, used for help documentation.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     Gets the default value of the parameter as a string, if one is defined.
    /// </summary>
    public required string? DefaultValue { get; init; }

    /// <summary>
    ///     Gets the configured sources from which the parameter can be resolved.
    /// </summary>
    public required ParamSource Sources { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the parameter is a secret.
    /// </summary>
    public required bool IsSecret { get; init; }

    /// <summary>
    ///     Gets a list of other parameters that this parameter's resolution depends on.
    /// </summary>
    public required IReadOnlyList<string> ChainedParams { get; init; }
}
