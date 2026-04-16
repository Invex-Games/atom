namespace DecSm.Atom.Tool;

public static class FileFinder
{
    private const int MaxDownstreamDepth = 4;

    /// <summary>
    ///     Directories to ignore during the downward Breadth-First Search.
    /// </summary>
    private static readonly HashSet<string> ExcludedFolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- Version Control ---
        ".git",
        ".github",
        ".devops",
        ".hg",
        ".svn",

        // --- IDEs & Editors ---
        ".vs",
        ".vscode",
        ".idea",

        // --- Build Artifacts & Output ---
        // Note - 'build' excluded as it might be a valid search
        "bin",
        "obj",
        "dist",
        "out",
        "target",
        "publish",

        // --- Dependency Managers ---
        "node_modules",
        "bower_components",
        "packages",

        // --- Languages & Runtimes ---

        // Python
        ".venv",
        "venv",
        "env",
        "__pycache__",

        // Java
        ".gradle",

        // Infrastructure
        ".m2",
        ".terraform",

        // --- System & Cache ---
        ".cache",
        ".sass-cache",
        "System Volume Information",
        "$RECYCLE.BIN",
    };

    /// <summary>
    ///     Markers that signal the "Root" of a project.
    ///     The upward search will stop here.
    /// </summary>
    private static readonly HashSet<string> RootMarkers = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".sln",
        "package.json",
        "go.mod",
        "Cargo.toml",
    };

    public static IFileInfo? FindFile(
        IFileSystem fileSystem,
        string startDirectory,
        string[] fileNames,
        bool checkParentSubfolderMatches)
    {
        // if it's already a file, return it
        if (fileSystem.File.Exists(startDirectory))
            return fileSystem.FileInfo.New(startDirectory);

        var initialDir = fileSystem.DirectoryInfo.New(startDirectory);

        // Search up
        var currentUp = initialDir;

        while (currentUp is { Exists: true })
        {
            var found = CheckDirectoryForFiles(fileSystem, currentUp.FullName, fileNames, checkParentSubfolderMatches);

            if (found != null)
                return found;

            // Stop condition: Don't climb higher than a project root marker
            if (RootMarkers.Any(marker =>
                    fileSystem.File.Exists(fileSystem.Path.Combine(currentUp.FullName, marker)) ||
                    fileSystem.Directory.Exists(fileSystem.Path.Combine(currentUp.FullName, marker))))
                break;

            currentUp = currentUp.Parent;
        }

        // Search down (Breadth-First Search)
        var queue = new Queue<(string Path, int Depth)>();

        try
        {
            foreach (var subDir in fileSystem.Directory.EnumerateDirectories(initialDir.FullName))
                if (!ExcludedFolderNames.Contains(fileSystem.Path.GetFileName(subDir)))
                    queue.Enqueue((subDir, 1));
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore directories we don't have access to
        }

        while (queue.Count > 0)
        {
            var (currentPath, depth) = queue.Dequeue();

            var found = CheckDirectoryForFiles(fileSystem, currentPath, fileNames, false);

            if (found != null)
                return found;

            if (depth >= MaxDownstreamDepth)
                continue;

            try
            {
                foreach (var subDir in fileSystem.Directory.EnumerateDirectories(currentPath))
                    if (!ExcludedFolderNames.Contains(fileSystem.Path.GetFileName(subDir)))
                        queue.Enqueue((subDir, depth + 1));
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore directories we don't have access to
            }
        }

        return null;
    }

    private static IFileInfo? CheckDirectoryForFiles(
        IFileSystem fileSystem,
        string directory,
        IEnumerable<string> names,
        bool lookInMatchingDirectoryNameInParentDirectories)
    {
        foreach (var name in names)
        {
            var directPath = fileSystem.Path.Combine(directory, name);

            if (fileSystem.File.Exists(directPath))
                return fileSystem.FileInfo.New(directPath);

            if (!lookInMatchingDirectoryNameInParentDirectories)
                continue;

            var nameWithoutExt = fileSystem.Path.GetFileNameWithoutExtension(name);

            if (string.IsNullOrEmpty(nameWithoutExt))
                continue;

            var nestedPath = fileSystem.Path.Combine(directory, nameWithoutExt, name);

            if (fileSystem.File.Exists(nestedPath))
                return fileSystem.FileInfo.New(nestedPath);
        }

        return null;
    }
}
