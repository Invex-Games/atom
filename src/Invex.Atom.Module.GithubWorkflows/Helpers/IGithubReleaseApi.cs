namespace Invex.Atom.Module.GithubWorkflows.Helpers;

/// <summary>
///     Defines the low-level GitHub release API operations used by <see cref="IGithubReleaseHelper" />.
/// </summary>
/// <remarks>
///     This interface is the injectable seam between <see cref="IGithubReleaseHelper" /> and the Octokit HTTP
///     client, allowing unit tests to verify exact request payloads and transitions without hitting the live
///     GitHub API.
///     The built-in implementation, <see cref="OctokitGithubReleaseApi" />, delegates directly to an Octokit
///     <see cref="GitHubClient" /> instance.
///     Register a custom implementation via dependency injection to replace or wrap the default behaviour.
/// </remarks>
[PublicAPI]
public interface IGithubReleaseApi
{
    /// <summary>Creates a new GitHub release.</summary>
    /// <param name="repositoryId">The numeric GitHub repository identifier.</param>
    /// <param name="newRelease">The payload describing the release to create.</param>
    /// <returns>The created <see cref="Release" />.</returns>
    Task<Release> CreateRelease(long repositoryId, NewRelease newRelease);

    /// <summary>
    ///     Finds a GitHub release by its Git tag name, returning <c>null</c> when no matching release exists.
    /// </summary>
    /// <param name="repositoryId">The numeric GitHub repository identifier.</param>
    /// <param name="tagName">The Git tag name to search for (e.g. <c>"v1.0.0"</c>).</param>
    /// <returns>
    ///     The matching <see cref="Release" />, or <c>null</c> if no release with the given tag is found.
    /// </returns>
    Task<Release?> FindReleaseByTag(long repositoryId, string tagName);

    /// <summary>Updates an existing GitHub release.</summary>
    /// <param name="repositoryId">The numeric GitHub repository identifier.</param>
    /// <param name="releaseId">The numeric release identifier.</param>
    /// <param name="update">The update payload.</param>
    /// <returns>The updated <see cref="Release" />.</returns>
    Task<Release> EditRelease(long repositoryId, long releaseId, ReleaseUpdate update);

    /// <summary>Returns all assets currently attached to a GitHub release.</summary>
    /// <param name="repositoryId">The numeric GitHub repository identifier.</param>
    /// <param name="releaseId">The numeric release identifier.</param>
    /// <returns>A read-only list of <see cref="ReleaseAsset" /> instances.</returns>
    Task<IReadOnlyList<ReleaseAsset>> GetReleaseAssets(long repositoryId, long releaseId);

    /// <summary>Permanently deletes a GitHub release.</summary>
    /// <param name="repositoryId">The numeric GitHub repository identifier.</param>
    /// <param name="releaseId">The numeric release identifier.</param>
    /// <returns>A <see cref="Task" /> that completes when the release has been deleted.</returns>
    Task DeleteRelease(long repositoryId, long releaseId);

    /// <summary>Uploads a release asset to an existing GitHub release.</summary>
    /// <param name="release">The target <see cref="Release" />.</param>
    /// <param name="upload">The upload payload including the asset stream and content-type.</param>
    /// <returns>The created <see cref="ReleaseAsset" />.</returns>
    Task<ReleaseAsset> UploadAsset(Release release, ReleaseAssetUpload upload);
}
