namespace Invex.Atom.Module.AzureStorage;

/// <summary>
///     Provides an interface for configuring Azure Blob Storage as an artifact store.
/// </summary>
/// <remarks>
///     <para>
///         This interface extends <see cref="IStoreArtifact" /> and <see cref="IRetrieveArtifact" />,
///         enabling the build to both upload and download artifacts from Azure Blob Storage.
///     </para>
///     <para>
///         Connect either with <see cref="AzureArtifactStorageConnectionString" />, or with
///         <see cref="AzureArtifactStorageAccountUrl" /> and a Microsoft Entra token credential, selected in this order:
///         a client secret (tenant ID, client ID and client secret); the GitHub Actions OIDC token (tenant ID and
///         client ID, with the <c>id-token: write</c> permission); an Azure Pipelines workload identity federation
///         service connection (tenant ID, client ID and service connection ID, with <c>SYSTEM_ACCESSTOKEN</c> mapped);
///         otherwise <c>DefaultAzureCredential</c>, followed by an interactive browser login when not running headless.
///     </para>
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IAzureArtifactStorage : IStoreArtifact, IRetrieveArtifact
{
    /// <summary>
    ///     Gets the connection string for the Azure Storage account.
    /// </summary>
    /// <remarks>
    ///     Optional when <see cref="AzureArtifactStorageAccountUrl" /> is set; takes precedence over it when both are
    ///     set. Store it securely, for example as a CI secret, in .NET user secrets or in Azure Key Vault.
    /// </remarks>
    [SecretDefinition("azurestorage-artifact-connectionstring", "Connection string for Azure storage container")]
    string? AzureArtifactStorageConnectionString => GetParam(() => AzureArtifactStorageConnectionString);

    /// <summary>
    ///     Gets the Blob service URL of the Azure Storage account, for example
    ///     <c>https://myaccount.blob.core.windows.net</c>.
    /// </summary>
    /// <remarks>
    ///     Used with a Microsoft Entra token credential when no connection string is set. The identity needs a data
    ///     role on the container, such as <c>Storage Blob Data Contributor</c>. See
    ///     <see cref="IAzureArtifactStorage" /> for how the credential is selected.
    /// </remarks>
    [ParamDefinition("azurestorage-artifact-account-url", "Blob service URL of the Azure storage account")]
    string? AzureArtifactStorageAccountUrl => GetParam(() => AzureArtifactStorageAccountUrl);

    /// <summary>
    ///     Gets the name of the Azure Blob Storage container to use for artifacts.
    /// </summary>
    /// <remarks>
    ///     This parameter specifies the target container within the Azure Storage account where
    ///     artifacts will be stored and retrieved.
    /// </remarks>
    [ParamDefinition("azurestorage-artifact-container", "Azure storage container")]
    string AzureArtifactStorageContainer => GetParam(() => AzureArtifactStorageContainer)!;

    /// <summary>
    ///     Gets the Microsoft Entra tenant ID used to authenticate with the storage account.
    /// </summary>
    /// <remarks>Required for client secret and federated (OIDC) authentication with an account URL.</remarks>
    [ParamDefinition("azurestorage-artifact-tenant-id", "Microsoft Entra tenant ID for Azure storage authentication")]
    string? AzureArtifactStorageTenantId => GetParam(() => AzureArtifactStorageTenantId);

    /// <summary>
    ///     Gets the client (application) ID of the app registration or user-assigned managed identity that has access
    ///     to the storage account.
    /// </summary>
    /// <remarks>
    ///     Required for client secret and federated (OIDC) authentication with an account URL. With
    ///     <c>DefaultAzureCredential</c> it is used as the workload identity and managed identity client ID.
    /// </remarks>
    [ParamDefinition("azurestorage-artifact-client-id",
        "Client ID of the app registration or managed identity that has access to the Azure storage account")]
    string? AzureArtifactStorageClientId => GetParam(() => AzureArtifactStorageClientId);

    /// <summary>
    ///     Gets the client secret of the app registration that has access to the storage account.
    /// </summary>
    /// <remarks>Optional; prefer federated credentials (OIDC) in CI where possible.</remarks>
    [SecretDefinition("azurestorage-artifact-client-secret",
        "Client secret of the app registration that has access to the Azure storage account")]
    string? AzureArtifactStorageClientSecret => GetParam(() => AzureArtifactStorageClientSecret);

    /// <summary>
    ///     Gets the ID of the Azure DevOps service connection (workload identity federation) used to authenticate in
    ///     Azure Pipelines.
    /// </summary>
    /// <remarks>
    ///     Only used in Azure Pipelines, where <c>SYSTEM_ACCESSTOKEN</c> must also be mapped into the target step's
    ///     environment.
    /// </remarks>
    [ParamDefinition("azurestorage-artifact-service-connection-id",
        "Azure DevOps service connection ID for workload identity federation with Azure storage")]
    string? AzureArtifactStorageServiceConnectionId => GetParam(() => AzureArtifactStorageServiceConnectionId);

    /// <summary>
    ///     Configures the host builder to add Azure Blob Storage artifact services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers the module's internal Azure Blob Storage implementation as the singleton
    ///     <see cref="IArtifactProvider" />, making Azure Blob Storage available for artifact management throughout
    ///     the build.
    /// </remarks>
    protected static partial void ConfigureHostBuilderFromIAzureArtifactStorage(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IArtifactProvider, AzureBlobArtifactProvider>();
}
