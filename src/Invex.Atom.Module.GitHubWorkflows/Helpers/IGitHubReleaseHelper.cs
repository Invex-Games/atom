namespace Invex.Atom.Module.GitHubWorkflows.Helpers;

/// <summary>
///     Provides helper methods for interacting with GitHub Releases within Invex.Atom builds.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IGitHubHelper" /> to provide functionality for
///     uploading artifacts to a GitHub Release, leveraging the GitHub API.
/// </remarks>
[PublicAPI]
public interface IGitHubReleaseHelper : IGitHubHelper
{
    /// <summary>
    ///     Uploads a build artifact to a specified GitHub Release.
    /// </summary>
    /// <param name="artifactName">
    ///     The name of the artifact to upload. This name should correspond to a directory within the
    ///     Atom artifacts directory.
    /// </param>
    /// <param name="releaseTag">The tag of the GitHub Release to which the artifact should be uploaded (e.g., "v1.0.0").</param>
    /// <param name="dryRunWhenNotRunningInGitHubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated (logged but not executed)
    ///     when the build is not running in a GitHub Actions environment.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no release exists with the given tag.</exception>
    /// <remarks>
    ///     The artifact will be zipped before uploading if it's a directory.
    /// </remarks>
    [PublicAPI]
    async Task UploadArtifactToRelease(
        string artifactName,
        string releaseTag,
        bool dryRunWhenNotRunningInGitHubActions = true,
        CancellationToken cancellationToken = default)
    {
        var artifactPath = RootedFileSystem.AtomArtifactsDirectory / artifactName;

        await UploadAssetToRelease(releaseTag, artifactPath, dryRunWhenNotRunningInGitHubActions, cancellationToken);
    }

    /// <summary>
    ///     Uploads a generic asset (file or directory) to a specified GitHub Release.
    /// </summary>
    /// <param name="releaseTag">The tag of the GitHub Release to which the asset should be uploaded (e.g., "v1.0.0").</param>
    /// <param name="assetPath">The path to the asset (file or directory) to upload.</param>
    /// <param name="dryRunWhenNotRunningInGitHubActions">
    ///     If <c>true</c> (default), the upload operation will be simulated (logged but not executed)
    ///     when the build is not running in a GitHub Actions environment.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no release exists with the given tag.</exception>
    /// <remarks>
    ///     If <paramref name="assetPath" /> points to a directory, it will be zipped into a `.zip` file
    ///     with the same name before uploading. If it points to a file that is not a `.zip` file,
    ///     a `.zip` file containing only that file is uploaded.
    /// </remarks>
    [PublicAPI]
    async Task UploadAssetToRelease(
        string releaseTag,
        RootedPath assetPath,
        bool dryRunWhenNotRunningInGitHubActions = true,
        CancellationToken cancellationToken = default)
    {
        if (!GitHub.IsGitHubActions && dryRunWhenNotRunningInGitHubActions)
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

        var client = new GitHubClient(new("Invex.Atom"), new InMemoryCredentialStore(new(GitHubToken)));

        var releases = await client.Repository.Release.GetAll(long.Parse(GitHub.Variables.RepositoryId));

        var release = releases.FirstOrDefault(x => x.TagName == releaseTag) ??
                      throw new InvalidOperationException(
                          $"No GitHub release with tag '{releaseTag}' exists in repository {GitHub.Variables.RepositoryId}.");

        if (assetPath.DirectoryExists)
        {
            var zipPath = RootedFileSystem.CreateRootedPath($"{assetPath}.zip");

            await ZipFile.CreateFromDirectoryAsync(assetPath, zipPath, cancellationToken);

            assetPath = zipPath;
        }
        else if (assetPath.FileName?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) is not true)
        {
            // Zip only this file, not its whole parent directory
            var zipPath = RootedFileSystem.CreateRootedPath($"{assetPath}.zip");

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                archive.CreateEntryFromFile(assetPath, assetPath.FileName ?? "asset");

            assetPath = zipPath;
        }

        var assetFile = RootedFileSystem.FileInfo.New(assetPath);

        await using var stream = assetFile.OpenRead();

        var asset = new ReleaseAssetUpload(assetPath.FileName, "application/zip", stream, TimeSpan.FromMinutes(5));
        await client.Repository.Release.UploadAsset(release, asset, cancellationToken);
    }
}
