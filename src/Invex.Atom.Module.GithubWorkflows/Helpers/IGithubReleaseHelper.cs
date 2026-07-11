namespace Invex.Atom.Module.GithubWorkflows.Helpers;

/// <summary>
///     Provides helper methods for interacting with GitHub Releases within Invex.Atom builds.
/// </summary>
/// <remarks>
///     <para>
///         This interface extends <see cref="IGithubHelper" /> to provide functionality for creating, finding,
///         publishing, inspecting, and deleting GitHub Releases, as well as uploading assets.
///     </para>
///     <para>
///         All operations go through the <see cref="IGithubReleaseApi" /> seam. Register a custom
///         <see cref="IGithubReleaseApi" /> in dependency injection to replace the default Octokit
///         client; this is the recommended approach for unit testing.
///     </para>
/// </remarks>
[PublicAPI]
public interface IGithubReleaseHelper : IGithubHelper
{
    /// <summary>
    ///     Returns the registered <see cref="IGithubReleaseApi" />, falling back to an
    ///     <see cref="OctokitGithubReleaseApi" /> backed by <see cref="IGithubHelper.GithubToken" /> when
    ///     no registration is present.
    /// </summary>
    private IGithubReleaseApi GetOrCreateReleaseApi() =>
        Services.GetService<IGithubReleaseApi>() ??
        new OctokitGithubReleaseApi(new(new("Invex.Atom"), new InMemoryCredentialStore(new(GithubToken))));

    /// <summary>
    ///     Parses the GitHub repository ID from the <c>GITHUB_REPOSITORY_ID</c> environment variable.
    /// </summary>
    /// <returns>The parsed numeric repository identifier.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the environment variable is absent or cannot be parsed as a <see cref="long" />.
    /// </exception>
    private long ParseRepositoryId()
    {
        if (!long.TryParse(Github.Variables.RepositoryId, out var repositoryId))
            throw new InvalidOperationException(
                $"Unable to parse GitHub repository id from '{Github.VariableNames.RepositoryId}' " +
                $"(value: '{Github.Variables.RepositoryId}').");

        return repositoryId;
    }

    /// <summary>
    ///     Creates a GitHub Release for a new tag, tagging a specific commit or the latest commit on a branch.
    /// </summary>
    /// <param name="tagName">
    ///     The name of the tag to create for the release (e.g. "v1.0.0"). The tag is created if it does not
    ///     already exist.
    /// </param>
    /// <param name="targetCommitish">
    ///     Specifies the commitish value that determines where the Git tag is created from. This can be a branch name
    ///     (in which case the tag is created from the latest commit on that branch) or a commit SHA (in which case the
    ///     tag is created from that specific commit). If <c>null</c> or empty, the repository's default branch is used.
    /// </param>
    /// <param name="name">
    ///     The display name of the release. If <c>null</c>, the <paramref name="tagName" /> is used as the
    ///     release name.
    /// </param>
    /// <param name="body">The description/body text of the release. May be <c>null</c> for an empty body.</param>
    /// <param name="draft">If <c>true</c>, the release is created as an unpublished draft. Defaults to <c>false</c>.</param>
    /// <param name="prerelease">If <c>true</c>, the release is flagged as a pre-release. Defaults to <c>false</c>.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the create operation will be simulated (logged but not executed)
    ///     when the build is not running in a GitHub Actions environment.
    /// </param>
    /// <returns>
    ///     A <see cref="Task" /> that resolves to the created <see cref="Release" />, or <c>null</c> when the operation
    ///     was simulated because the build is not running in GitHub Actions.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <remarks>
    ///     GitHub automatically creates the Git tag <paramref name="tagName" /> from <paramref name="targetCommitish" />
    ///     as part of creating the release.
    /// </remarks>
    [PublicAPI]
    async Task<Release?> CreateRelease(
        string tagName,
        string? targetCommitish = null,
        string? name = null,
        string? body = null,
        bool draft = false,
        bool prerelease = false,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning(
                "Not running in GitHub Actions, simulating creation of release {TagName} targeting {TargetCommitish}.",
                tagName,
                string.IsNullOrEmpty(targetCommitish)
                    ? "<default branch>"
                    : targetCommitish);

            return null;
        }

        var newRelease = new NewRelease(tagName)
        {
            Name = name ?? tagName,
            Body = body,
            Draft = draft,
            Prerelease = prerelease,
        };

        if (!string.IsNullOrEmpty(targetCommitish))
            newRelease.TargetCommitish = targetCommitish;

