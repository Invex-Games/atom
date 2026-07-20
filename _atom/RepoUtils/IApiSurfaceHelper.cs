namespace Atom.RepoUtils;

internal sealed record BreakingChanges(IReadOnlyList<Change> MajorChanges, IReadOnlyList<Change> MinorChanges);

internal sealed record Change(RootedPath Path, List<string> AddedLines, List<string> DeletedLines);

internal interface IApiSurfaceHelper : IBuildAccessor
{
    async Task<BreakingChanges> IdentifyBreakingChanges(
        SemVer oldVersion,
        string oldCommitHash,
        SemVer newVersion,
        string newCommitHash,
        CancellationToken cancellationToken,
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
        var arguments = new List<string>
        {
            "-c",
            "core.quotePath=false",
            "diff",
            "--unified=0",
            "--no-color",
            "--no-ext-diff",
            oldCommitHash,
            newCommitHash,
            "--",
        };
        arguments.AddRange(targetFiles);

        var result = await ProcessRunner.RunAsync(new("git", [.. arguments])
            {
                WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                OutputLogLevel = LogLevel.Debug,
            },
            cancellationToken);
        var changes = ParseChanges(result.Output, targetFiles);

        Logger.LogDebug("Changes: {@Changes}",
            new
            {
                Content = result.Output,
                LinesDeleted = changes.Sum(x => x.DeletedLines.Count),
                LinesAdded = changes.Sum(x => x.AddedLines.Count),
            });

        if (changes.Count is 0)
            return new([], []);

        IReadOnlyList<Change> suspiciousChanges = changes
            .Where(x => x.DeletedLines.Count > 0)
            .ToList();

        Logger.LogDebug("Suspicious changes: {@SuspiciousChanges}", suspiciousChanges);

        var majorChanges = suspiciousChanges
            .Where(x => x.DeletedLines.Count > 0 &&
                        x
                            .DeletedLines
                            .Select(line => line.Trim())
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

    private List<Change> ParseChanges(string patch, HashSet<string> targetFiles)
    {
        var changes = new List<Change>();
        Change? currentChange = null;
        string? oldPath = null;

        using var reader = new StringReader(patch);

        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith("diff --git ", StringComparison.Ordinal))
            {
                currentChange = null;
                oldPath = null;
            }
            else if (line.StartsWith("--- ", StringComparison.Ordinal))
            {
                oldPath = ParseDiffPath(line[4..]);
            }
            else if (line.StartsWith("+++ ", StringComparison.Ordinal))
            {
                var newPath = ParseDiffPath(line[4..]);
                var path = newPath ?? oldPath;

                if (path is not null && targetFiles.Contains(path))
                {
                    currentChange = new(RootedFileSystem.AtomRootDirectory / path, [], []);
                    changes.Add(currentChange);
                }
            }
            else if (currentChange is not null && line.StartsWith('+'))
            {
                currentChange.AddedLines.Add(line[1..]);
            }
            else if (currentChange is not null && line.StartsWith('-'))
            {
                currentChange.DeletedLines.Add(line[1..]);
            }
        }

        return changes;
    }

    private static string? ParseDiffPath(string value)
    {
        if (value is "/dev/null")
            return null;

        return value.StartsWith("a/", StringComparison.Ordinal) ||
               value.StartsWith("b/", StringComparison.Ordinal)
            ? value[2..]
            : value;
    }

    private HashSet<string> FormatTargetFiles(RootedPath[] filesToCheck)
    {
        var targetFiles = filesToCheck
            .Select(x => RootedFileSystem.Path.IsPathRooted(x)
                ? RootedFileSystem.Path.GetRelativePath(RootedFileSystem.AtomRootDirectory, x)
                : x)
            .Select(x => x.Replace("\\", "/"))
            .Select(x => x.StartsWith('/')
                ? x[1..]
                : x)
            .ToHashSet();

        return targetFiles;
    }
}
