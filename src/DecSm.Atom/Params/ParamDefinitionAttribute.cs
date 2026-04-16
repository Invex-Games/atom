namespace DecSm.Atom.Params;

// NOTE: If the constructor args are modified, the BuildDefinitionSourceGenerator will need to be updated

/// <summary>
///     Defines a parameter for a build target, specifying its command-line name, description, and resolution sources.
/// </summary>
/// <remarks>
///     This attribute is used on properties within target definition interfaces to declare parameters.
///     For sensitive values, use <see cref="SecretDefinitionAttribute" /> instead.
/// </remarks>
/// <example>
///     <code>
/// public interface IMyTarget
/// {
///     [ParamDefinition("my-param", "A description of my parameter.")]
///     string MyParam => GetParam(() => MyParam);
/// }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public class ParamDefinitionAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ParamDefinitionAttribute" /> class.
    /// </summary>
    /// <param name="argName">The name of the parameter on the command line (e.g., "my-param").</param>
    /// <param name="description">A description of the parameter for help text.</param>
    /// <param name="sources">The sources from which the parameter can be resolved.</param>
    /// <param name="chainedParams">An array of other parameters that this parameter depends on.</param>
    public ParamDefinitionAttribute(
        string argName,
        string description,
        ParamSource sources = ParamSource.All,
        string[]? chainedParams = null)
    {
        ArgName = argName;
        Description = description;
        Sources = sources;
        ChainedParams = chainedParams ?? [];
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
    public bool IsSecret { get; protected init; }

    /// <summary>
    ///     Gets an array of other parameters that this parameter depends on.
    /// </summary>
    public string[] ChainedParams { get; }
}
