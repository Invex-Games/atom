namespace Invex.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides an implementation of <see cref="IBuildOptionProvider" /> that injects the Azure Key Vault connection
///     parameters into generated workflows.
/// </summary>
/// <remarks>
///     Options are only contributed when <see cref="UseAzureKeyVault" /> is enabled. Each parameter is injected
///     according to <see cref="IAzureKeyVault.AzureKeyVaultValueInjections" />, and is only resolved by secret
///     providers, never looked up in the vault itself.
/// </remarks>
internal sealed class AzureKeyVaultOptionsProvider(IAzureKeyVault keyVault) : IBuildOptionProvider
{
    /// <summary>
    ///     Gets additional build options based on the Azure Key Vault injection configuration.
    /// </summary>
    /// <param name="baseOptions">The build options defined so far.</param>
    /// <returns>The injection options for the Azure Key Vault parameters, or an empty list when disabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if an injection type is not recognised.</exception>
    public IReadOnlyList<IBuildOption> GetBuildOptions(IReadOnlyList<IBuildOption> baseOptions)
    {
        if (!UseAzureKeyVault.IsEnabled(baseOptions))
            return [];

        var injections = keyVault.AzureKeyVaultValueInjections;

        return new[]
            {
                CreateInjection(nameof(IAzureKeyVault.AzureKeyVaultAddress), injections.Address),
                CreateInjection(nameof(IAzureKeyVault.AzureKeyVaultTenantId), injections.TenantId),
                CreateInjection(nameof(IAzureKeyVault.AzureKeyVaultClientId), injections.ClientId),
                CreateInjection(nameof(IAzureKeyVault.AzureKeyVaultClientSecret), injections.ClientSecret),
                CreateInjection(nameof(IAzureKeyVault.AzureKeyVaultServiceConnectionId),
                    injections.ServiceConnectionId),
            }
            .OfType<IBuildOption>()
            .ToList();
    }

    private static IBuildOption? CreateInjection(string paramName, AzureKeyVaultValueInjectionType injectionType) =>
        injectionType switch
        {
            AzureKeyVaultValueInjectionType.EnvironmentVariable =>
                BuildOptions.Inject.SecretFromWorkflowEnvironment(paramName),
            AzureKeyVaultValueInjectionType.Secret => BuildOptions.Inject.SecretForSecretProvider(paramName),
            AzureKeyVaultValueInjectionType.None => null,
            _ => throw new ArgumentOutOfRangeException(nameof(injectionType), injectionType, null),
        };
}
