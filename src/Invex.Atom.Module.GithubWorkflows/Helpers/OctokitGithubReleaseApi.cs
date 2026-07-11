namespace Invex.Atom.Module.GithubWorkflows.Helpers;

/// <summary>
///     The default <see cref="IGithubReleaseApi" /> implementation that delegates all release operations to an
///     Octokit <see cref="GitHubClient" />.
/// </summary>
/// <param name="client">The authenticated Octokit client used for all release API calls.</param>
/// <remarks>
///     To override the GitHub API behaviour in dependency injection (for example in tests), register a custom
///     <see cref="IGithubReleaseApi" /> before the host is built. When no registration is present,
///     <see cref="IGithubReleaseHelper" /> creates a short-lived instance of this class automatically.
/// </remarks>
[PublicAPI]
public sealed class OctokitGithubReleaseApi(GitHubClient client) : IGithubReleaseApi
{
    /// <inheritdoc />
    public async Task<Release> CreateRelease(long repositoryId, NewRelease newRelease) =>
        await client.Repository.Release.Create(repositoryId, newRelease);

    /// <inheritdoc />
    public async Task<Release?> FindReleaseByTag(long repositoryId, string tagName)
    {
        var releases = await client.Repository.Release.GetAll(repositoryId);

        return releases.FirstOrDefault(r => string.Equals(r.TagName, tagName, StringComparison.Ordinal));
    }

    /// <inheritdoc />
    public async Task<Release> EditRelease(long repositoryId, long releaseId, ReleaseUpdate update) =>
        await client.Repository.Release.Edit(repositoryId, releaseId, update);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ReleaseAsset>> GetReleaseAssets(long repositoryId, long releaseId) =>
        await client.Repository.Release.GetAllAssets(repositoryId, releaseId);

    /// <inheritdoc />
    public async Task DeleteRelease(long repositoryId, long releaseId) =>
        await client.Repository.Release.Delete(repositoryId, releaseId);

    /// <inheritdoc />
    public async Task<ReleaseAsset> UploadAsset(Release release, ReleaseAssetUpload upload) =>
        await client.Repository.Release.UploadAsset(release, upload);
}
