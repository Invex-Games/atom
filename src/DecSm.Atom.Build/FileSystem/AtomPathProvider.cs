namespace DecSm.Atom.Build.FileSystem;

[PublicAPI]
internal sealed class AtomPathProvider(IServiceProvider services) : IPathProvider
{
    private IAtomFileSystem AtomFileSystem => field ??= services.GetRequiredService<IAtomFileSystem>();

    public int Priority => 0;

    public RootedPath? GetPath(string key) =>
        key switch
        {
            AtomPaths.Root => GetRoot(),
            AtomPaths.Artifacts or AtomPaths.Publish => AtomFileSystem.GetPath(AtomPaths.Root) / "atom-publish",
            AtomPaths.Temp => new(AtomFileSystem, AtomFileSystem.Path.GetTempPath()),
            _ => null,
        };

    /// <summary>
    ///     Determines the root directory by traversing up from the current directory and looking for project markers.
    /// </summary>
    private RootedPath GetRoot()
    {
        var currentDir = AtomFileSystem.CurrentDirectory;

        while (currentDir.Parent is not null)
        {
            currentDir = currentDir.Parent;

            if (AtomFileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, "*.git", SearchOption.TopDirectoryOnly)
                    .Any() ||
                AtomFileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.slnx", SearchOption.TopDirectoryOnly)
                    .Any() ||
                AtomFileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly)
                    .Any())
                return currentDir;
        }

        return AtomFileSystem.CurrentDirectory;
    }
}
