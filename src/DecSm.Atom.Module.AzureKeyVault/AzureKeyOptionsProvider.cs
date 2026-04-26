namespace DecSm.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides an implementation of <see cref="IBuildOptionProvider" />.
/// </summary>
/// <remarks>
///     This provider integrates with build options to define how Azure Key Vault parameters are injected.
/// </remarks>
[PublicAPI]
public sealed class AzureKeyOptionsProvider(IAzureKeyVault keyVault) : IBuildOptionProvider
{
    /// <summary>
    ///     Gets additional build options based on the Azure Key Vault injection configuration.
    /// </summary>
    /// <remarks>
    ///     This method dynamically generates build options based on the
    ///     <see cref="IAzureKeyVault.AzureKeyVaultValueInjections" />
    ///     configuration, allowing for flexible secret injection strategies.
    /// </remarks>
    public IReadOnlyList<IBuildOption> GetBuildOptions(IReadOnlyList<IBuildOption> baseOptions)
    {
        if (!UseAzureKeyVault.IsEnabled(baseOptions))
            return [];

        var injections = keyVault.AzureKeyVaultValueInjections;

        var valueInjections = new List<IBuildOption>();

        switch (injections.Address)
        {
            case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                valueInjections.Add(
                    BuildOptions.Inject.SecretFromWorkflowEnvironment(nameof(IAzureKeyVault.AzureVaultAddress)));

                break;
            case AzureKeyVaultValueInjectionType.Secret:
                valueInjections.Add(
                    BuildOptions.Inject.SecretForSecretProvider(nameof(IAzureKeyVault.AzureVaultAddress)));

                break;
            case AzureKeyVaultValueInjectionType.None:

                break;
            default:

                throw new ArgumentOutOfRangeException(nameof(injections.Address), injections.Address, null);
        }

        switch (injections.TenantId)
        {
            case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                valueInjections.Add(
                    BuildOptions.Inject.SecretFromWorkflowEnvironment(nameof(IAzureKeyVault.AzureVaultTenantId)));

                break;
            case AzureKeyVaultValueInjectionType.Secret:
                valueInjections.Add(
                    BuildOptions.Inject.SecretForSecretProvider(nameof(IAzureKeyVault.AzureVaultTenantId)));

                break;
            case AzureKeyVaultValueInjectionType.None:

                break;
            default:

                throw new ArgumentOutOfRangeException(nameof(injections.TenantId), injections.TenantId, null);
        }

        switch (injections.AppId)
        {
            case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                valueInjections.Add(
                    BuildOptions.Inject.SecretFromWorkflowEnvironment(nameof(IAzureKeyVault.AzureVaultAppId)));

                break;
            case AzureKeyVaultValueInjectionType.Secret:
                valueInjections.Add(
                    BuildOptions.Inject.SecretForSecretProvider(nameof(IAzureKeyVault.AzureVaultAppId)));

                break;
            case AzureKeyVaultValueInjectionType.None:

                break;
            default:

                throw new ArgumentOutOfRangeException(nameof(injections.AppId), injections.AppId, null);
        }

        switch (injections.AppSecret)
        {
            case AzureKeyVaultValueInjectionType.EnvironmentVariable:
                valueInjections.Add(
                    BuildOptions.Inject.SecretFromWorkflowEnvironment(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                break;
            case AzureKeyVaultValueInjectionType.Secret:
                valueInjections.Add(
                    BuildOptions.Inject.SecretForSecretProvider(nameof(IAzureKeyVault.AzureVaultAppSecret)));

                break;
            case AzureKeyVaultValueInjectionType.None:

                break;
            default:

                throw new ArgumentOutOfRangeException(nameof(injections.AppSecret), injections.AppSecret, null);
        }

        return valueInjections;
    }
}
