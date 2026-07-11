namespace Invex.Atom.Module.Dotnet.Tests;

[TestFixture]
internal sealed class PushPackageToNugetTests
{
    // A package path constructed from the default mock file system used by CreateTestHost.
    private static readonly RootedPath TestPackagePath = new(FileSystemUtils.DefaultMockFileSystem,
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\artifacts\MyPackage\MyPackage.1.0.0.nupkg"
            : "/Atom/artifacts/MyPackage/MyPackage.1.0.0.nupkg");

    private const string Feed = "https://api.nuget.org/v3/index.json";
    private const string ApiKey = "secret-api-key-abc123";

    private static IProcessRunner FakeRunnerReturning(int exitCode, string stdout = "", string stderr = "")
    {
        var fake = A.Fake<IProcessRunner>();

        A
            .CallTo(() => fake.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .Returns(Task.FromResult(new ProcessRunResult(new("dotnet", "test"), exitCode, stdout, stderr)));

        return fake;
    }

    private static INugetHelper CreateSut(IProcessRunner processRunner, CommandLineArgs? commandLineArgs = null)
    {
        var host = CreateTestHost<NugetHelperBuild>(commandLineArgs: commandLineArgs,
            configure: builder => builder.Services.AddSingleton(processRunner));

        return (INugetHelper)host.Services.GetRequiredService<IBuildDefinition>();
    }

    [Test]
    public async Task PushPackageToNuget_WhenProcessExitsZero_DoesNotThrow()
    {
        // Arrange
        var sut = CreateSut(FakeRunnerReturning(0));

        // Act / Assert
        await Should.NotThrowAsync(() => sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey));
    }

    [Test]
    public async Task PushPackageToNuget_WhenProcessExitsZero_InvokesRunAsyncOnce()
    {
        // Arrange
        var fakeRunner = FakeRunnerReturning(0);
        var sut = CreateSut(fakeRunner);

        // Act
        await sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey);

        // Assert
        A
            .CallTo(() => fakeRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task PushPackageToNuget_WhenDryRunEnabled_DoesNotInvokeRunAsync()
    {
        // Arrange
        var fakeRunner = FakeRunnerReturning(0);

        var dryRunArgs = new CommandLineArgs(true,
            [new ParamArg("nuget-dry-run", nameof(INugetHelper.NugetDryRun), "true")]);

        var sut = CreateSut(fakeRunner, dryRunArgs);

        // Act
        await sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey);

        // Assert
        A
            .CallTo(() => fakeRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task PushPackageToNuget_WhenCancelled_PropagatesOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var fakeRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => fakeRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .Returns(Task.FromCanceled<ProcessRunResult>(cts.Token));

        var sut = CreateSut(fakeRunner);

        // Act / Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey, cancellationToken: cts.Token));
    }

    [Test]
    public async Task PushPackageToNuget_WhenProcessExitsNonZero_ThrowsStepFailedException()
    {
        // Arrange
        var sut = CreateSut(FakeRunnerReturning(1, stderr: "Push rejected by server."));

        // Act / Assert
        var ex = await Should.ThrowAsync<StepFailedException>(() =>
            sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey));

        ex.Message.ShouldContain("exit code 1");
    }

    [Test]
    public async Task PushPackageToNuget_WhenProcessExitsNonZero_ExceptionMessageContainsSanitizedStderr()
    {
        // Arrange
        const string stderrWithKey = $"Authentication failed: key={ApiKey} is invalid.";

        var sut = CreateSut(FakeRunnerReturning(401, stderr: stderrWithKey));

        // Act
        var ex = await Should.ThrowAsync<StepFailedException>(() =>
            sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey));

        // Assert
        ex.Message.ShouldContain("exit code 401");
        ex.Message.ShouldNotContain(ApiKey);
        ex.Message.ShouldContain("[REDACTED]");
    }

    [Test]
    public async Task PushPackageToNuget_WhenProcessExitsNonZero_StderrWithoutKeyIsPreserved()
    {
        // Arrange
        const string stderrClean = "Package version already exists on the feed.";

        var sut = CreateSut(FakeRunnerReturning(1, stderr: stderrClean));

        // Act
        var ex = await Should.ThrowAsync<StepFailedException>(() =>
            sut.PushPackageToNuget(TestPackagePath, Feed, ApiKey));

        // Assert
        ex.Message.ShouldContain(stderrClean);
    }
}
