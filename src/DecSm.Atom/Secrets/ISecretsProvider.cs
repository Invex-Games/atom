namespace DecSm.Atom.Secrets;

/// <summary>
///     Defines a contract for retrieving sensitive configuration values (secrets) from a secure storage provider.
/// </summary>
/// <remarks>
///     <para>
///         This interface is the core of the Atom framework's secret management system, enabling secure retrieval
///         of sensitive data like API keys and connection strings. Implementations can integrate with various
///         backends such as Azure Key Vault or .NET User Secrets.
///     </para>
///     <para>
///         The framework automatically discovers and queries registered providers when resolving parameters
///         marked with <see cref="SecretDefinitionAttribute" />. Providers are queried in registration order,
///         creating a fallback chain (e.g., from a cloud provider to local user secrets).
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // 1. Implement the interface
/// public class MySecretsProvider : ISecretsProvider
/// {
///     public string? GetSecret(string key) => MySecureStore.Get(key);
/// }
/// // 2. Register the provider in your build definition
/// protected override void ConfigureServices(IServiceCollection services)
/// {
///     services.AddSingleton&lt;ISecretsProvider, MySecretsProvider&gt;();
/// }
/// // 3. Use a secret parameter in a target
/// [SecretDefinition("my-api-key", "An API key.")]
/// string MyApiKey => GetParam(() => MyApiKey);
///     </code>
/// </example>
/// <seealso cref="SecretDefinitionAttribute" />
/// <seealso cref="IParamService" />
[PublicAPI]
public interface ISecretsProvider
{
    /// <summary>
    ///     Retrieves a secret value by its key.
    /// </summary>
    /// <param name="key">The unique identifier for the secret to retrieve.</param>
    /// <returns>
    ///     The secret value associated with the key, or <c>null</c> if the secret is not found in this provider.
    /// </returns>
    /// <remarks>
    ///     Implementations should return <c>null</c> rather than throwing an exception if a key is not found,
    ///     allowing the framework to query other registered providers.
    /// </remarks>
    string? GetSecret(string key);
}
