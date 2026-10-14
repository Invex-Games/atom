namespace Invex.Atom.Build.Secrets;

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
///         This interface is not included in <see cref="BuildDefinition" /> by default; implement it explicitly on
///         your build definition to enable the user secrets store. The build project also needs a
///         <c>UserSecretsId</c>.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // 1. Implement the interface in your build definition
/// [BuildDefinition]
/// partial class Build : BuildDefinition, IDotnetUserSecrets, IMyTarget;
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
    protected static partial void ConfigureBuilderFromIDotnetUserSecrets(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<DotnetUserSecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<DotnetUserSecretsProvider>());
}
