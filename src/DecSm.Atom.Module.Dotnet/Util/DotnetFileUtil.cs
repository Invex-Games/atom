namespace DecSm.Atom.Module.Dotnet.Util;

[PublicAPI]
public static class DotnetFileUtil
{
    /// <summary>
    ///     Finds the Directory.Build.props files that apply to the given project file.
    /// </summary>
    /// <param name="projectFilePath">The full path to the project file.</param>
    /// <param name="rootPath">The root path for the operation. Directories above this path will not be searched.</param>
    /// <returns>The project file as well as any Directory.Build.props files that apply to it.</returns>
    public static IEnumerable<RootedPath> GetPropertyFilesForProject(RootedPath projectFilePath, RootedPath rootPath)
    {
        List<RootedPath> filesToTransform = [projectFilePath];

        var dir = projectFilePath;

        do
        {
            dir = dir.Parent;

            if (dir is null)
                break;

            var file = dir / "Directory.Build.props";

            if (file.FileExists)
                filesToTransform.Add(file);
        } while (dir != rootPath);

        return filesToTransform;
    }

    /// <summary>
    ///     Gets the full path to a project file by its name.
    /// </summary>
    /// <param name="fileSystem">The file system to use.</param>
    /// <param name="projectName">The name of the project.</param>
    /// <returns>The full path to the project file.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the project file does not exist.</exception>
    /// <remarks>
    ///     Searches for a file matching the pattern {projectName}.*proj in all subdirectories of the root directory.
    ///     If multiple files match, the one with the shortest path ancestry (fewest directory levels) is returned.
    /// </remarks>
    public static RootedPath? GetProjectFilePathByName(IAtomFileSystem fileSystem, string projectName)
    {
        var foundProjectPath = fileSystem
            .Directory
            .GetFiles(fileSystem.AtomRootDirectory, $"{projectName}.csproj", SearchOption.AllDirectories)
            .OrderBy(p => p.Split(Path.DirectorySeparatorChar)
                .Length)
            .FirstOrDefault();

        return foundProjectPath is null
            ? null
            : fileSystem.CreateRootedPath(foundProjectPath);
    }
}
