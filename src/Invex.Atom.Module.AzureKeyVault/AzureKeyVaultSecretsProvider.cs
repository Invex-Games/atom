namespace Invex.Atom.Module.AzureKeyVault;

/// <summary>
///     Provides an implementation of <see cref="ISecretsProvider" /> for retrieving secrets from Azure Key Vault.
/// </summary>
/// <remarks>
///     <para>
///         This provider connects to Azure Key Vault using the parameters defined in <see cref="IAzureKeyVault" />.
///         The credential is selected once, on first use (see <see cref="IAzureKeyVault" /> for the order).
///     </para>
///     <para>
///         If the vault cannot be reached (authentication fails or a request times out), the provider logs a warning and returns
///         <c>null</c> for the remaining secrets of the run instead of retrying, so other providers can be used.
///     </para>
/// </remarks>
[PublicAPI]
public sealed class AzureKeyVaultSecretsProvider(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IAzureKeyVault keyVault,
    ILogger<AzureKeyVaultSecretsProvider> logger
) : ISecretsProvider
{
    private static readonly string[] ConnectionParamNames =
    [
        nameof(IAzureKeyVault.AzureKeyVaultAddress),
        nameof(IAzureKeyVault.AzureKeyVaultTenantId),
        nameof(IAzureKeyVault.AzureKeyVaultClientId),
        nameof(IAzureKeyVault.AzureKeyVaultClientSecret),
        nameof(IAzureKeyVault.AzureKeyVaultServiceConnectionId),
        nameof(IAzureKeyVault.AzureKeyVaultAuthPort),
        nameof(IAzureKeyVault.AzureKeyVaultTimeout),
    ];

    private SecretClient? _secretClient;
    private bool _unavailable;

    /// <summary>
    ///     Retrieves a secret value from Azure Key Vault based on the provided key.
    /// </summary>
    /// <param name="key">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">
    ///     A token to observe; combined with <see cref="IAzureKeyVault.AzureKeyVaultTimeout" />.
    /// </param>
    /// <returns>
    ///     The secret value if found; otherwise <c>null</c>, including when the key is one of the vault's own
    ///     connection parameters or the vault is unavailable.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the Azure Key Vault address is not set.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is cancelled.</exception>
    public async ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken)
    {
        // Never look up the parameters needed to connect to the vault, or we'll end up in a loop
        if (_unavailable || IsConnectionParam(key))
            return null;

        logger.LogDebug("Attempting to get secret {Key} from Azure Key Vault", key);

        if (keyVault.AzureKeyVaultAddress is null or "")
        {
            var addressArg = buildDefinition.ParamDefinitions[nameof(IAzureKeyVault.AzureKeyVaultAddress)].ArgName;

            throw new InvalidOperationException(
                $"Azure Key Vault address '{addressArg}' must be set to use Azure Key Vault.");
        }

        try
        {
            var client = _secretClient ??= new(new(keyVault.AzureKeyVaultAddress), CreateCredential());

            return await GetSecretAsync(client, key, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _unavailable = true;

            logger.LogWarning(ex,
                "Failed to get '{Key}' from Azure Key Vault. Secrets will not be available from Azure Key Vault.",
                key);

            return null;
        }
    }

    /// <summary>
    ///     Selects and creates the credential used to access the vault.
    /// </summary>
    private TokenCredential CreateCredential()
    {
        var (kind, credential) = AzureCredentialFactory.Create(new()
        {
            TenantId = keyVault.AzureKeyVaultTenantId,
            ClientId = keyVault.AzureKeyVaultClientId,
            ClientSecret = keyVault.AzureKeyVaultClientSecret,
            ServiceConnectionId = keyVault.AzureKeyVaultServiceConnectionId,
            AllowInteractive = !args.HasHeadless,
            InteractiveRedirectPort = keyVault.AzureKeyVaultAuthPort,
        });

        logger.LogDebug("Using {CredentialKind} credential for Azure Key Vault", kind);

        if (kind is AzureCredentialKind.DefaultWithInteractiveFallback)
            logger.LogInformation(
                "No Azure Key Vault credentials were configured; using DefaultAzureCredential with an interactive browser login as a fallback.");

        return credential;
    }

    /// <summary>
    ///     Determines whether the given key is the argument name of one of the parameters used to connect to the
    ///     vault, which must never be looked up in the vault itself.
    /// </summary>
    private bool IsConnectionParam(string key) =>
        ConnectionParamNames.Any(name => buildDefinition.ParamDefinitions.TryGetValue(name, out var definition) &&
                                         definition.ArgName == key);

    /// <summary>
    ///     Retrieves a secret from the provided <see cref="SecretClient" />.
    /// </summary>
    /// <param name="client">The <see cref="SecretClient" /> to use for secret retrieval.</param>
    /// <param name="key">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">The caller's cancellation token.</param>
    /// <returns>The secret value if found, otherwise <c>null</c>.</returns>
    /// <exception cref="AuthenticationFailedException">Thrown if no credential could authenticate.</exception>
    private async Task<string?> GetSecretAsync(SecretClient client, string key, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Getting Azure Key Vault secret '{Key}'. If a login prompt appears, please authenticate with an Azure account that has access to the Vault.",
            key);

        var timeout = keyVault.AzureKeyVaultTimeout;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (timeout > 0)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeout));

        try
        {
            var response = await client.GetSecretAsync(key, cancellationToken: timeoutCts.Token);

            if (response.HasValue)
            {
                logger.LogTrace("Successfully retrieved secret '{Key}' from Azure Key Vault.", key);

                return response.Value.Value;
            }

            logger.LogWarning("Secret '{Key}' not found in Azure Key Vault.", key);
        }
        catch (RequestFailedException requestFailed) when (requestFailed.Status == 404)
        {
            logger.LogDebug("Secret '{Key}' not found in Azure Key Vault.", key);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _unavailable = true;

            logger.LogWarning(
                "Azure Key Vault secret retrieval for '{Key}' timed out after {Timeout} seconds. Secrets will not be available from Azure Key Vault.",
                key,
                timeout);
        }

        return null;
    }
}
