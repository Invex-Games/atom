namespace Invex.Atom.Module.AzureKeyVault.Tests;

[TestFixture]
internal sealed class AzureKeyVaultSecretsProviderTests
{
    private static ParamDefinition Param(string name, string argName, bool isSecret = false) =>
        new(name)
        {
            ArgName = argName,
            Description = name,
            Sources = ParamSource.All,
            IsSecret = isSecret,
            ChainedParams = [],
        };

    private static (AzureKeyVaultSecretsProvider Provider, IAzureKeyVault KeyVault) CreateProvider()
    {
        var buildDefinition = A.Fake<IBuildDefinition>();

        A
            .CallTo(() => buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                [nameof(IAzureKeyVault.AzureKeyVaultAddress)] =
                    Param(nameof(IAzureKeyVault.AzureKeyVaultAddress), "azure-key-vault-address"),
                [nameof(IAzureKeyVault.AzureKeyVaultTenantId)] =
                    Param(nameof(IAzureKeyVault.AzureKeyVaultTenantId), "azure-key-vault-tenant-id"),
                [nameof(IAzureKeyVault.AzureKeyVaultClientId)] =
                    Param(nameof(IAzureKeyVault.AzureKeyVaultClientId), "azure-key-vault-client-id"),
                [nameof(IAzureKeyVault.AzureKeyVaultClientSecret)] =
                    Param(nameof(IAzureKeyVault.AzureKeyVaultClientSecret), "azure-key-vault-client-secret", true),
            });

        var keyVault = A.Fake<IAzureKeyVault>();

        return (new(buildDefinition,
            new(true, []),
            keyVault,
            NullLogger<AzureKeyVaultSecretsProvider>.Instance), keyVault);
    }

    [TestCase("azure-key-vault-address")]
    [TestCase("azure-key-vault-tenant-id")]
    [TestCase("azure-key-vault-client-id")]
    [TestCase("azure-key-vault-client-secret")]
    public async Task GetSecretAsync_ForConnectionParam_ReturnsNullWithoutReadingVaultParams(string key)
    {
        // Arrange
        var (provider, keyVault) = CreateProvider();

        // Act
        var result = await provider.GetSecretAsync(key, CancellationToken.None);

        // Assert
        result.ShouldBeNull();

        A
            .CallTo(() => keyVault.AzureKeyVaultAddress)
            .MustNotHaveHappened();
    }

    [Test]
    public async Task GetSecretAsync_WithoutAddress_ThrowsWithArgName()
    {
        // Arrange
        var (provider, keyVault) = CreateProvider();

        A
            .CallTo(() => keyVault.AzureKeyVaultAddress)
            .Returns(null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await provider.GetSecretAsync("some-secret", CancellationToken.None));

        exception.Message.ShouldContain("azure-key-vault-address");
    }
}
