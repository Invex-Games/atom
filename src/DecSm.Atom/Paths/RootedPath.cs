namespace DecSm.Atom.Paths;

/// <summary>
///     Represents a file system path that is rooted within a specific <see cref="IAtomFileSystem" /> instance.
/// </summary>
/// <param name="FileSystem">The file system instance this path belongs to.</param>
/// <param name="Path">The absolute path string.</param>
[PublicAPI]
public record RootedPath(IAtomFileSystem FileSystem, string Path)
{
    /// <summary>
    ///     Gets the parent directory of the current path.
    /// </summary>
    /// <returns>
    ///     A new <see cref="RootedPath" /> representing the parent directory, or <c>null</c> if the current path is a
    ///     root.
    /// </returns>
    public RootedPath? Parent
    {
        get
        {
            if (FileSystem.Path.GetPathRoot(Path) == Path)
                return null;

            var path = Path switch
            {
                [.., '/'] or [.., '\\'] => Path[..^1],
                _ => Path,
            };

            var lastForwardSlash = path.LastIndexOf('/');
            var lastBackSlash = path.LastIndexOf('\\');

            var lastSlash = Math.Max(lastForwardSlash, lastBackSlash);

            if (lastSlash == -1)
                return null;

            return this with
            {
                Path = $"{path[..lastSlash]}{FileSystem.Path.DirectorySeparatorChar}",
            };
        }
    }

    /// <summary>
    ///     Indicates whether the path exists in the file system as either a file or a directory.
    /// </summary>
    public bool PathExists => FileExists || DirectoryExists;

    /// <summary>
    ///     Indicates whether the path exists in the file system as a file.
    /// </summary>
    public bool FileExists => FileSystem.File.Exists(Path);

    /// <summary>
    ///     Indicates whether the path exists in the file system as a directory.
    /// </summary>
    public bool DirectoryExists => FileSystem.Directory.Exists(Path);

    /// <summary>
    ///     Gets the file name from the current path.
    /// </summary>
    /// <returns>The file name including its extension, or <c>null</c> if the path does not represent an existing file.</returns>
    public string? FileName =>
        FileExists
            ? FileSystem.Path.GetFileName(Path)
            : null;

    /// <summary>
    ///     Gets the file name of the current path without its extension.
    /// </summary>
    /// <returns>The file name without its extension.</returns>
    public string FileNameWithoutExtension => FileSystem.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    ///     Gets the directory name of the current path.
    /// </summary>
    /// <returns>The directory name, or <c>null</c> if the path does not represent an existing directory.</returns>
    public string? DirectoryName =>
        DirectoryExists
            ? FileSystem.Path.GetDirectoryName(Path)
            : null;

    /// <summary>
    ///     Combines the current <see cref="RootedPath" /> with a string segment.
    /// </summary>
    /// <param name="left">The base <see cref="RootedPath" />.</param>
    /// <param name="right">The string segment to append.</param>
    /// <returns>A new <see cref="RootedPath" /> representing the combined path.</returns>
    public static RootedPath operator /(RootedPath left, string right) =>
        left with
        {
            Path = left.FileSystem.Path.Combine(left.Path, right),
        };

    /// <summary>
    ///     Implicitly converts a <see cref="RootedPath" /> to its string representation.
    /// </summary>
    /// <param name="path">The <see cref="RootedPath" /> to convert.</param>
    /// <returns>The string representation of the path.</returns>
    public static implicit operator string(RootedPath path) =>
        path.Path;

    /// <inheritdoc />
    public override string ToString() =>
        Path;
}
