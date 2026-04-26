namespace DecSm.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides an implementation of <see cref="ISecretsProvider" /> for retrieving secrets from Azure Key Vault.
/// </summary>
/// <remarks>
///     This provider connects to Azure Key Vault using the parameters defined in <see cref="IAzureKeyVault" />
///     and allows for dynamic retrieval of secrets during the build process.
/// </remarks>
[PublicAPI]
public sealed class AzureKeySecretsProvider(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IAzureKeyVault keyVault,
    ILogger<AzureKeySecretsProvider> logger
) : ISecretsProvider
{
    private SecretClient? _secretClient;

    /// <summary>
    ///     Retrieves a secret value from Azure Key Vault based on the provided key.
    /// </summary>
    /// <param name="key">The name of the secret to retrieve.</param>
    /// <returns>The secret value if found, otherwise <c>null</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Azure Vault address is not set.</exception>
    public string? GetSecret(string key)
    {
        // We don't want to lookup any secrets that we need to use for authentication, or we'll end up in a loop
        if (key is nameof(IAzureKeyVault.AzureVaultAddress)
            or nameof(IAzureKeyVault.AzureVaultTenantId)
            or nameof(IAzureKeyVault.AzureVaultAppId))
            return null;

        logger.LogDebug("Attempting to get secret {Key} from Azure Key Vault", key);

        if (keyVault.AzureVaultAddress is null or "")
        {
            var addressArg = buildDefinition.ParamDefinitions[nameof(IAzureKeyVault.AzureVaultAddress)].ArgName;

            throw new InvalidOperationException(
                $"Azure Vault address '{addressArg}' must be set to use Azure Key Vault.");
        }

        try
        {
            var client = _secretClient ??= new(new(keyVault.AzureVaultAddress), GetCredential(keyVault));

            return GetSecret(client, key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to get '{Key}' from Azure Key Vault. Secrets will not be available from Azure Key Vault.",
                key);

            return null;
        }
    }

    /// <summary>
    ///     Obtains the appropriate <see cref="TokenCredential" /> for authenticating with Azure Key Vault.
    /// </summary>
    /// <param name="definition">The Azure Key Vault definition containing authentication parameters.</param>
    /// <returns>A <see cref="TokenCredential" /> instance.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if running in headless mode and required Azure Vault parameters are not set.
    /// </exception>
    private TokenCredential GetCredential(IAzureKeyVault definition)
    {
        if (definition is
            {
                AzureVaultTenantId.Length: > 0, AzureVaultAppId.Length: > 0, AzureVaultAppSecret.Length: > 0,
            })
            return new ClientSecretCredential(definition.AzureVaultTenantId,
                definition.AzureVaultAppId,
                definition.AzureVaultAppSecret);

        if (args.HasHeadless)
            throw new InvalidOperationException(
                "When running in headless mode, Azure Vault parameters must be set: azure-vault-address, azure-vault-tenant-id, azure-vault-app-id, azure-vault-app-secret.");

        logger.LogInformation(
            "Attempting interactive browser login for Azure Key Vault. Please ensure you have a browser available.");

        var port = buildDefinition.AccessParam(nameof(IAzureKeyVault.AzureVaultAuthPort)) switch
        {
            string portString => int.TryParse(portString, out var parsedPort)
                ? parsedPort
                : throw new InvalidOperationException(
                    $"Invalid port value for AzureVaultAuthPort: '{portString}'. Please provide a valid integer."),
            int portInt => portInt,
            _ => 0,
        };

        return new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            AdditionallyAllowedTenants =
            {
                definition.AzureVaultTenantId ?? "*",
            },
            RedirectUri = port > 0
                ? new($"http://localhost:{port}")
                : null,
        });
    }

    /// <summary>
    ///     Retrieves a secret from the provided <see cref="SecretClient" />.
    /// </summary>
    /// <param name="client">The <see cref="SecretClient" /> to use for secret retrieval.</param>
    /// <param name="key">The name of the secret to retrieve.</param>
    /// <returns>The secret value if found, otherwise <c>null</c>.</returns>
    private string? GetSecret(SecretClient client, string key)
    {
        logger.LogInformation(
            "Getting Azure Key Vault secret '{Key}'. If a login prompt appears, please authenticate with an Azure account that has access to the Vault.",
            key);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

        try
        {
            var response = client.GetSecret(key, cancellationToken: cts.Token);

            if (response.HasValue)
            {
                logger.LogTrace("Successfully retrieved secret '{Key}' from Azure Key Vault.", key);

                return response.Value.Value;
            }

            logger.LogWarning("Secret '{Key}' not found in Azure Key Vault.", key);

            return null;
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Azure Key Vault secret retrieval for '{Key}' timed out.", key);
        }
        catch (AuthenticationFailedException authenticationFailed)
        {
            logger.LogWarning(
                "Authentication failed when trying to access Azure Key Vault for secret '{Key}': {Message}. Secrets will not be available from Azure Key Vault.",
                key,
                authenticationFailed.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An unexpected error occurred while trying to retrieve secret '{Key}' from Azure Key Vault. Secrets will not be available from Azure Key Vault.",
                key);
        }

        return null;
    }
}
