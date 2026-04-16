namespace DecSm.Atom.Secrets;

/// <summary>
///     An interface that, when implemented on a build definition, enables sourcing secrets from the .NET user secrets
///     store.
/// </summary>
/// <remarks>
///     This interface uses source generation to automatically register the <see cref="DotnetUserSecretsProvider" />
///     with the dependency injection container. This allows the <see cref="IParamService" /> to resolve secrets
///     marked with <see cref="SecretDefinitionAttribute" /> from the local user secrets store, which is ideal
///     for development-time secrets that should not be committed to source control.
///     <para>
///         This interface is automatically included in the default <see cref="BuildDefinition" />.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // 1. Implement the interface in your build definition
/// [BuildDefinition]
/// partial class Build : IDotnetUserSecrets, IMyTarget;
/// // 2. Define a secret parameter
/// [SecretDefinition("my-secret", "A secret from user secrets.")]
/// string MySecret => GetParam(() => MySecret);
/// // The value for "my-secret" will now be resolved from the user secrets store.
///     </code>
/// </example>
/// <seealso cref="DotnetUserSecretsProvider" />
/// <seealso cref="ISecretsProvider" />
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IDotnetUserSecrets
{
    /// <summary>
    ///     Configures the host builder to register the <see cref="DotnetUserSecretsProvider" />.
    /// </summary>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<DotnetUserSecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<DotnetUserSecretsProvider>());
}
