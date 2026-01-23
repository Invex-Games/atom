namespace DecSm.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides integration with Azure Key Vault for managing secrets and configuration.
/// </summary>
/// <remarks>
///     This interface defines the parameters required to connect to an Azure Key Vault
///     and configures the necessary services for retrieving secrets.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IAzureKeyVault : IBuildAccessor
{
    /// <summary>
    ///     Gets the address (URI) of the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     This parameter is crucial for identifying which Azure Key Vault instance to connect to.
    ///     It can be provided via command-line arguments, environment variables, or `appsettings.json`.
    /// </remarks>
    [ParamDefinition("azure-vault-address", "Address for the Azure Vault")]
    string? AzureVaultAddress => GetParam(() => AzureVaultAddress);

    /// <summary>
    ///     Gets the Tenant ID associated with the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     The Tenant ID is part of the authentication process to ensure access to the correct Azure Active Directory tenant.
    /// </remarks>
    [ParamDefinition("azure-vault-tenant-id", "Tenant ID for the Azure Vault")]
    string? AzureVaultTenantId => GetParam(() => AzureVaultTenantId);

    /// <summary>
    ///     Gets the Application (Client) ID for the App Registration that has access to the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     This ID is used for service principal authentication to Azure Key Vault.
    ///     Ensure the registered application has appropriate permissions (e.g., Get, List) on secrets in the vault.
    /// </remarks>
    [ParamDefinition("azure-vault-app-id", "Azure ID for App Registration that has access to the Azure Vault")]
    string? AzureVaultAppId => GetParam(() => AzureVaultAppId);

    /// <summary>
    ///     Gets the client secret for the App Registration that has access to the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     This secret is used in conjunction with the <see cref="AzureVaultAppId" /> for authenticating
    ///     the application to Azure Key Vault. It should be treated as sensitive information.
    /// </remarks>
    [ParamDefinition("azure-vault-app-secret", "Azure Secret for App Registration that has access to the Azure Vault")]
    string? AzureVaultAppSecret => GetParam(() => AzureVaultAppSecret);

    /// <summary>
    ///     Gets the port number for the local authentication redirect during the Azure Key Vault authentication process.
    /// </summary>
    /// <remarks>
    ///     This port is used when performing device code or interactive authentication flows.
    ///     The default port is 3421. Setting this to 0 will select a random available port.
    /// </remarks>
    [ParamDefinition("azure-vault-auth-port",
        "Port for local auth redirect (default 3421). Set to 0 to use a random port.")]
    int AzureVaultAuthPort => GetParam(() => AzureVaultAuthPort, 3421);

    /// <summary>
    ///     Configures how Azure Key Vault parameter values are injected into the build process.
    /// </summary>
    /// <remarks>
    ///     This property allows customization of the source for each Azure Key Vault related parameter.
    ///     By default, most parameters are expected from environment variables, while the app secret is
    ///     expected from a secure secret store.
    /// </remarks>
    AzureKeyVaultValueInjections AzureKeyVaultValueInjections => new();

    /// <summary>
    ///     Configures the host builder to add Azure Key Vault services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="AzureKeySecretsProvider" /> as a singleton service,
    ///     making it available for secret retrieval and workflow option provisioning throughout the build.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<AzureKeySecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<AzureKeySecretsProvider>())
            .AddSingleton<IWorkflowOptionProvider>(x => x.GetRequiredService<AzureKeySecretsProvider>());
}

/// <summary>
///     Defines the injection types for Azure Key Vault related parameter values.
/// </summary>
/// <param name="Address">The injection type for the Azure Vault Address.</param>
/// <param name="TenantId">The injection type for the Azure Vault Tenant ID.</param>
/// <param name="AppId">The injection type for the Azure Vault Application ID.</param>
/// <param name="AppSecret">The injection type for the Azure Vault Application Secret.</param>
[PublicAPI]
public sealed record AzureKeyVaultValueInjections(
    AzureKeyVaultValueInjectionType Address = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType TenantId = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType AppId = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType AppSecret = AzureKeyVaultValueInjectionType.Secret
);

/// <summary>
///     Specifies the source from which an Azure Key Vault parameter value should be injected.
/// </summary>
[PublicAPI]
public enum AzureKeyVaultValueInjectionType
{
    /// <summary>
    ///     The parameter value is not injected.
    /// </summary>
    None,

    /// <summary>
    ///     The parameter value is retrieved from an environment variable.
    /// </summary>
    EnvironmentVariable,

    /// <summary>
    ///     The parameter value is retrieved from a secure secret store.
    /// </summary>
    Secret,
}
