namespace Invex.Atom.Module.GithubWorkflows.Helpers;

/// <summary>
///     Provides helper methods for interacting with GitHub Releases within Invex.Atom builds.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IGithubHelper" /> to provide functionality for
///     uploading artifacts to a GitHub Release, leveraging the GitHub API.
/// </remarks>
[PublicAPI]
public interface IGithubReleaseHelper : IGithubHelper
{
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

        var client = new GitHubClient(new("Invex.Atom"), new InMemoryCredentialStore(new(GithubToken)));

        var newRelease = new NewRelease(tagName)
        {
            Name = name ?? tagName,
            Body = body,
            Draft = draft,
            Prerelease = prerelease,
        };

        if (!string.IsNullOrEmpty(targetCommitish))
            newRelease.TargetCommitish = targetCommitish;

        return await client.Repository.Release.Create(long.Parse(Github.Variables.RepositoryId), newRelease);
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

        var client = new GitHubClient(new("Invex.Atom"), new InMemoryCredentialStore(new(GithubToken)));

        var releases = await client.Repository.Release.GetAll(long.Parse(Github.Variables.RepositoryId));

        var release = releases.FirstOrDefault(x => x.TagName == releaseTag);

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
        await client.Repository.Release.UploadAsset(release, asset);
    }
}