        return await GetOrCreateReleaseApi()
            .CreateRelease(ParseRepositoryId(), newRelease);
    }

    /// <summary>
    ///     Finds an existing GitHub Release by its Git tag name.
    /// </summary>
    /// <param name="tagName">The tag name to look up (e.g. <c>"v1.0.0"</c>).</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the lookup is simulated (logged but not executed) when not running in
    ///     GitHub Actions, and <c>null</c> is returned.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token that can be used to cancel the operation before it begins.
    ///     Note: cancellation is checked before the network call; the underlying HTTP request is not cancelled.
    /// </param>
    /// <returns>
    ///     The matching <see cref="Release" />, <c>null</c> if no release with that tag exists, or <c>null</c>
    ///     when the operation was simulated.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is already cancelled.</exception>
    [PublicAPI]
    async Task<Release?> FindRelease(
        string tagName,
        bool dryRunWhenNotRunningInGithubActions = true,
        CancellationToken cancellationToken = default)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning("Not running in GitHub Actions, simulating lookup of release {TagName}.", tagName);

            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        return await GetOrCreateReleaseApi()
            .FindReleaseByTag(ParseRepositoryId(), tagName);
    }

    /// <summary>
    ///     Publishes a draft GitHub Release, making it publicly visible.
    /// </summary>
    /// <param name="releaseId">The numeric identifier of the draft release to publish.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the publish operation is simulated (logged but not executed) when not
    ///     running in GitHub Actions, and <c>null</c> is returned.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token that can be used to cancel the operation before it begins.
    ///     Note: cancellation is checked before the network call; the underlying HTTP request is not cancelled.
    /// </param>
    /// <returns>
    ///     The updated <see cref="Release" /> with <see cref="Release.Draft" /> set to <c>false</c>,
    ///     or <c>null</c> when the operation was simulated.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is already cancelled.</exception>
    [PublicAPI]
    async Task<Release?> PublishRelease(
        long releaseId,
        bool dryRunWhenNotRunningInGithubActions = true,
        CancellationToken cancellationToken = default)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning("Not running in GitHub Actions, simulating publish of release {ReleaseId}.", releaseId);

            return null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var update = new ReleaseUpdate
        {
            Draft = false,
        };

        return await GetOrCreateReleaseApi()
            .EditRelease(ParseRepositoryId(), releaseId, update);
    }

    /// <summary>
    ///     Returns the file names of all assets currently attached to a GitHub Release.
    /// </summary>
    /// <param name="releaseId">The numeric identifier of the release.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the listing is simulated (logged but not executed) when not running in
    ///     GitHub Actions, and an empty list is returned.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token that can be used to cancel the operation before it begins.
    ///     Note: cancellation is checked before the network call; the underlying HTTP request is not cancelled.
    /// </param>
    /// <returns>
    ///     A read-only list of asset file names (the <see cref="ReleaseAsset.Name" /> of each asset),
    ///     or an empty list when the operation was simulated or the release has no assets.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is already cancelled.</exception>
    [PublicAPI]
    async Task<IReadOnlyList<string>> GetReleaseAssetNames(
        long releaseId,
        bool dryRunWhenNotRunningInGithubActions = true,
        CancellationToken cancellationToken = default)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning("Not running in GitHub Actions, simulating asset listing for release {ReleaseId}.",
                releaseId);

            return [];
        }

        cancellationToken.ThrowIfCancellationRequested();

        var assets = await GetOrCreateReleaseApi()
            .GetReleaseAssets(ParseRepositoryId(), releaseId);

        return assets
            .Select(a => a.Name)
            .ToList();
    }

    /// <summary>
    ///     Permanently deletes a GitHub Release. Use this for rollback when a release pipeline fails after
    ///     the release was created.
    /// </summary>
    /// <param name="releaseId">The numeric identifier of the release to delete.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the deletion is simulated (logged but not executed) when not running in
    ///     GitHub Actions.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token that can be used to cancel the operation before it begins.
    ///     Note: cancellation is checked before the network call; the underlying HTTP request is not cancelled.
    /// </param>
    /// <returns>A <see cref="Task" /> that completes when the release has been deleted.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken" /> is already cancelled.</exception>
    [PublicAPI]
    async Task DeleteRelease(
        long releaseId,
        bool dryRunWhenNotRunningInGithubActions = true,
        CancellationToken cancellationToken = default)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning("Not running in GitHub Actions, simulating deletion of release {ReleaseId}.", releaseId);

            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        await GetOrCreateReleaseApi()
            .DeleteRelease(ParseRepositoryId(), releaseId);
    }

    /// <summary>
    ///     Uploads a build artifact to a specified GitHub Release.
    /// </summary>
    /// <param name="artifactName">
    ///     The name of the artifact to upload. This name should correspond to a directory within the
    ///     Atom artifacts directory.
    /// </param>
    /// <param name="releaseTag">The tag of the GitHub Release to which the artifact should be uploaded (e.g., "v1.0.0").</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated (logged but not executed)
    ///     when the build is not running in a GitHub Actions environment.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     The artifact will be zipped before uploading if it's a directory.
    ///     The method will attempt to find an existing release with the given tag.
    /// </remarks>
    [PublicAPI]
    async Task UploadArtifactToRelease(
        string artifactName,
        string releaseTag,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        var artifactPath = RootedFileSystem.AtomArtifactsDirectory / artifactName;

        await UploadAssetToRelease(releaseTag, artifactPath, dryRunWhenNotRunningInGithubActions);
    }

    /// <summary>
    ///     Uploads a build artifact to a specific GitHub Release.
    /// </summary>
    /// <param name="artifactName">
    ///     The name of the artifact directory within the Atom artifacts directory.
    /// </param>
    /// <param name="release">The GitHub Release to which the artifact should be uploaded.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated when not running in GitHub Actions.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    [PublicAPI]
    async Task UploadArtifactToRelease(
        string artifactName,
        Release release,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        var artifactPath = RootedFileSystem.AtomArtifactsDirectory / artifactName;

        await UploadAssetToRelease(release, artifactPath, dryRunWhenNotRunningInGithubActions);
    }

    /// <summary>
    ///     Uploads a generic asset (file or directory) to a specified GitHub Release.
    /// </summary>
    /// <param name="releaseTag">The tag of the GitHub Release to which the asset should be uploaded (e.g., "v1.0.0").</param>
    /// <param name="assetPath">The path to the asset (file or directory) to upload.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated (logged but not executed)
    ///     when the build is not running in a GitHub Actions environment.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the GitHub repository ID cannot be parsed.</exception>
    /// <remarks>
    ///     If <paramref name="assetPath" /> points to a directory, it will be zipped into a `.zip` file
    ///     with the same name before uploading. If it points to a file that is not a `.zip` file,
    ///     it will also be zipped.
    /// </remarks>
    [PublicAPI]
    async Task UploadAssetToRelease(
        string releaseTag,
        RootedPath assetPath,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning(
                "Not running in GitHub Actions, simulating upload of artifact {AssetPath} to release {ReleaseTag}.",
                assetPath,
                releaseTag);

            if (assetPath.DirectoryExists)
                Logger.LogInformation("Artifact path {AssetPath} is a directory containing {FileCount} files.",
                    assetPath,
                    RootedFileSystem.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories)
                        .Length);
            else
                Logger.LogInformation("Artifact path {AssetPath} is a file.", assetPath);

            return;
        }

        var api = GetOrCreateReleaseApi();
        var repositoryId = ParseRepositoryId();

        var release = await api.FindReleaseByTag(repositoryId, releaseTag) ??
                      throw new StepFailedException($"Could not find GitHub release with tag '{releaseTag}'.");

        await UploadAssetToReleaseCore(api, release, assetPath);
    }

    /// <summary>
    ///     Uploads a generic asset to a specific GitHub Release.
    /// </summary>
    /// <param name="release">The GitHub Release to which the asset should be uploaded.</param>
    /// <param name="assetPath">The file or directory to upload.</param>
    /// <param name="dryRunWhenNotRunningInGithubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated when not running in GitHub Actions.
    /// </param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    [PublicAPI]
    async Task UploadAssetToRelease(
        Release release,
        RootedPath assetPath,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning(
                "Not running in GitHub Actions, simulating upload of artifact {AssetPath} to release {ReleaseTag}.",
                assetPath,
                release.TagName);

            return;
        }

        await UploadAssetToReleaseCore(GetOrCreateReleaseApi(), release, assetPath);
    }

    private async Task UploadAssetToReleaseCore(IGithubReleaseApi api, Release release, RootedPath assetPath)
    {

        if (assetPath.DirectoryExists)
        {
            var zipPath = RootedFileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath, zipPath);
            #endif

            assetPath = zipPath;
        }

        var assetFile = RootedFileSystem.FileInfo.New(assetPath);

        if (!assetFile.FullName.EndsWith(".zip"))
        {
            // zip the file
            var zipPath = RootedFileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath.Parent!, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath.Parent!, zipPath);
            #endif

            assetPath = zipPath;
        }

        assetFile = RootedFileSystem.FileInfo.New(assetPath);

        await using var stream = assetFile.OpenRead();

        var asset = new ReleaseAssetUpload(assetPath.FileName, "application/zip", stream, TimeSpan.FromMinutes(5));
        await api.UploadAsset(release, asset);
    }
}
