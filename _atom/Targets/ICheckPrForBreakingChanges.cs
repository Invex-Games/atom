using Repository = LibGit2Sharp.Repository;

namespace Atom.Targets;

public sealed record ReleaseInfo(string CommitHash, SemVer Version);

public interface ICheckPrForBreakingChanges : IGithubHelper, IPullRequestHelper, ISetupBuildInfo, IApiSurfaceHelper
{
    private RootedPath[] FilesToCheck =>
    [
        FileSystem.AtomRootDirectory /
        "DecSm.Atom.Tests" /
        "ApiSurfaceTests" /
        "PublicApiSurfaceTests.VerifyPublicApiSurface.verified.txt",
    ];

    Target CheckPrForBreakingChanges =>
        t => t
            .RequiresParam(nameof(GithubToken), nameof(PullRequestNumber))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .Executes(async cancellationToken =>
            {
                var owner = Github.Variables.RepositoryOwner;
                Logger.LogDebug("Target repository owner: {Owner}", owner);

                using var repo = new Repository(FileSystem.AtomRootDirectory);

                var currentCommitHash = repo.Head.Tip.Sha;
                Logger.LogDebug("Current commit hash: {CommitHash}", currentCommitHash);

                var currentVersion = BuildVersion;
                Logger.LogDebug("Current version: {Version}", currentVersion);

                var latestReleaseInfo = FindLatestReleaseInfo(repo, currentVersion);
                Logger.LogDebug("Latest release info: {ReleaseInfo}", latestReleaseInfo);

                if (latestReleaseInfo is null)
                    return;

                var breakingChanges = IdentifyBreakingChanges(latestReleaseInfo.Version,
                    latestReleaseInfo.CommitHash,
                    currentVersion,
                    currentCommitHash,
                    FilesToCheck);

                var body = breakingChanges.MajorChanges.Count > 0
                    ? currentVersion.Major > latestReleaseInfo.Version.Major
                        ? $"""
                           ℹ️ **Major Breaking Changes Detected**

                           This pull request contains major breaking changes to the public API surface.

                           **Version Bump Status:** ✅ Major version has been bumped from `{latestReleaseInfo.Version.Major}` to `{currentVersion.Major}`

                           **Files with breaking changes:**
                           {string.Join("\n", breakingChanges.MajorChanges.Select(x => $"- `{x.Path}`"))}

                           The major version has already been appropriately incremented to reflect these breaking changes.
                           """
                        : $"""
                           ⚠️ **Major Breaking Changes Detected - Action Required**

                           This pull request contains major breaking changes to the public API surface, but the major version has not been bumped.

                           **Current Version:** `{currentVersion}`
                           **Latest Release:** `{latestReleaseInfo.Version}`

                           **Files with breaking changes:**
                           {string.Join("\n", breakingChanges.MajorChanges.Select(x => $"- `{x.Path}` ({x.DeletedLines.Count} lines removed)"))}

                           **Required Action:** Please increment the major version number before merging this pull request.
                           """
                    : breakingChanges.MinorChanges.Count > 0
                        ? currentVersion.Minor > latestReleaseInfo.Version.Minor
                            ? $"""
                               ℹ️ **Minor Breaking Changes Detected**

                               This pull request contains minor breaking changes to the public API surface.

                               **Version Bump Status:** ✅ Minor version has been bumped from `{latestReleaseInfo.Version.Minor}` to `{currentVersion.Minor}`

                               **Files with breaking changes:**
                               {string.Join("\n", breakingChanges.MinorChanges.Select(x => $"- `{x.Path}`"))}

                               The minor version has already been appropriately incremented to reflect these changes.
                               """
                            : $"""
                               ⚠️ **Minor Breaking Changes Detected - Action Required**

                               This pull request contains minor breaking changes to the public API surface, but the minor version has not been bumped.

                               **Current Version:** `{currentVersion}`
                               **Latest Release:** `{latestReleaseInfo.Version}`

                               **Files with breaking changes:**
                               {string.Join("\n", breakingChanges.MinorChanges.Select(x => $"- `{x.Path}` ({x.AddedLines.Count} lines added)"))}

                               **Required Action:** Please increment the minor version number before merging this pull request.
                               """
                        : """
                          ✅ **No Breaking Changes Detected**

                          This pull request does not contain any breaking changes to the public API surface.
                          Safe to merge without version bump considerations.
                          """;

                var hasInvalidChanges = breakingChanges switch
                {
                    { MajorChanges.Count: > 0 } when currentVersion.Major <= latestReleaseInfo.Version.Major => true,
                    { MinorChanges.Count: > 0 } when currentVersion.Minor <= latestReleaseInfo.Version.Minor => true,
                    _ => false,
                };

                await AddCheckStatus(owner,
                    hasInvalidChanges
                        ? "failure"
                        : "success",
                    body,
                    cancellationToken);
            });

