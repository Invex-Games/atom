// This file is compiled into each Azure module (linked from the module's .csproj), so its types stay internal to
// every assembly that includes it.

namespace Invex.Atom.Shared.Azure;

/// <summary>
///     The values used to select and build an Azure <see cref="TokenCredential" />.
/// </summary>
internal sealed record AzureCredentialSettings
{
    /// <summary>
    ///     Gets the Microsoft Entra tenant ID.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    ///     Gets the client (application) ID of the app registration or user-assigned managed identity.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    ///     Gets the client secret of the app registration.
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    ///     Gets the Azure DevOps service connection ID used for workload identity federation in Azure Pipelines.
    /// </summary>
    public string? ServiceConnectionId { get; init; }

    /// <summary>
    ///     Gets a value indicating whether an interactive browser login may be used as a last resort.
    /// </summary>
    public bool AllowInteractive { get; init; }

    /// <summary>
    ///     Gets the localhost port for the interactive login redirect, or 0 to let the library choose.
    /// </summary>
    public int InteractiveRedirectPort { get; init; }
}

/// <summary>
///     The kind of credential selected by <see cref="AzureCredentialFactory" />.
/// </summary>
internal enum AzureCredentialKind
{
    /// <summary>
    ///     A client secret for an app registration (<see cref="ClientSecretCredential" />).
    /// </summary>
    ClientSecret,

    /// <summary>
    ///     A GitHub Actions OIDC token exchanged through workload identity federation.
    /// </summary>
    GithubActionsOidc,

    /// <summary>
    ///     An Azure Pipelines OIDC token exchanged through a workload identity federation service connection.
    /// </summary>
    AzurePipelinesOidc,

    /// <summary>
    ///     <see cref="DefaultAzureCredential" /> (environment, workload identity, managed identity, Azure CLI, ...).
    /// </summary>
    Default,

    /// <summary>
    ///     <see cref="DefaultAzureCredential" />, falling back to an interactive browser login.
    /// </summary>
    DefaultWithInteractiveFallback,
}

/// <summary>
///     Selects and creates the Azure <see cref="TokenCredential" /> used by the Azure modules.
/// </summary>
/// <remarks>
///     Credentials are selected in this order:
///     <list type="number">
///         <item>
///             <description>Tenant ID, client ID and client secret: <see cref="ClientSecretCredential" />.</description>
///         </item>
///         <item>
///             <description>
///                 Tenant ID and client ID in GitHub Actions with <c>id-token: write</c> permission
///                 (<c>ACTIONS_ID_TOKEN_REQUEST_URL</c> and <c>ACTIONS_ID_TOKEN_REQUEST_TOKEN</c> are set): the job's OIDC
///                 token is exchanged through a federated credential.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Tenant ID, client ID and service connection ID in Azure Pipelines (<c>SYSTEM_OIDCREQUESTURI</c> and
///                 <c>SYSTEM_ACCESSTOKEN</c> are set): <see cref="AzurePipelinesCredential" />.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Otherwise <see cref="DefaultAzureCredential" />, followed by an interactive browser login when
///                 interactive use is allowed.
///             </description>
///         </item>
///     </list>
///     Empty values and unexpanded Azure DevOps macros (such as <c>$(MY_VARIABLE)</c>) are treated as unset.
/// </remarks>
internal static class AzureCredentialFactory
{
    /// <summary>
    ///     The audience Microsoft Entra expects for federated tokens.
    /// </summary>
    internal const string FederatedTokenAudience = "api://AzureADTokenExchange";

    internal const string GithubTokenRequestUrlVariable = "ACTIONS_ID_TOKEN_REQUEST_URL";
    internal const string GithubTokenRequestTokenVariable = "ACTIONS_ID_TOKEN_REQUEST_TOKEN";
    internal const string AzurePipelinesOidcRequestUriVariable = "SYSTEM_OIDCREQUESTURI";
    internal const string AzurePipelinesAccessTokenVariable = "SYSTEM_ACCESSTOKEN";

    private static readonly HttpClient SharedHttpClient = new();

    /// <summary>
    ///     Selects the credential kind for the given settings and environment.
    /// </summary>
    /// <param name="settings">The configured credential settings.</param>
    /// <param name="getEnvironmentVariable">Reads an environment variable; <c>null</c> when unset.</param>
    /// <returns>The selected <see cref="AzureCredentialKind" />.</returns>
    internal static AzureCredentialKind Select(
        AzureCredentialSettings settings,
        Func<string, string?> getEnvironmentVariable)
    {
        var tenantId = Normalize(settings.TenantId);
        var clientId = Normalize(settings.ClientId);

        if (tenantId is null || clientId is null)
            return DefaultKind(settings);

        if (Normalize(settings.ClientSecret) is not null)
            return AzureCredentialKind.ClientSecret;

        if (Normalize(getEnvironmentVariable(GithubTokenRequestUrlVariable)) is not null &&
            Normalize(getEnvironmentVariable(GithubTokenRequestTokenVariable)) is not null)
            return AzureCredentialKind.GithubActionsOidc;

        if (Normalize(settings.ServiceConnectionId) is not null &&
            Normalize(getEnvironmentVariable(AzurePipelinesOidcRequestUriVariable)) is not null &&
            Normalize(getEnvironmentVariable(AzurePipelinesAccessTokenVariable)) is not null)
            return AzureCredentialKind.AzurePipelinesOidc;

        return DefaultKind(settings);
    }

