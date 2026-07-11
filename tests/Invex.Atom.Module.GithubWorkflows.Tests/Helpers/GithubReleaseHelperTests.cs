using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Runtime.CompilerServices;
using FakeItEasy;
using Invex.Atom.Build.Exceptions;
using Octokit;
using Environment = System.Environment;

namespace Invex.Atom.Module.GithubWorkflows.Tests.Helpers;

/// <summary>
///     Minimal <see cref="IGithubReleaseHelper" /> implementation that wires the interface to a custom
///     <see cref="IServiceProvider" /> so tests can inject a mock <see cref="IGithubReleaseApi" />.
/// </summary>
internal sealed class ReleaseHelperSubject(IServiceProvider services) : IGithubReleaseHelper
{
    public IServiceProvider Services => services;
}

[TestFixture]
[NonParallelizable]
internal sealed class GithubReleaseHelperTests
{
    private const long RepositoryId = 99999L;
    private string? _originalGithubActions;
    private string? _originalRepositoryId;

    // Returns an IGithubReleaseHelper so default interface methods are callable without an extra cast.
    private static IGithubReleaseHelper BuildHelper(IGithubReleaseApi? api = null)
    {
        var sc = new ServiceCollection().AddLogging();

        if (api is not null)
            sc.AddSingleton(api);

        return new ReleaseHelperSubject(sc.BuildServiceProvider());
    }

