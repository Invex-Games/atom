namespace DecSm.Atom.Params;

/// <summary>
///     Defines a secret parameter for a build target, which will be masked in logs and can be resolved from secret stores.
/// </summary>
/// <remarks>
///     This attribute inherits from <see cref="ParamDefinitionAttribute" /> and sets the
///     <see cref="ParamDefinitionAttribute.IsSecret" />
///     property to <c>true</c>. Use this for sensitive values like API keys or passwords.
/// </remarks>
/// <example>
///     <code>
/// public interface IMyTarget
/// {
///     [SecretDefinition("api-key", "The API key for accessing the service.")]
///     string ApiKey => GetParam(() => ApiKey);
/// }
///     </code>
/// </example>
[PublicAPI]
public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SecretDefinitionAttribute" /> class.
    /// </summary>
    /// <param name="argName">The name of the secret on the command line (e.g., "api-key").</param>
    /// <param name="description">A description of the secret for help text.</param>
    /// <param name="sources">The sources from which the secret can be resolved.</param>
    /// <param name="chainedParams">An array of other parameters that this secret depends on.</param>
    public SecretDefinitionAttribute(
        string argName,
        string description,
        ParamSource sources = ParamSource.All,
        string[]? chainedParams = null) : base(argName, description, sources, chainedParams)
    {
        IsSecret = true;
    }
}