    /// <summary>
    ///     Selects and creates a credential for the given settings.
    /// </summary>
    /// <param name="settings">The configured credential settings.</param>
    /// <param name="getEnvironmentVariable">
    ///     Reads an environment variable; defaults to <see cref="Environment.GetEnvironmentVariable(string)" />.
    /// </param>
    /// <returns>The selected kind and the credential.</returns>
    internal static (AzureCredentialKind Kind, TokenCredential Credential) Create(
        AzureCredentialSettings settings,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

        var kind = Select(settings, getEnvironmentVariable);
        var tenantId = Normalize(settings.TenantId);
        var clientId = Normalize(settings.ClientId);

        TokenCredential credential = kind switch
        {
            AzureCredentialKind.ClientSecret => new ClientSecretCredential(tenantId,
                clientId,
                Normalize(settings.ClientSecret)),

            AzureCredentialKind.GithubActionsOidc => new ClientAssertionCredential(tenantId,
                clientId,
                cancellationToken => GetGithubActionsOidcTokenAsync(SharedHttpClient,
                    getEnvironmentVariable(GithubTokenRequestUrlVariable)!,
                    getEnvironmentVariable(GithubTokenRequestTokenVariable)!,
                    cancellationToken)),

            AzureCredentialKind.AzurePipelinesOidc => new AzurePipelinesCredential(tenantId,
                clientId,
                Normalize(settings.ServiceConnectionId),
                getEnvironmentVariable(AzurePipelinesAccessTokenVariable)),

            AzureCredentialKind.Default => CreateDefault(tenantId, clientId),

            AzureCredentialKind.DefaultWithInteractiveFallback => new ChainedTokenCredential(
                CreateDefault(tenantId, clientId),
                new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
                {
                    TenantId = tenantId,
                    AdditionallyAllowedTenants =
                    {
                        tenantId ?? "*",
                    },
                    RedirectUri = settings.InteractiveRedirectPort > 0
                        ? new($"http://localhost:{settings.InteractiveRedirectPort}")
                        : null,
                })),

            _ => throw new ArgumentOutOfRangeException(nameof(settings), kind, "Unknown credential kind."),
        };

        return (kind, credential);
    }

    /// <summary>
    ///     Requests a GitHub Actions OIDC token for the Microsoft Entra federated token audience.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for the request.</param>
    /// <param name="requestUrl">The value of <c>ACTIONS_ID_TOKEN_REQUEST_URL</c>.</param>
    /// <param name="requestToken">The value of <c>ACTIONS_ID_TOKEN_REQUEST_TOKEN</c>.</param>
    /// <param name="cancellationToken">A token to observe.</param>
    /// <returns>The OIDC token (JWT).</returns>
    /// <exception cref="InvalidOperationException">Thrown if the response does not contain a token.</exception>
    internal static async Task<string> GetGithubActionsOidcTokenAsync(
        HttpClient httpClient,
        string requestUrl,
        string requestToken,
        CancellationToken cancellationToken)
    {
        var separator = requestUrl.Contains('?')
            ? '&'
            : '?';

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{requestUrl}{separator}audience={Uri.EscapeDataString(FederatedTokenAudience)}");

        request.Headers.Authorization = new("Bearer", requestToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return document.RootElement.TryGetProperty("value", out var value) &&
               value.GetString() is { Length: > 0 } token
            ? token
            : throw new InvalidOperationException("The GitHub Actions OIDC token response did not contain a token.");
    }

    /// <summary>
    ///     Returns <c>null</c> for empty values and unexpanded Azure DevOps macros; otherwise the trimmed value.
    /// </summary>
    internal static string? Normalize(string? value)
    {
        value = value?.Trim();

        return value is null or "" || (value.StartsWith("$(", StringComparison.Ordinal) && value.EndsWith(')'))
            ? null
            : value;
    }

    private static AzureCredentialKind DefaultKind(AzureCredentialSettings settings) =>
        settings.AllowInteractive
            ? AzureCredentialKind.DefaultWithInteractiveFallback
            : AzureCredentialKind.Default;

    private static DefaultAzureCredential CreateDefault(string? tenantId, string? clientId) =>
        new(new DefaultAzureCredentialOptions
        {
            TenantId = tenantId,
            WorkloadIdentityClientId = clientId,
            ManagedIdentityClientId = clientId,
        });
}
