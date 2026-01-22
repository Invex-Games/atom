using LibGit2Sharp;
using Commit = LibGit2Sharp.Commit;
using Repository = LibGit2Sharp.Repository;

namespace Atom.Targets;

public interface ICheckPrForBreakingChanges : IGithubHelper, IPullRequestHelper, ISetupBuildInfo
{
    private static readonly string[] FilesToCheck =
    [
        "DecSm.Atom.Tests/ApiSurfaceTests/PublicApiSurfaceTests.VerifyPublicApiSurface.verified.txt",
    ];

    Target CheckPrForBreakingChanges =>
        t => t
            .RequiresParam(nameof(GithubToken), nameof(PullRequestNumber))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .Executes(async cancellationToken =>
            {
                var actor = Github.Variables.Actor;
                var owner = Github.Variables.RepositoryOwner;

                var repository = Github.Variables
                    .Repository
                    .Split('/')
                    .Last();

                Logger.LogInformation("Github API action context: {Context}",
                    new
                    {
                        Actor = actor,
                        Owner = owner,
                        Repo = repository,
                    });

                using var repo = new Repository(FileSystem.AtomRootDirectory);
                var currentCommitHash = repo.Head.Tip.Sha;

                var currentVersion = BuildVersion;

                var latestReleaseVersion = repo
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
                    .ToList();

                if (latestReleaseVersion.Count is 0)
                {
                    Logger.LogWarning("No release found for current version {CurrentVersion}.", currentVersion);

                    return;
                }

                var releaseCommitHash = latestReleaseVersion.MaxBy(x => x.Version)!.Tag.Target.Sha!;

                var oldCommit = repo.Lookup<Commit>(releaseCommitHash);
                var newCommit = repo.Lookup<Commit>(currentCommitHash);

                var breakingChanges = ContainsChangedOrDeletedLines(
                    repo.Diff.Compare<Patch>(oldCommit.Tree, newCommit.Tree),
                    FilesToCheck);

                foreach (var breakingChange in breakingChanges)
                    Logger.LogWarning("[WARNING] {ChangePath} has {ChangeLinesDeleted} lines removed or modified.",
                        breakingChange.Path,
                        breakingChange.DeletedLines);

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

                var addCommentMutation = new Mutation()
                    .AddComment(new AddCommentInput
                    {
                        SubjectId = prQueryResult.Id,
                        Body = breakingChanges.Any()
                            ? $"""
                               ⚠️ **BREAKING CHANGES DETECTED!** ⚠️

                               This pull request contains breaking changes to the public API surface.
                               Found {breakingChanges.Count} file(s) with modifications that may affect consumers.

                               **Action Required:** Please review these changes carefully before merging. Consider:
                               - Updating major version number
                               - Documenting migration path
                               - Notifying consumers of breaking changes
                               """
                            : """
                              ✅ **No Breaking Changes Detected**

                              This pull request does not contain any breaking changes to the public API surface.
                              Safe to merge without version bump considerations.
                              """,
                    })
                    .Select(x => new
                    {
                        x.ClientMutationId,
                    })
                    .Compile();

                await connection.Run(addCommentMutation, cancellationToken: cancellationToken);

                var enableAutoMergeResult =
                    await connection.Run(addCommentMutation, cancellationToken: cancellationToken);

                if (enableAutoMergeResult is null)
                    throw new StepFailedException("Could not add comment.");
            });

    IReadOnlyList<(RootedPath Path, IReadOnlyList<string> DeletedLines)> ContainsChangedOrDeletedLines(
        Patch changes,
        IEnumerable<string> filePaths)
    {
        var targetFiles = filePaths
            .Select(x => FileSystem.Path.IsPathRooted(x)
                ? FileSystem.Path.GetRelativePath(FileSystem.AtomRootDirectory, x)
                : x)
            .Select(x => x.Replace("\\", "/"))
            .Select(x => x.StartsWith('/')
                ? x[1..]
                : x)
            .ToHashSet();

        return changes
            .Where(x => targetFiles.Contains(x.Path) && x.LinesDeleted > 0)
            .Select(change => (FileSystem.AtomRootDirectory / change.Path,
                (IReadOnlyList<string>)change.DeletedLines.ConvertAll(l => l.Content)))
            .ToList();
    }
}
