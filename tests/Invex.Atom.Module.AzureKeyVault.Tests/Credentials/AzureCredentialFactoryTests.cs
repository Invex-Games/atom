namespace Invex.Atom.Module.AzureKeyVault.Tests.Credentials;

[TestFixture]
internal sealed class AzureCredentialFactoryTests
{
    private static readonly Dictionary<string, string?> GitHubEnvironment = new()
    {
        [AzureCredentialFactory.GitHubTokenRequestUrlVariable] = "https://token.actions.example/request?api-version=2",
        [AzureCredentialFactory.GitHubTokenRequestTokenVariable] = "request-token",
    };

    private static readonly Dictionary<string, string?> AzurePipelinesEnvironment = new()
    {
        [AzureCredentialFactory.AzurePipelinesOidcRequestUriVariable] = "https://dev.azure.com/org/oidc",
        [AzureCredentialFactory.AzurePipelinesAccessTokenVariable] = "system-access-token",
    };

    private static readonly Dictionary<string, string?> EmptyEnvironment = new();

    private static Func<string, string?> Env(Dictionary<string, string?> values) =>
        name => values.GetValueOrDefault(name);

    private static AzureCredentialSettings Settings(
        string? tenantId = "tenant",
        string? clientId = "client",
        string? clientSecret = null,
        string? serviceConnectionId = null,
        bool allowInteractive = false) =>
        new()
        {
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            ServiceConnectionId = serviceConnectionId,
            AllowInteractive = allowInteractive,
        };

    [Test]
    public void Select_WithClientSecret_PrefersClientSecretEvenInGitHubActions() =>
        AzureCredentialFactory
            .Select(Settings(clientSecret: "secret"), Env(GitHubEnvironment))
            .ShouldBe(AzureCredentialKind.ClientSecret);

    [Test]
    public void Select_InGitHubActionsWithIdToken_UsesGitHubOidc() =>
        AzureCredentialFactory
            .Select(Settings(), Env(GitHubEnvironment))
            .ShouldBe(AzureCredentialKind.GitHubActionsOidc);

    [Test]
    public void Select_InAzurePipelinesWithServiceConnection_UsesAzurePipelinesOidc() =>
        AzureCredentialFactory
            .Select(Settings(serviceConnectionId: "connection"), Env(AzurePipelinesEnvironment))
            .ShouldBe(AzureCredentialKind.AzurePipelinesOidc);

    [Test]
    public void Select_InAzurePipelinesWithoutServiceConnection_UsesDefault() =>
        AzureCredentialFactory
            .Select(Settings(), Env(AzurePipelinesEnvironment))
            .ShouldBe(AzureCredentialKind.Default);

    [Test]
    public void Select_InAzurePipelinesWithoutAccessToken_UsesDefault() =>
        AzureCredentialFactory
            .Select(Settings(serviceConnectionId: "connection"),
                Env(new()
                {
                    [AzureCredentialFactory.AzurePipelinesOidcRequestUriVariable] = "https://dev.azure.com/org/oidc",
                }))
            .ShouldBe(AzureCredentialKind.Default);

    [Test]
    public void Select_WithoutTenantOrClient_UsesDefaultEvenWithSecretAndOidc()
    {
        AzureCredentialFactory
            .Select(Settings(null, clientSecret: "secret"), Env(GitHubEnvironment))
            .ShouldBe(AzureCredentialKind.Default);

        AzureCredentialFactory
            .Select(Settings(clientId: null, clientSecret: "secret"), Env(GitHubEnvironment))
            .ShouldBe(AzureCredentialKind.Default);
    }

    [Test]
    public void Select_WhenInteractiveAllowed_FallsBackToDefaultWithInteractive() =>
        AzureCredentialFactory
            .Select(Settings(null, null, allowInteractive: true), Env(EmptyEnvironment))
            .ShouldBe(AzureCredentialKind.DefaultWithInteractiveFallback);

    [Test]
    public void Select_WithUnexpandedDevOpsMacros_TreatsValuesAsUnset() =>
        AzureCredentialFactory
            .Select(Settings(clientSecret: "$(AZURE_KEY_VAULT_CLIENT_SECRET)",
                    serviceConnectionId: "$(AZURE_KEY_VAULT_SERVICE_CONNECTION_ID)"),
                Env(AzurePipelinesEnvironment))
            .ShouldBe(AzureCredentialKind.Default);

    [TestCase(null, null)]
    [TestCase("", null)]
    [TestCase("   ", null)]
    [TestCase("$(SOME_VARIABLE)", null)]
    [TestCase(" value ", "value")]
    [TestCase("$value", "$value")]
    public void Normalize_ReturnsExpectedValue(string? value, string? expected) =>
        AzureCredentialFactory
            .Normalize(value)
            .ShouldBe(expected);

    [TestCase(AzureCredentialKind.ClientSecret, typeof(ClientSecretCredential))]
    [TestCase(AzureCredentialKind.GitHubActionsOidc, typeof(ClientAssertionCredential))]
    [TestCase(AzureCredentialKind.AzurePipelinesOidc, typeof(AzurePipelinesCredential))]
    [TestCase(AzureCredentialKind.Default, typeof(DefaultAzureCredential))]
    [TestCase(AzureCredentialKind.DefaultWithInteractiveFallback, typeof(ChainedTokenCredential))]
    public void Create_ReturnsCredentialForSelectedKind(AzureCredentialKind expectedKind, Type expectedType)
    {
        // Arrange
        var (settings, environment) = expectedKind switch
        {
            AzureCredentialKind.ClientSecret => (Settings(clientSecret: "secret"), EmptyEnvironment),
            AzureCredentialKind.GitHubActionsOidc => (Settings(), GitHubEnvironment),
            AzureCredentialKind.AzurePipelinesOidc => (Settings(serviceConnectionId: "connection"),
                AzurePipelinesEnvironment),
            AzureCredentialKind.Default => (Settings(), EmptyEnvironment),
            _ => (Settings(allowInteractive: true), EmptyEnvironment),
        };

        // Act
        var (kind, credential) = AzureCredentialFactory.Create(settings, Env(environment));

        // Assert
        kind.ShouldBe(expectedKind);
        credential.ShouldBeOfType(expectedType);
    }

    [Test]
    public async Task GetGitHubActionsOidcTokenAsync_RequestsTokenForEntraAudience()
    {
        // Arrange
        var handler = new RecordingHandler("""{"count":1,"value":"github-jwt"}""");
        using var httpClient = new HttpClient(handler);

        // Act
        var token = await AzureCredentialFactory.GetGitHubActionsOidcTokenAsync(httpClient,
            "https://token.actions.example/request?api-version=2",
            "request-token",
            CancellationToken.None);

        // Assert
        token.ShouldBe("github-jwt");
        handler.Request.ShouldNotBeNull();

        handler
            .Request
            .RequestUri!
            .ToString()
            .ShouldBe("https://token.actions.example/request?api-version=2&audience=api%3A%2F%2FAzureADTokenExchange");

        handler.Request.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        handler.Request.Headers.Authorization.Parameter.ShouldBe("request-token");
    }

    [Test]
    public async Task GetGitHubActionsOidcTokenAsync_WithoutToken_Throws()
    {
        // Arrange
        using var httpClient = new HttpClient(new RecordingHandler("{}"));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() =>
            AzureCredentialFactory.GetGitHubActionsOidcTokenAsync(httpClient,
                "https://token.actions.example/request",
                "request-token",
                CancellationToken.None));
    }

    private sealed class RecordingHandler(string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody),
            });
        }
    }
}
