namespace Invex.Atom.Build.FileSystem;

[PublicAPI]
internal sealed class AtomPathProvider(IServiceProvider services) : IPathProvider
{
    private IRootedFileSystem RootedFileSystem => field ??= services.GetRequiredService<IRootedFileSystem>();

    public int Priority => 0;

    public RootedPath? GetPath(string key) =>
        key switch
        {
            AtomPaths.Root => GetRoot(),
            AtomPaths.Artifacts or AtomPaths.Publish => RootedFileSystem.GetPath(AtomPaths.Root) / "atom-publish",
            AtomPaths.Temp => new(RootedFileSystem, RootedFileSystem.Path.GetTempPath()),
            _ => null,
        };

    /// <summary>
    ///     Determines the root directory by traversing up from the current directory and looking for project markers.
    /// </summary>
    private RootedPath GetRoot()
    {
        var currentDir = RootedFileSystem.CurrentDirectory;

        while (currentDir.Parent is not null)
        {
            currentDir = currentDir.Parent;

            if (RootedFileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, "*.git", SearchOption.TopDirectoryOnly)
                    .Any() ||
                RootedFileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.slnx", SearchOption.TopDirectoryOnly)
                    .Any() ||
                RootedFileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly)
                    .Any())
                return currentDir;
        }

        return RootedFileSystem.CurrentDirectory;
    }
}
