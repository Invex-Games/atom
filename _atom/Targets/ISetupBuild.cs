using LibGit2Sharp;
using Commit = LibGit2Sharp.Commit;
using Repository = LibGit2Sharp.Repository;
using Tag = LibGit2Sharp.Tag;

namespace Atom.Targets;

internal interface ISetupBuild : ISetupBuildInfo, IGithubHelper, IPullRequestHelper
{
    private static readonly string[] FilesToCheck =
    [
        "DecSm.Atom.Tests/ApiSurfaceTests/PublicApiSurfaceTests.VerifyPublicApiSurface.verified.txt",
        "DecSm.Atom/Paths/AtomFileSystem.cs",
    ];

    new Target SetupBuildInfo =>
        t => t
            .Extends<ISetupBuildInfo>(x => x.SetupBuildInfo, true)
            .RequiresParam(nameof(GithubToken), nameof(PullRequestNumber))
            .Executes(async cancellationToken =>
            {
                var targetFiles = FormatTargetFiles();

                var owner = Github.Variables.RepositoryOwner;
                Logger.LogDebug("Target repository owner: {Owner}", owner);

                var repository = Github.Variables
                    .Repository
                    .Split('/')
                    .Last();

                Logger.LogDebug("Target repository: {Repository}", repository);

                using var repo = new Repository(FileSystem.AtomRootDirectory);

                var currentCommitHash = repo.Head.Tip.Sha;
                Logger.LogDebug("Current commit hash: {CommitHash}", currentCommitHash);

                var currentVersion = BuildVersion;
                Logger.LogDebug("Current version: {Version}", currentVersion);

                var latestReleaseInfo = FindLatestReleaseInfo(repo, currentVersion);
                Logger.LogDebug("Latest release info: {ReleaseInfo}", latestReleaseInfo);

                if (latestReleaseInfo is null)
                    return;

                var releaseCommitHash = latestReleaseInfo.Value.Tag.Target.Sha!;
                Logger.LogDebug("Release commit hash: {CommitHash}", releaseCommitHash);

                var oldCommit = repo.Lookup<Commit>(releaseCommitHash);
                var newCommit = repo.Lookup<Commit>(currentCommitHash);
                var changes = repo.Diff.Compare<Patch>(oldCommit.Tree, newCommit.Tree);

                Logger.LogDebug("Changes: {@Changes}",
                    new
                    {
                        changes.Content,
                        changes.LinesDeleted,
                        changes.LinesAdded,
                    });

                IReadOnlyList<Change> suspiciousChanges = changes
                    .Where(x => targetFiles.Contains(x.Path) && x.LinesDeleted > 0)
                    .Select(x => new Change(FileSystem.AtomRootDirectory / x.Path, x.AddedLines, x.DeletedLines))
                    .ToList();

                Logger.LogDebug("Suspicious changes: {@SuspiciousChanges}", suspiciousChanges);

                var majorChanges = suspiciousChanges
                    .Where(x => x.DeletedLines.Count > 0 &&
                                x
                                    .DeletedLines
                                    .Select(l => l.Content.Trim())
                                    .All(deletedLine => !deletedLine.StartsWith(',') && !deletedLine.EndsWith(',')))
                    .ToList();

                Logger.LogDebug("Major changes: {@MajorChanges}", majorChanges);

                var minorChanges = suspiciousChanges
                    .Except(majorChanges)
                    .Where(x => x.AddedLines.Count > 0)
                    .ToList();

                Logger.LogDebug("Minor changes: {@MinorChanges}", minorChanges);

                var productHeader = new ProductHeaderValue("Atom");
                var connection = new Connection(productHeader, new InMemoryCredentialStore(GithubToken));

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

                var body = majorChanges.Count > 0
                    ? currentVersion.Major > latestReleaseInfo.Value.Version.Major
                        ? $"""
                           ⚠️ **Major Breaking Changes Detected**

                           This pull request contains major breaking changes to the public API surface.

                           **Version Bump Status:** ✅ Major version has been bumped from `{latestReleaseInfo.Value.Version.Major}` to `{currentVersion.Major}`

                           **Files with breaking changes:**
                           {string.Join("\n", majorChanges.Select(x => $"- `{x.Path}`"))}

                           The major version has already been appropriately incremented to reflect these breaking changes.
                           """
                        : $"""
                           ⚠️ **Major Breaking Changes Detected - Action Required**

                           This pull request contains major breaking changes to the public API surface, but the major version has not been bumped.

                           **Current Version:** `{currentVersion}`
                           **Latest Release:** `{latestReleaseInfo.Value.Version}`

                           **Files with breaking changes:**
                           {string.Join("\n", majorChanges.Select(x => $"- `{x.Path}` ({x.DeletedLines.Count} lines removed)"))}

                           **Required Action:** Please increment the major version number before merging this pull request.
                           """
                    : minorChanges.Count > 0
                        ? currentVersion.Minor > latestReleaseInfo.Value.Version.Minor
                            ? $"""
                               ℹ️ **Minor Breaking Changes Detected**

                               This pull request contains minor breaking changes to the public API surface.

                               **Version Bump Status:** ✅ Minor version has been bumped from `{latestReleaseInfo.Value.Version.Minor}` to `{currentVersion.Minor}`

                               **Files with breaking changes:**
                               {string.Join("\n", minorChanges.Select(x => $"- `{x.Path}`"))}

                               The minor version has already been appropriately incremented to reflect these changes.
                               """
                            : $"""
                               ℹ️ **Minor Breaking Changes Detected - Action Required**

                               This pull request contains minor breaking changes to the public API surface, but the minor version has not been bumped.

                               **Current Version:** `{currentVersion}`
                               **Latest Release:** `{latestReleaseInfo.Value.Version}`

                               **Files with breaking changes:**
                               {string.Join("\n", minorChanges.Select(x => $"- `{x.Path}` ({x.AddedLines.Count} lines added)"))}

                               **Required Action:** Please increment the minor version number before merging this pull request.
                               """
                        : """
                          ✅ **No Breaking Changes Detected**

                          This pull request does not contain any breaking changes to the public API surface.
                          Safe to merge without version bump considerations.
                          """;

                var addCommentMutation = new Mutation()
                    .AddComment(new AddCommentInput
                    {
                        SubjectId = prQueryResult.Id,
                        Body = body,
                    })
                    .Select(x => new
                    {
                        x.ClientMutationId,
                    })
                    .Compile();

                await connection.Run(addCommentMutation, cancellationToken: cancellationToken);

                var addCommentResult = await connection.Run(addCommentMutation, cancellationToken: cancellationToken);

                if (addCommentResult is null)
                    throw new StepFailedException("Could not add comment.");
            });

    private (Tag Tag, SemVer Version)? FindLatestReleaseInfo(Repository repo, SemVer currentVersion)
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

        return (version.Tag, version.Version);
    }

    private HashSet<string> FormatTargetFiles()
    {
        var targetFiles = FilesToCheck
            .Select(x => FileSystem.Path.IsPathRooted(x)
                ? FileSystem.Path.GetRelativePath(FileSystem.AtomRootDirectory, x)
                : x)
            .Select(x => x.Replace("\\", "/"))
            .Select(x => x.StartsWith('/')
                ? x[1..]
                : x)
            .ToHashSet();

        return targetFiles;
    }
}

internal sealed record Change(RootedPath Path, List<Line> AddedLines, List<Line> DeletedLines);
