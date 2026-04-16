namespace DecSm.Atom.Tests;

/// <summary>
///     Provides the full path to the source directory of the current project.<br />
///     (Only meaningful on the machine where this code was compiled.)<br />
///     From <a href="https://stackoverflow.com/a/66285728/773113" />
/// </summary>
internal static class ProjectSourcePath
{
    private const string RelativePath = nameof(ProjectSourcePath) + ".cs";
    private static string? _lazyValue;

    /// <summary>
    ///     The full path to the source directory of the current project.
    /// </summary>
    public static string Value => _lazyValue ??= Calculate();

    private static string Calculate([CallerFilePath] string? path = null)
    {
        Debug.Assert(path!.EndsWith(RelativePath, StringComparison.Ordinal));

        return path[..^RelativePath.Length];
    }
}
