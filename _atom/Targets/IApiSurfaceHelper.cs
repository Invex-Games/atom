using DecSm.Atom.Build;
using LibGit2Sharp;
using Commit = LibGit2Sharp.Commit;
using Repository = LibGit2Sharp.Repository;

namespace Atom.Targets;

public sealed record BreakingChanges(IReadOnlyList<Change> MajorChanges, IReadOnlyList<Change> MinorChanges);

public sealed record Change(RootedPath Path, List<Line> AddedLines, List<Line> DeletedLines);

public interface IApiSurfaceHelper : IBuildAccessor
{
    BreakingChanges IdentifyBreakingChanges(
        SemVer oldVersion,
        string oldCommitHash,
        SemVer newVersion,
        string newCommitHash,
        params RootedPath[] filesToCheck)
    {
        var filesToCheckDisplay = string.Join(", ", filesToCheck);

        Logger.LogDebug("Identifying breaking changes with options: {@Options}",
            new
            {
                oldVersion,
                oldCommitHash,
                newVersion,
                newCommitHash,
                filesToCheck = filesToCheckDisplay,
            });

        var targetFiles = FormatTargetFiles(filesToCheck);

        using var repo = new Repository(FileSystem.AtomRootDirectory);
        var oldCommit = repo.Lookup<Commit>(oldCommitHash);

        if (oldCommit?.IsMissing is not false)
            throw new InvalidOperationException($"Commit {oldCommitHash} is missing.");

        var newCommit = repo.Lookup<Commit>(newCommitHash);

        if (newCommit?.IsMissing is not false)
            throw new InvalidOperationException($"Commit {newCommitHash} is missing.");

        var changes = repo.Diff.Compare<Patch>(oldCommit.Tree, newCommit.Tree);

        Logger.LogDebug("Changes: {@Changes}",
            new
            {
                changes.Content,
                changes.LinesDeleted,
                changes.LinesAdded,
            });

        if (changes is null or { LinesAdded: 0, LinesDeleted: 0 })
            return new([], []);

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

        return new(majorChanges, minorChanges);
    }

    private HashSet<string> FormatTargetFiles(RootedPath[] filesToCheck)
    {
        var targetFiles = filesToCheck
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