    [SetUp]
    public void SetUp()
    {
        _originalGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        _originalRepositoryId = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_ID");
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "false");
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY_ID", RepositoryId.ToString());
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", _originalGithubActions);
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY_ID", _originalRepositoryId);
    }

    // CreateRelease payload and exact target

    [Test]
    public async Task CreateRelease_DraftTrue_PassesDraftPayloadToApi()
    {
        // Arrange
        var expectedRelease = CreateFakeRelease(1, "v1.0.0");
        NewRelease? captured = null;
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.CreateRelease(RepositoryId, A<NewRelease>._))
            .Invokes((long _, NewRelease nr) => captured = nr)
            .Returns(expectedRelease);

        var helper = BuildHelper(api);

        // Act
        var result = await helper.CreateRelease("v1.0.0", draft: true, dryRunWhenNotRunningInGithubActions: false);

        // Assert
        result.ShouldBe(expectedRelease);
        captured.ShouldNotBeNull();
        captured.Draft.ShouldBeTrue();
        captured.TagName.ShouldBe("v1.0.0");
    }

    [Test]
    public async Task CreateRelease_WithTargetCommitish_SetsExactTargetOnPayload()
    {
        // Arrange
        NewRelease? captured = null;
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.CreateRelease(RepositoryId, A<NewRelease>._))
            .Invokes((long _, NewRelease nr) => captured = nr)
            .Returns(CreateFakeRelease(2, "v2.0.0"));

        var helper = BuildHelper(api);

        // Act
        await helper.CreateRelease("v2.0.0", "refs/heads/release/2.0", dryRunWhenNotRunningInGithubActions: false);

        // Assert
        captured.ShouldNotBeNull();
        captured.TargetCommitish.ShouldBe("refs/heads/release/2.0");
    }

    [Test]
    public async Task CreateRelease_WhenNotInGithubActions_ReturnsDryRunNull()
    {
        // Arrange
        var helper = BuildHelper();

        // Act
        var result = await helper.CreateRelease("v1.0.0"); // dryRun = true (default)

        // Assert
        result.ShouldBeNull();
    }

    // FindRelease lookup and not-found behavior

    [Test]
    public async Task FindRelease_WhenReleaseExists_ReturnsMatchedRelease()
    {
        // Arrange
        var expected = CreateFakeRelease(10, "v1.0.0");
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.FindReleaseByTag(RepositoryId, "v1.0.0"))
            .Returns(expected);

        var helper = BuildHelper(api);

        // Act
        var result = await helper.FindRelease("v1.0.0", false);

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    public async Task FindRelease_WhenReleaseDoesNotExist_ReturnsNull()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.FindReleaseByTag(RepositoryId, "v99.0.0"))
            .Returns(Task.FromResult<Release?>(null));

        var helper = BuildHelper(api);

        // Act
        var result = await helper.FindRelease("v99.0.0", false);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task FindRelease_WhenNotInGithubActions_ReturnsDryRunNull()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();
        var helper = BuildHelper(api);

        // Act
        var result = await helper.FindRelease("v1.0.0"); // dryRun = true (default)

        // Assert
        result.ShouldBeNull();

        A
            .CallTo(() => api.FindReleaseByTag(A<long>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void FindRelease_WithCancelledToken_ThrowsOperationCancelled()
    {
        // Arrange
        var helper = BuildHelper(A.Fake<IGithubReleaseApi>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        Should.ThrowAsync<OperationCanceledException>(() => helper.FindRelease("v1.0.0", false, cts.Token));
    }

    [Test]
    public async Task UploadAssetToRelease_WhenReleaseDoesNotExist_ThrowsActionableFailure()
    {
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.FindReleaseByTag(RepositoryId, "v1.0.0"))
            .Returns(Task.FromResult<Release?>(null));

        var helper = BuildHelper(api);
        var fileSystem = new MockFileSystem();

        var assetPath = new RootedPath(fileSystem,
            Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\artifact.zip"
                : "/artifact.zip");

        var exception = await Should.ThrowAsync<StepFailedException>(() =>
            helper.UploadAssetToRelease("v1.0.0", assetPath, false));

        exception.Message.ShouldContain("v1.0.0");
    }

    // PublishRelease draft-to-published transition

    [Test]
    public async Task PublishRelease_SetsReleaseUpdateDraftToFalse()
    {
        // Arrange
        ReleaseUpdate? captured = null;
        var published = CreateFakeRelease(5, "v3.0.0");
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.EditRelease(RepositoryId, 5, A<ReleaseUpdate>._))
            .Invokes((long _, long _, ReleaseUpdate u) => captured = u)
            .Returns(published);

        var helper = BuildHelper(api);

        // Act
        var result = await helper.PublishRelease(5, false);

        // Assert
        result.ShouldBe(published);
        captured.ShouldNotBeNull();
        captured.Draft.ShouldBe(false);
    }

    [Test]
    public async Task PublishRelease_WhenNotInGithubActions_ReturnsDryRunNull()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();
        var helper = BuildHelper(api);

        // Act
        var result = await helper.PublishRelease(42); // dryRun = true (default)

        // Assert
        result.ShouldBeNull();

        A
            .CallTo(() => api.EditRelease(A<long>._, A<long>._, A<ReleaseUpdate>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void PublishRelease_WithCancelledToken_ThrowsOperationCancelled()
    {
        // Arrange
        var helper = BuildHelper(A.Fake<IGithubReleaseApi>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        Should.ThrowAsync<OperationCanceledException>(() => helper.PublishRelease(1, false, cts.Token));
    }

    // GetReleaseAssetNames

    [Test]
    public async Task GetReleaseAssetNames_ReturnsNamesOfAllAttachedAssets()
    {
        // Arrange
        var assets = new List<ReleaseAsset>
        {
            CreateFakeAsset("package.zip"),
            CreateFakeAsset("checksums.txt"),
        };

        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.GetReleaseAssets(RepositoryId, 7))
            .Returns(assets);

        var helper = BuildHelper(api);

        // Act
        var names = await helper.GetReleaseAssetNames(7, false);

        // Assert
        names.ShouldBe(["package.zip", "checksums.txt"], false);
    }

    [Test]
    public async Task GetReleaseAssetNames_WhenReleaseHasNoAssets_ReturnsEmptyList()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.GetReleaseAssets(RepositoryId, 8))
            .Returns(new List<ReleaseAsset>());

        var helper = BuildHelper(api);

        // Act
        var names = await helper.GetReleaseAssetNames(8, false);

        // Assert
        names.ShouldBeEmpty();
    }

    [Test]
    public async Task GetReleaseAssetNames_WhenNotInGithubActions_ReturnsDryRunEmptyList()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();
        var helper = BuildHelper(api);

        // Act
        var names = await helper.GetReleaseAssetNames(99); // dryRun = true (default)

        // Assert
        names.ShouldBeEmpty();

        A
            .CallTo(() => api.GetReleaseAssets(A<long>._, A<long>._))
            .MustNotHaveHappened();
    }

    // DeleteRelease cleanup and rollback

    [Test]
    public async Task DeleteRelease_CallsApiDeleteWithCorrectReleaseId()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();

        A
            .CallTo(() => api.DeleteRelease(RepositoryId, 20))
            .Returns(Task.CompletedTask);

        var helper = BuildHelper(api);

        // Act
        await helper.DeleteRelease(20, false);

        // Assert
        A
            .CallTo(() => api.DeleteRelease(RepositoryId, 20))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteRelease_WhenNotInGithubActions_SkipsApiCall()
    {
        // Arrange
        var api = A.Fake<IGithubReleaseApi>();
        var helper = BuildHelper(api);

        // Act
        await helper.DeleteRelease(99); // dryRun = true (default)

        // Assert
        A
            .CallTo(() => api.DeleteRelease(A<long>._, A<long>._))
            .MustNotHaveHappened();
    }

    [Test]
    public void DeleteRelease_WithCancelledToken_ThrowsOperationCancelled()
    {
        // Arrange
        var helper = BuildHelper(A.Fake<IGithubReleaseApi>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act / Assert
        Should.ThrowAsync<OperationCanceledException>(() => helper.DeleteRelease(1, false, cts.Token));
    }

    // Test helpers

    private static Release CreateFakeRelease(int id, string tagName)
    {
        // Octokit Release has no public parameterless constructor.
        var release = (Release)RuntimeHelpers.GetUninitializedObject(typeof(Release));
        SetProperty(release, "Id", id);
        SetProperty(release, "TagName", tagName);
        SetProperty(release, "Draft", false);

        return release;
    }

    private static ReleaseAsset CreateFakeAsset(string name)
    {
        var asset = (ReleaseAsset)RuntimeHelpers.GetUninitializedObject(typeof(ReleaseAsset));
        SetProperty(asset, "Name", name);

        return asset;
    }

    private static void SetProperty(object obj, string propertyName, object? value)
    {
        var prop = obj
            .GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        prop
            ?.GetSetMethod(true)
            ?.Invoke(obj, [value]);
    }
}
