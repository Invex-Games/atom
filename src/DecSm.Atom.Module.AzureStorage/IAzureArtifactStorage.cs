namespace DecSm.Atom.Module.AzureStorage;

/// <summary>
///     Provides an interface for configuring Azure Blob Storage as an artifact store.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IStoreArtifact" /> and <see cref="IRetrieveArtifact" />,
///     enabling the build to both upload and download artifacts from Azure Blob Storage.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IAzureArtifactStorage : IStoreArtifact, IRetrieveArtifact
{
    /// <summary>
    ///     Gets the connection string for the Azure Storage account.
    /// </summary>
    /// <remarks>
    ///     This secret definition is crucial for authenticating and connecting to the Azure Storage account.
    ///     It should be stored securely, for example, in `appsettings.json` under the `Secrets` section
    ///     or as an environment variable marked as a secret in your CI/CD system.
    /// </remarks>
    [SecretDefinition("azurestorage-artifact-connectionstring", "Connection string for Azure storage container")]
    string AzureArtifactStorageConnectionString => GetParam(() => AzureArtifactStorageConnectionString)!;

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
    ///     Configures the host builder to add Azure Blob Storage artifact services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="AzureBlobArtifactProvider" /> as the singleton
    ///     implementation for <see cref="IArtifactProvider" />, making Azure Blob Storage
    ///     available for artifact management throughout the build.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IArtifactProvider, AzureBlobArtifactProvider>();
}
