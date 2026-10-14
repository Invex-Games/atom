namespace Invex.Atom.Build.Params;

/// <summary>
///     The common base for <see cref="ParamDefinitionAttribute" /> and <see cref="SecretDefinitionAttribute" />.
/// </summary>
/// <remarks>
///     This type cannot be derived from outside Atom: the source generator recognises only the two sealed
///     attributes, so a custom subclass would silently define no parameter.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public abstract class ParamDefinitionAttributeBase : Attribute
{
    // NOTE: If the constructor args are modified, the BuildDefinitionSourceGenerator will need to be updated
    private protected ParamDefinitionAttributeBase(
        string argName,
        string description,
        ParamSource sources,
        string[]? chainedParams,
        bool isSecret)
    {
        ArgName = argName;
        Description = description;
        Sources = sources;
        ChainedParams = chainedParams ?? [];
        IsSecret = isSecret;
    }

    /// <summary>
    ///     Gets the name of the parameter on the command line.
    /// </summary>
    public string ArgName { get; }

    /// <summary>
    ///     Gets the description of the parameter.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Gets the sources from which the parameter can be resolved.
    /// </summary>
    public ParamSource Sources { get; }

    /// <summary>
    ///     Gets a value indicating whether the parameter is a secret.
    /// </summary>
    public bool IsSecret { get; }

    /// <summary>
    ///     Gets the other parameters that this parameter depends on.
    /// </summary>
    public IReadOnlyList<string> ChainedParams { get; }
}
