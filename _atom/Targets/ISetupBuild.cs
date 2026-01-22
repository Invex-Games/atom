using LibGit2Sharp;
using Commit = LibGit2Sharp.Commit;
using Repository = LibGit2Sharp.Repository;

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

                var changes = repo.Diff.Compare<Patch>(oldCommit.Tree, newCommit.Tree);

                var targetFiles = FilesToCheck
                    .Select(x => FileSystem.Path.IsPathRooted(x)
                        ? FileSystem.Path.GetRelativePath(FileSystem.AtomRootDirectory, x)
                        : x)
                    .Select(x => x.Replace("\\", "/"))
                    .Select(x => x.StartsWith('/')
                        ? x[1..]
                        : x)
                    .ToHashSet();

                var breakingChanges = (IReadOnlyList<(RootedPath Path, IReadOnlyList<string> DeletedLines)>)changes
                    .Where(x => targetFiles.Contains(x.Path) && x.LinesDeleted > 0)
                    .Select(change => (FileSystem.AtomRootDirectory / change.Path,
                        (IReadOnlyList<string>)change.DeletedLines.ConvertAll(l => l.Content)))
                    .ToList();

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

                if (breakingChanges.Count is 0)
                {
                    // Add a comment describing that there are no breaking changes
                    var addCommentMutation = new Mutation()
                        .AddComment(new AddCommentInput
                        {
                            SubjectId = prQueryResult.Id,
                            Body = """
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

                    var addCommentResult =
                        await connection.Run(addCommentMutation, cancellationToken: cancellationToken);

                    if (addCommentResult is null)
                        throw new StepFailedException("Could not add comment.");
                }
                else
                {
                    // Add a review comment that must be acknowledged, describing that there are breaking changes
                    var breakingChangesDescription = string.Join("\n",
                        breakingChanges.Select(x =>
                            $"- `{x.Path}`: {x.DeletedLines.Count} line(s) removed or modified"));

                    var addPullRequestReviewThread = new Mutation()
                        .AddPullRequestReview(new AddPullRequestReviewInput
                        {
                            PullRequestId = prQueryResult.Id,
                            Body = $"""
                                    ⚠️ **Breaking Changes Detected**

                                    This pull request contains breaking changes to the public API surface.
                                    Please review the following changes carefully:

                                    {breakingChangesDescription}

                                    **Action Required:**
                                    - Ensure the version bump is appropriate (major version for breaking changes)
                                    - Update migration documentation if necessary
                                    - Verify all consumers can handle these changes
                                    """,
                            Event = PullRequestReviewEvent.RequestChanges,
                            Threads = breakingChanges.Select(x =>
                            {
                                var path = FileSystem
                                    .Path
                                    .GetRelativePath(FileSystem.AtomRootDirectory, x.Path)
                                    .Replace("\\", "/")
                                    .Trim();

                                Logger.LogInformation("Requesting changes for breaking change in {Path}: {Join}...",
                                    path,
                                    string.Join(", ", x.DeletedLines));

                                return new DraftPullRequestReviewThread
                                {
                                    Path = $"'{path}'",
                                    Body = $"""
                                            ⚠️ Breaking changes detected in this file:

                                            {string.Join("\n", x.DeletedLines.Select((line, index) => $"Line {index + 1}: {line.TrimEnd()}"))}

                                            These lines were removed or modified, which may break existing consumers.
                                            """,
                                };
                            }),
                        })
                        .Select(x => new
                        {
                            x.ClientMutationId,
                        })
                        .Compile();

                    var addPullRequestReviewThreadResult = await connection.Run(addPullRequestReviewThread,
                        cancellationToken: cancellationToken);

                    if (addPullRequestReviewThreadResult is null)
                        throw new StepFailedException("Could not add review comment.");
                }
            });
}
