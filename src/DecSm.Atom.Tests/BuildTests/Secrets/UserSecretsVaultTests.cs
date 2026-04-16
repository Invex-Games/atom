namespace DecSm.Atom.Tests.BuildTests.Secrets;

[TestFixture]
public class UserSecretsVaultTests
{
    [Test]
    public void UserSecretsVault_WhenMatch_ReturnsSecret()
    {
        System.Diagnostics.Process.Start(new ProcessStartInfo("dotnet", "user-secrets set secret-1 SecretValue")
        {
            WorkingDirectory = ProjectSourcePath.Value,
        })!.WaitForExit();

        try
        {
            // Arrange
            var host = CreateTestHost<UserSecretsBuild>(commandLineArgs: new(true,
                [new CommandArg("UserSecretsTarget")]));

            var userSecretsBuild = (UserSecretsBuild)host.Services.GetRequiredService<IBuildDefinition>();

            ((DotnetUserSecretsProvider)host.Services.GetRequiredService<ISecretsProvider>()).SecretsAssembly =
                typeof(UserSecretsVaultTests).Assembly;

            // Act
            host.Run();

            // Assert
            userSecretsBuild.ExecutionValue.ShouldBe("SecretValue");
        }
        finally
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo("dotnet", "user-secrets remove secret-1")
            {
                WorkingDirectory = ProjectSourcePath.Value,
            })!.WaitForExit();
        }
    }

    [Test]
    public void UserSecretsVault_WhenNoMatch_ReturnsNull()
    {
        // Arrange
        var host = CreateTestHost<UserSecretsBuild>(commandLineArgs: new(true, [new CommandArg("UserSecretsTarget")]));

        var userSecretsBuild = (UserSecretsBuild)host.Services.GetRequiredService<IBuildDefinition>();

        ((DotnetUserSecretsProvider)host.Services.GetRequiredService<ISecretsProvider>()).SecretsAssembly =
            typeof(UserSecretsVaultTests).Assembly;

        // Act
        host.Run();

        // Assert
        userSecretsBuild.ExecutionValue.ShouldBeNull();
    }
}
