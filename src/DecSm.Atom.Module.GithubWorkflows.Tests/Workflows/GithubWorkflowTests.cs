using Environment = System.Environment;

namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[TestFixture]
public class GithubWorkflowTests
{
    private static string WorkflowDir =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\.github\workflows\"
            : "/Atom/.github/workflows/";

    private static string DependabotDir =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\.github\"
            : "/Atom/.github/";

    [Test]
    public void MinimalBuild_GeneratesNoWorkflows()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<MinimalBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        build.Run();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeFalse();
    }

    [Test]
    public async Task SimpleBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<SimpleBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}simple-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task DependentBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<DependentBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}dependent-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ArtifactBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var console = new TestConsole();
        var build = CreateTestHost<ArtifactBuild>(console, fileSystem, new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue(console.Output);

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}artifact-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task CustomArtifactBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var console = new TestConsole();

        var build = CreateTestHost<CustomArtifactBuild>(console,
            fileSystem,
            new(true, [new GenArg()]),
            configure: builder => builder.Services.AddSingleton<IArtifactProvider, TestArtifactProvider>());

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue(console.Output);

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}custom-artifact-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ManualInputBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<ManualInputBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}manual-input-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ManualInputStabilityBuild_GeneratesWorkflowWithStableInputs()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build1 = CreateTestHost<ManualInputStabilityBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true,
            [
                new GenArg(),
                new ParamArg("--string-param-without-default",
                    nameof(IManualInputStabilityTarget.StringParamWithoutDefault),
                    "1"),
                new ParamArg("--string-param-with-default",
                    nameof(IManualInputStabilityTarget.StringParamWithDefault),
                    "1"),
                new ParamArg("--bool-param-without-default",
                    nameof(IManualInputStabilityTarget.BoolParamWithoutDefault),
                    "true"),
                new ParamArg("--bool-param-with-default",
                    nameof(IManualInputStabilityTarget.BoolParamWithDefault),
                    "true"),
                new ParamArg("--choice-param-without-default",
                    nameof(IManualInputStabilityTarget.ChoiceParamWithoutDefault),
                    "choice 1"),
                new ParamArg("--choice-param-with-default",
                    nameof(IManualInputStabilityTarget.ChoiceParamWithDefault),
                    "choice 1"),
            ]));

        var build2 = CreateTestHost<ManualInputStabilityBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true,
            [
                new GenArg(),
                new ParamArg("--string-param-without-default",
                    nameof(IManualInputStabilityTarget.StringParamWithoutDefault),
                    "2"),
                new ParamArg("--string-param-with-default",
                    nameof(IManualInputStabilityTarget.StringParamWithDefault),
                    "2"),
                new ParamArg("--bool-param-without-default",
                    nameof(IManualInputStabilityTarget.BoolParamWithoutDefault),
                    "false"),
                new ParamArg("--bool-param-with-default",
                    nameof(IManualInputStabilityTarget.BoolParamWithDefault),
                    "false"),
                new ParamArg("--choice-param-without-default",
                    nameof(IManualInputStabilityTarget.ChoiceParamWithoutDefault),
                    "choice 2"),
                new ParamArg("--choice-param-with-default",
                    nameof(IManualInputStabilityTarget.ChoiceParamWithDefault),
                    "choice 2"),
            ]));

        // Act
        await build1.RunAsync();
        var workflow1 = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}manual-input-stability-workflow.yml");

        await build2.RunAsync();
        var workflow2 = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}manual-input-stability-workflow.yml");

        // Assert
        workflow1.ShouldBe(workflow2);
    }

    [Test]
    public async Task SetupDotnetBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<SetupDotnetBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}setup-dotnet.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ReleaseTriggerBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<ReleaseTriggerBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}releasetrigger-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task EnvironmentBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<EnvironmentBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}environment-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task CheckoutOptionsBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<CheckoutOptionBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}checkoutoption-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task DuplicateDependencyBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<DuplicateDependencyBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}duplicatedependency-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task PermissionsBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<PermissionsBuild>(fileSystem: fileSystem,
            commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}permissions-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task DependabotBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;

        var build = CreateTestHost<DependabotBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(DependabotDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{DependabotDir}dependabot.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }
}
