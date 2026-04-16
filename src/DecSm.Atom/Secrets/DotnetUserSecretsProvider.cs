namespace DecSm.Atom.Secrets;

/// <summary>
///     An <see cref="ISecretsProvider" /> implementation that retrieves secrets from the .NET user secrets store.
/// </summary>
/// <remarks>
///     This provider integrates with the .NET user secrets system, allowing for secure access to sensitive
///     configuration values during local development without storing them in source control.
/// </remarks>
internal sealed class DotnetUserSecretsProvider : ISecretsProvider
{
    /// <summary>
    ///     Gets or sets the assembly used to identify the user secrets configuration.
    /// </summary>
    /// <remarks>
    ///     This assembly should have the <c>UserSecretsIdAttribute</c> applied. If not set, the entry assembly is used.
    /// </remarks>
    public Assembly? SecretsAssembly { get; set; }

    /// <summary>
    ///     Retrieves a secret value by its key from the .NET user secrets store.
    /// </summary>
    /// <param name="key">The key of the secret to retrieve.</param>
    /// <returns>The secret value, or <c>null</c> if the key is not found.</returns>
    /// <remarks>
    ///     This method builds a configuration using the user secrets provider and searches for the specified key,
    ///     loading secrets based on the <see cref="SecretsAssembly" /> or the entry assembly.
    /// </remarks>
    public string? GetSecret(string key)
    {
        var userSecrets = new ConfigurationBuilder()
            .AddUserSecrets(SecretsAssembly ?? Assembly.GetEntryAssembly()!)
            .Build()
            .AsEnumerable()
            .ToDictionary(x => x.Key, x => x.Value);

        return userSecrets.GetValueOrDefault(key);
    }
}
