namespace DecSm.Atom.Paths;

/// <summary>
///     Defines a provider for locating paths within the Atom file system.
/// </summary>
[PublicAPI]
public interface IPathProvider
{
    /// <summary>
    ///     Gets the priority of this provider. Providers with higher priority values are queried first.
    /// </summary>
    int Priority { get; }

    /// <summary>
    ///     Attempts to locate a path based on a given key.
    /// </summary>
    /// <param name="key">The key identifying the path to locate (e.g., "Root", "Artifacts").</param>
    /// <returns>A <see cref="RootedPath" /> if the provider can resolve the key; otherwise, <c>null</c>.</returns>
    RootedPath? GetPath(string key);
}
