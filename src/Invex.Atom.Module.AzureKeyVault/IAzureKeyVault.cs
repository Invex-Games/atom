namespace Invex.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides integration with Azure Key Vault for managing secrets and configuration.
/// </summary>
/// <remarks>
///     <para>
///         This interface defines the parameters required to connect to an Azure Key Vault
///         and configures the necessary services for retrieving secrets.
///     </para>
///     <para>
///         The vault is accessed with a Microsoft Entra token credential, selected in this order:
///     </para>
///     <list type="number">
///         <item>
///             <description>
///                 <see cref="AzureKeyVaultTenantId" />, <see cref="AzureKeyVaultClientId" /> and
///                 <see cref="AzureKeyVaultClientSecret" />: a client secret credential.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="AzureKeyVaultTenantId" /> and <see cref="AzureKeyVaultClientId" /> in a GitHub Actions job
///                 with the <c>id-token: write</c> permission: the job's OIDC token, exchanged through a federated
///                 credential on the app registration.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="AzureKeyVaultTenantId" />, <see cref="AzureKeyVaultClientId" /> and
///                 <see cref="AzureKeyVaultServiceConnectionId" /> in Azure Pipelines with <c>SYSTEM_ACCESSTOKEN</c>
///                 mapped into the step: a workload identity federation service connection.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Otherwise <c>DefaultAzureCredential</c> (environment variables, workload identity, managed identity,
///                 Azure CLI, Azure Developer CLI, ...), followed by an interactive browser login when not running
///                 headless.
///             </description>
///         </item>
///     </list>
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IAzureKeyVault : IBuildAccessor
{
    /// <summary>
    ///     Gets the address (URI) of the Azure Key Vault, for example <c>https://my-vault.vault.azure.net/</c>.
    /// </summary>
    /// <remarks>
    ///     This parameter is crucial for identifying which Azure Key Vault instance to connect to.
    ///     It can be provided via command-line arguments, environment variables, or `appsettings.json`.
    /// </remarks>
    [ParamDefinition("azure-key-vault-address", "Address (URI) of the Azure Key Vault")]
    string? AzureKeyVaultAddress => GetParam(() => AzureKeyVaultAddress);

    /// <summary>
    ///     Gets the Microsoft Entra tenant ID used to authenticate with the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     Required for client secret and federated (OIDC) authentication. When set, it also restricts
    ///     <c>DefaultAzureCredential</c> and interactive login to this tenant.
    /// </remarks>
    [ParamDefinition("azure-key-vault-tenant-id", "Microsoft Entra tenant ID for Azure Key Vault authentication")]
    string? AzureKeyVaultTenantId => GetParam(() => AzureKeyVaultTenantId);

    /// <summary>
    ///     Gets the client (application) ID of the app registration or user-assigned managed identity that has access
    ///     to the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     Required for client secret and federated (OIDC) authentication. With <c>DefaultAzureCredential</c> it is
    ///     used as the workload identity and managed identity client ID. The identity needs permission to get secrets
    ///     from the vault (for example the <c>Key Vault Secrets User</c> role).
    /// </remarks>
    [ParamDefinition("azure-key-vault-client-id",
        "Client ID of the app registration or managed identity that has access to the Azure Key Vault")]
    string? AzureKeyVaultClientId => GetParam(() => AzureKeyVaultClientId);

    /// <summary>
    ///     Gets the client secret of the app registration that has access to the Azure Key Vault.
    /// </summary>
    /// <remarks>
    ///     Optional. When set together with <see cref="AzureKeyVaultTenantId" /> and
    ///     <see cref="AzureKeyVaultClientId" />, client secret authentication is used. Prefer federated credentials
    ///     (OIDC) in CI where possible. This value is never looked up in the vault itself.
    /// </remarks>
    [SecretDefinition("azure-key-vault-client-secret",
        "Client secret of the app registration that has access to the Azure Key Vault")]
    string? AzureKeyVaultClientSecret => GetParam(() => AzureKeyVaultClientSecret);

    /// <summary>
    ///     Gets the ID of the Azure DevOps service connection (workload identity federation) used to authenticate in
    ///     Azure Pipelines.
    /// </summary>
    /// <remarks>
    ///     Only used in Azure Pipelines, where <c>SYSTEM_ACCESSTOKEN</c> must also be mapped into the target step's
    ///     environment.
    /// </remarks>
    [ParamDefinition("azure-key-vault-service-connection-id",
        "Azure DevOps service connection ID for workload identity federation with Azure Key Vault")]
    string? AzureKeyVaultServiceConnectionId => GetParam(() => AzureKeyVaultServiceConnectionId);

    /// <summary>
    ///     Gets the port number for the local redirect of the interactive browser login.
    /// </summary>
    /// <remarks>
    ///     Only used when no other credential is available and the build is not running headless.
    ///     The default port is 3421. Setting this to 0 will select a random available port.
    /// </remarks>
    [ParamDefinition("azure-key-vault-auth-port",
        "Port for local auth redirect (default 3421). Set to 0 to use a random port.")]
    int AzureKeyVaultAuthPort => GetParam(() => AzureKeyVaultAuthPort, 3421);

    /// <summary>
    ///     Gets the timeout, in seconds, for retrieving a single secret from Azure Key Vault, including any
    ///     authentication that the request triggers.
    /// </summary>
    /// <remarks>
    ///     The default is 120 seconds, which leaves time to complete an interactive login. Set to 0 to disable the
    ///     timeout and rely only on the build's cancellation token.
    /// </remarks>
    [ParamDefinition("azure-key-vault-timeout",
        "Timeout in seconds for retrieving a secret from Azure Key Vault (default 120). Set to 0 to disable.")]
    int AzureKeyVaultTimeout => GetParam(() => AzureKeyVaultTimeout, 120);

    /// <summary>
    ///     Configures how Azure Key Vault parameter values are injected into generated workflows.
    /// </summary>
    /// <remarks>
    ///     This property allows customization of the source for each Azure Key Vault related parameter.
    ///     By default, the address, tenant ID and client ID are read from workflow variables, the client secret from
    ///     the platform's secret store, and the service connection ID is not injected.
    /// </remarks>
    AzureKeyVaultValueInjections AzureKeyVaultValueInjections => new();

    /// <summary>
    ///     Configures the host builder to add Azure Key Vault services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers the Azure Key Vault <see cref="ISecretsProvider" /> and an
    ///     <see cref="IBuildOptionProvider" /> that injects the vault connection parameters into generated workflows.
    ///     Both implementations are internal to this module.
    /// </remarks>
    protected static partial void ConfigureHostBuilderFromIAzureKeyVault(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<ISecretsProvider, AzureKeyVaultSecretsProvider>()
            .AddSingleton<IBuildOptionProvider, AzureKeyVaultOptionsProvider>();
}

/// <summary>
///     Defines the injection types for Azure Key Vault related parameter values in generated workflows.
/// </summary>
/// <param name="Address">The injection type for <see cref="IAzureKeyVault.AzureKeyVaultAddress" />.</param>
/// <param name="TenantId">The injection type for <see cref="IAzureKeyVault.AzureKeyVaultTenantId" />.</param>
/// <param name="ClientId">The injection type for <see cref="IAzureKeyVault.AzureKeyVaultClientId" />.</param>
/// <param name="ClientSecret">The injection type for <see cref="IAzureKeyVault.AzureKeyVaultClientSecret" />.</param>
/// <param name="ServiceConnectionId">
///     The injection type for <see cref="IAzureKeyVault.AzureKeyVaultServiceConnectionId" />.
/// </param>
[PublicAPI]
public sealed record AzureKeyVaultValueInjections(
    AzureKeyVaultValueInjectionType Address = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType TenantId = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType ClientId = AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AzureKeyVaultValueInjectionType ClientSecret = AzureKeyVaultValueInjectionType.Secret,
    AzureKeyVaultValueInjectionType ServiceConnectionId = AzureKeyVaultValueInjectionType.None
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
    ///     The parameter value is retrieved from a workflow variable (GitHub <c>vars</c>, Azure DevOps variables).
    /// </summary>
    EnvironmentVariable,

    /// <summary>
    ///     The parameter value is retrieved from the platform's secret store.
    /// </summary>
    Secret,
}
