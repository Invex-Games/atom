using FakeItEasy;
using Invex.Atom.Build.Args;
using Invex.Atom.Module.Dotnet.Helpers;
using Invex.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Invex.Atom.Module.Dotnet.Tests.Helpers;

[TestFixture]
public sealed class DotnetToolInstallHelperTests
{
    private const string ToolName = "GitVersion.Tool";

    private static string ListOutput(string version) =>
        $"""
         Package Id           Version      Commands
         ---------------------------------------------------
         gitversion.tool      {version}        dotnet-gitversion

         """;

    private static (IDotnetToolInstallHelper Helper, List<string> Calls) CreateSut(
        string? installedVersion,
        bool headless = false)
    {
        var calls = new List<string>();
        var processRunner = A.Fake<IProcessRunner>();

        ProcessRunResult Respond(ProcessRunOptions options)
        {
            calls.Add($"{options.Name} {options.Args}");

            return options.Args.StartsWith("tool list", StringComparison.Ordinal)
                ? installedVersion is null
                    ? new(options, 1, "Package Id      Version      Commands\n", string.Empty)
                    : new(options, 0, ListOutput(installedVersion), string.Empty)
                : new(options, 0, string.Empty, string.Empty);
        }

        A
            .CallTo(() => processRunner.Run(A<ProcessRunOptions>._))
            .ReturnsLazily((ProcessRunOptions options) => Respond(options));

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) => Task.FromResult(Respond(options)));

        var services = new ServiceCollection()
            .AddSingleton(processRunner)
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .AddSingleton(new CommandLineArgs(true,
                headless
                    ? [new HeadlessArg()]
                    : []))
            .BuildServiceProvider();

        return (new TestHelper(services), calls);
    }

    [Test]
    public void GetInstalledToolVersion_WhenInstalled_ReturnsVersion()
    {
        // Arrange
        var (helper, calls) = CreateSut("6.8.2");

        // Act
        var version = helper.GetInstalledToolVersion(ToolName);

        // Assert
        version.ShouldBe("6.8.2");
        calls.ShouldBe(["dotnet tool list GitVersion.Tool -g"]);
    }

    [Test]
    public async Task GetInstalledToolVersionAsync_WhenNotInstalled_ReturnsNull()
    {
        // Arrange
        var (helper, calls) = CreateSut(null);

        // Act
        var version = await helper.GetInstalledToolVersionAsync(ToolName, false);

        // Assert
        version.ShouldBeNull();
        calls.ShouldBe(["dotnet tool list GitVersion.Tool"]);
    }

    [TestCase(null)]
    [TestCase("6.*")]
    [TestCase("6.8.2")]
    public void InstallTool_WhenMatchingVersionInstalled_DoesNotUpdate(string? version)
    {
        // Arrange
        var (helper, calls) = CreateSut("6.8.2");

        // Act
        helper.InstallTool(ToolName, version);

        // Assert
        calls.ShouldBe(["dotnet tool list GitVersion.Tool -g"]);
    }

    [TestCase("6.8.2", "6.8")]
    [TestCase("6.8.2", "6.9.*")]
    [TestCase("7.0.0", "6.*")]
    public void InstallTool_WhenOtherVersionInstalled_UpdatesAllowingDowngrade(string installed, string version)
    {
        // Arrange
        var (helper, calls) = CreateSut(installed);

        // Act
        helper.InstallTool(ToolName, version);

        // Assert
        calls.Count.ShouldBe(2);
        calls[1]
            .ShouldBe($"dotnet tool update GitVersion.Tool --version {version} --allow-downgrade -g");
    }

    [Test]
    public void InstallTool_WhenNotInstalled_UpdatesWithoutVersion()
    {
        // Arrange
        var (helper, calls) = CreateSut(null);

        // Act
        helper.InstallTool(ToolName);

        // Assert
        calls.Count.ShouldBe(2);
        calls[1]
            .ShouldBe("dotnet tool update GitVersion.Tool  -g");
    }

    [Test]
    public void InstallTool_WhenHeadless_AlwaysUpdates()
    {
        // Arrange
        var (helper, calls) = CreateSut("6.8.2", true);

        // Act
        helper.InstallTool(ToolName, "6.*");

        // Assert
        calls.ShouldBe(["dotnet tool update GitVersion.Tool --version 6.* --allow-downgrade -g"]);
    }

    [Test]
    public async Task InstallToolAsync_WhenFloatingVersionMatches_DoesNotUpdate()
    {
        // Arrange
        var (helper, calls) = CreateSut("6.8.2");

        // Act
        await helper.InstallToolAsync(ToolName, "6.*");

        // Assert
        calls.ShouldBe(["dotnet tool list GitVersion.Tool -g"]);
    }

    [Test]
    public async Task InstallToolAsync_WhenVersionDiffers_Updates()
    {
        // Arrange
        var (helper, calls) = CreateSut("1.2.3");

        // Act
        await helper.InstallToolAsync(ToolName, "1.2");

        // Assert
        calls.Count.ShouldBe(2);
        calls[1]
            .ShouldBe("dotnet tool update GitVersion.Tool --version 1.2 --allow-downgrade -g");
    }

    private sealed class TestHelper(IServiceProvider services) : IDotnetToolInstallHelper
    {
        public IServiceProvider Services => services;
    }
}
