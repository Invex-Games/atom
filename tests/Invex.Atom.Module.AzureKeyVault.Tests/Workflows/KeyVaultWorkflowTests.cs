namespace Invex.Atom.Module.AzureKeyVault.Tests.Workflows;

[TestFixture]
internal sealed class KeyVaultWorkflowTests
{
    private static string GitHubWorkflowDir =>
        System.Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\.github\workflows\"
            : "/Atom/.github/workflows/";

    private static string DevOpsWorkflowDir =>
        System.Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\.devops\workflows\"
            : "/Atom/.devops/workflows/";

    [Test]
    public async Task GitHubKeyVaultBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<GitHubKeyVaultBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new CommandArg(nameof(IWorkflowBuildDefinition.Gen))]));

        // Act
        await build.RunAsync();

        // Assert
        var workflow = await fileSystem.File.ReadAllTextAsync($"{GitHubWorkflowDir}github-key-vault-workflow.yml");

        await Verify(workflow);
    }

    [Test]
    public async Task DevOpsKeyVaultBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<DevOpsKeyVaultBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new CommandArg(nameof(IWorkflowBuildDefinition.Gen))]));

        // Act
        await build.RunAsync();

        // Assert
        var workflow = await fileSystem.File.ReadAllTextAsync($"{DevOpsWorkflowDir}devops-key-vault-workflow.yml");

        await Verify(workflow);
    }

    [Test]
    public void KeyVaultBuild_RegistersInternalProviders()
    {
        // Arrange
        using var host = CreateTestHost<GitHubKeyVaultBuild>();

        // Act & Assert
        host
            .Services
            .GetServices<ISecretsProvider>()
            .ShouldContain(provider => provider is AzureKeyVaultSecretsProvider);

        host
            .Services
            .GetServices<IBuildOptionProvider>()
            .ShouldContain(provider => provider is AzureKeyVaultOptionsProvider);
    }
}
