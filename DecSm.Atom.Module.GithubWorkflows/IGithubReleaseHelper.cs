namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides helper methods for interacting with GitHub Releases within DecSm.Atom builds.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IGithubHelper" /> to provide functionality for
///     uploading artifacts to a GitHub Release, leveraging the GitHub API.
/// </remarks>
[PublicAPI]
public interface IGithubReleaseHelper : IGithubHelper
{
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
        var artifactPath = FileSystem.AtomArtifactsDirectory / artifactName;

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
                    FileSystem.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories)
                        .Length);
            else
                Logger.LogInformation("Artifact path {AssetPath} is a file.", assetPath);

            return;
        }

        var client = new GitHubClient(new("DecSm.Atom"), new InMemoryCredentialStore(new(GithubToken)));

        var releases = await client.Repository.Release.GetAll(long.Parse(Github.Variables.RepositoryId));

        var release = releases.FirstOrDefault(x => x.TagName == releaseTag);

        if (assetPath.DirectoryExists)
        {
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath, zipPath);
            #endif

            assetPath = zipPath;
        }

        var assetFile = FileSystem.FileInfo.New(assetPath);

        if (!assetFile.FullName.EndsWith(".zip"))
        {
            // zip the file
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath.Parent!, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath.Parent!, zipPath);
            #endif

            assetPath = zipPath;
        }

        assetFile = FileSystem.FileInfo.New(assetPath);

        await using var stream = assetFile.OpenRead();

        var asset = new ReleaseAssetUpload(assetPath.FileName, "application/zip", stream, TimeSpan.FromMinutes(5));
        await client.Repository.Release.UploadAsset(release, asset);
    }
}
