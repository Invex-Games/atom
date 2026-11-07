namespace Invex.Atom.Build.Util;

/// <summary>
///     Converts locations on disk to paths relative to the Atom root directory.
/// </summary>
internal static class AtomRootPath
{
    /// <summary>
    ///     Gets a location's path relative to the Atom root directory, using <c>/</c> as the separator.
    /// </summary>
    /// <param name="fileSystem">The file system whose <c>AtomRootDirectory</c> is the root.</param>
    /// <param name="path">The location. <c>.</c> and <c>..</c> segments are resolved first.</param>
    /// <returns>
    ///     The relative path, such as <c>src/AppA/GitVersion.yml</c>, without a trailing <c>/</c>; or
    ///     <see langword="null" /> if the location is the Atom root directory itself or is not below it.
    /// </returns>
    public static string? GetRelativePathBelowRoot(IRootedFileSystem fileSystem, string path) =>
        GetRelativePathAtOrBelowRoot(fileSystem, path) is { } relativePath and not "."
            ? relativePath
            : null;

    /// <summary>
    ///     Gets a location's path relative to the Atom root directory, using <c>/</c> as the separator, allowing the
    ///     Atom root directory itself.
    /// </summary>
    /// <param name="fileSystem">The file system whose <c>AtomRootDirectory</c> is the root.</param>
    /// <param name="path">The location. <c>.</c> and <c>..</c> segments are resolved first.</param>
    /// <returns>
    ///     The relative path, such as <c>src/AppA/GitVersion.yml</c>, without a trailing <c>/</c>; <c>.</c> for the
    ///     Atom root directory itself; or <see langword="null" /> if the location is not at or below it.
    /// </returns>
    public static string? GetRelativePathAtOrBelowRoot(IRootedFileSystem fileSystem, string path)
    {
        var relativePath = fileSystem
            .Path
            .GetRelativePath(fileSystem.AtomRootDirectory, fileSystem.Path.GetFullPath(path))
            .Replace('\\', '/')
            .TrimEnd('/');

        if (relativePath is "" or ".")
            return ".";

        return relativePath is ".." ||
               relativePath.StartsWith("../", StringComparison.Ordinal) ||
               fileSystem.Path.IsPathRooted(relativePath)
            ? null
            : relativePath;
    }
}