    ReleaseInfo? FindLatestReleaseInfo(Repository repo, SemVer currentVersion)
    {
        var releaseVersions = repo
            .Tags
            .Select(x => new
            {
                Tag = x,
                Version = !x.FriendlyName.StartsWith('v')
                    ? null
                    : !SemVer.TryParse(x.FriendlyName[1..], out var version)
                        ? null
                        : version,
            })
            .Where(x => x.Version is not null && x.Version < currentVersion)
            .Select(x => new
            {
                Tag = x.Tag!,
                Version = x.Version!,
            })
            .ToList();

        if (releaseVersions.Count is 0)
        {
            Logger.LogWarning("No release found for current version {CurrentVersion}.", currentVersion);

            return null;
        }

        var version = releaseVersions.MaxBy(x => x.Version)!;

        return new(version.Tag.Target.Sha, version.Version);
    }

    private async Task AddCheckStatus(
        string owner,
        string status,
        string description,
        CancellationToken cancellationToken)
    {
        var repository = Github.Variables
            .Repository
            .Split('/')
            .Last();

        Logger.LogDebug("Target repository: {Repository}", repository);

        var productHeader = new ProductHeaderValue("Atom");
        var connection = new Connection(productHeader, new InMemoryCredentialStore(GithubToken));

        var repoQuery = new Query()
            .Repository(repository, owner)
            .Select(r => new
            {
                r.Id,
            })
            .Compile();

        var repoQueryResult = await connection.Run(repoQuery, cancellationToken: cancellationToken);

        if (repoQueryResult.Id.Value is null)
            throw new StepFailedException("Could not find repository.");

        var prQuery = new Query()
            .Repository(repository, owner)
            .PullRequest(PullRequestNumber)
            .Select(p => new
            {
                p.Id,
                p.HeadRefOid,
            })
            .Compile();

        var prQueryResult = await connection.Run(prQuery, cancellationToken: cancellationToken);

        if (prQueryResult.Id.Value is null)
            throw new StepFailedException("Could not find pull request.");

        using var repo = new Repository(FileSystem.AtomRootDirectory);

        var checkRunMutation = new Mutation()
            .CreateCheckRun(new CreateCheckRunInput
            {
                RepositoryId = repoQueryResult.Id,
                Name = "API Surface Breaking Changes Check",
                HeadSha = prQueryResult.HeadRefOid,
                Status = RequestableCheckStatusState.Completed,
                Conclusion = status == "success"
                    ? CheckConclusionState.Success
                    : CheckConclusionState.Failure,
                CompletedAt = DateTimeOffset.UtcNow,
                Output = new()
                {
                    Title = "Breaking Changes Analysis",
                    Summary = description,
                },
            })
            .Select(x => new
            {
                x.ClientMutationId,
            })
            .Compile();

        var checkRunResult = await connection.Run(checkRunMutation, cancellationToken: cancellationToken);

        if (checkRunResult is null)
            throw new StepFailedException("Could not create check run.");
    }
}
