namespace Invex.Atom.Module.AzureStorage.Tests;

[TestFixture]
internal sealed class AzureBlobArtifactProviderTests
{
    private const string DevelopmentConnectionString = "UseDevelopmentStorage=true";

    [Test]
    public void CreateServiceClient_WithConnectionString_UsesConnectionStringAndIgnoresAccountUrl()
    {
        // Arrange
        var credentialSettingsRead = false;

        // Act
        var client = AzureBlobArtifactProvider.CreateServiceClient(DevelopmentConnectionString,
            "https://other.blob.core.windows.net",
            () =>
            {
                credentialSettingsRead = true;

                return new();
            },
            NullLogger.Instance);

        // Assert
        client.AccountName.ShouldBe("devstoreaccount1");
        credentialSettingsRead.ShouldBeFalse();
    }

    [Test]
    public void CreateServiceClient_WithAccountUrlOnly_UsesTokenCredential()
    {
        // Arrange
        AzureCredentialSettings? settings = null;

        // Act
        var client = AzureBlobArtifactProvider.CreateServiceClient(null,
            "https://myaccount.blob.core.windows.net",
            () => settings = new()
            {
                TenantId = "tenant",
                ClientId = "client",
                ClientSecret = "secret",
            },
            NullLogger.Instance);

        // Assert
        client.Uri.ShouldBe(new("https://myaccount.blob.core.windows.net"));
        client.AccountName.ShouldBe("myaccount");
        settings.ShouldNotBeNull();
    }

    [TestCase(null, null)]
    [TestCase("", " ")]
    [TestCase("$(AZURESTORAGE_ARTIFACT_CONNECTIONSTRING)", "$(AZURESTORAGE_ARTIFACT_ACCOUNT_URL)")]
    public void CreateServiceClient_WithoutConnectionStringOrAccountUrl_Throws(
        string? connectionString,
        string? accountUrl)
    {
        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            AzureBlobArtifactProvider.CreateServiceClient(connectionString,
                accountUrl,
                () => new(),
                NullLogger.Instance));

        exception.Message.ShouldContain("azurestorage-artifact-connectionstring");
        exception.Message.ShouldContain("azurestorage-artifact-account-url");
    }
}
