namespace Invex.Atom.Build.Secrets;

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
///         marked with <see cref="SecretDefinitionAttribute" />. Providers are queried in descending
///         <see cref="Priority" /> order (ties preserve registration order), creating a fallback chain
///         (e.g., from a cloud provider to local user secrets).
///     </para>
///     <para>
///         Before a build executes its targets, Atom asynchronously prefetches the secret parameters used by the targets
///         that will run, passing the build's cancellation token (which is signalled when the host stops). Parameter
///         property getters (<c>GetParam(() => MySecret)</c>) are synchronous, so a secret that was not prefetched (for
///         example, one read by name from a provider or a target that is not part of the run) is resolved by blocking
///         on <see cref="GetSecretAsync" /> with <see cref="CancellationToken.None" />. Atom runs as a console
///         application without a <see cref="SynchronizationContext" />, so this does not deadlock, but implementations
///         should still enforce their own timeouts for network calls.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // 1. Implement the interface
/// public class MySecretsProvider : ISecretsProvider
/// {
///     public async ValueTask&lt;string?&gt; GetSecretAsync(string key, CancellationToken cancellationToken) =>
///         await MySecureStore.GetAsync(key, cancellationToken);
/// }
/// // 2. Register the provider in your build definition
/// public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
/// {
///     builder.Services.AddSingleton&lt;ISecretsProvider, MySecretsProvider&gt;();
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
    ///     Asynchronously retrieves a secret value by its key.
    /// </summary>
    /// <param name="key">The unique identifier for the secret to retrieve (the parameter's argument name).</param>
    /// <param name="cancellationToken">
    ///     A token that is cancelled when the build is cancelled or the host is stopping. It is
    ///     <see cref="CancellationToken.None" /> when the secret is resolved synchronously outside the prefetch phase.
    /// </param>
    /// <returns>
    ///     The secret value associated with the key, or <c>null</c> if the secret is not found in this provider.
    /// </returns>
    /// <remarks>
    ///     Implementations should return <c>null</c> rather than throwing an exception if a key is not found,
    ///     allowing the framework to query other registered providers. Results are not cached by the provider
    ///     contract; Atom caches resolved values in <see cref="IParamService" />.
    /// </remarks>
    ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the resolution priority of the secrets provider.
    /// </summary>
    /// <remarks>
    ///     Providers with a higher priority value are queried first when resolving secrets among
    ///     multiple providers; providers with equal priority are queried in registration order.
    ///     The default value is 0.
    /// </remarks>
    int Priority => 0;
}
