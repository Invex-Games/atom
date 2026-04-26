namespace DecSm.Atom.FileSystem;

/// <summary>
///     Defines a provider that can resolve a <see cref="RootedPath" /> for a given key within the
///     Atom file system.
/// </summary>
/// <remarks>
///     <para>
///         Multiple providers can be registered simultaneously.  <see cref="IAtomFileSystem.GetPath" />
///         queries them in descending <see cref="Priority" /> order and returns the first non-<c>null</c>
///         result.  A provider that does not recognize a key should return <c>null</c> to let the next
///         provider in the chain handle it.
///     </para>
///     <para>
///         Because <see cref="IAtomFileSystem.GetPath" /> caches successful resolutions, a provider's
///         <see cref="GetPath" /> method is called at most once per key per lifetime of the
///         <see cref="IAtomFileSystem" /> instance.
///     </para>
/// </remarks>
[PublicAPI]
public interface IPathProvider
{
    /// <summary>
    ///     Gets the priority of this provider.
    /// </summary>
    /// <remarks>
    ///     Higher values are queried before lower values.  When two providers can both resolve the
    ///     same key the one with the higher priority wins.  Use a negative value to register a
    ///     low-priority fallback that only applies when no other provider matches.
    /// </remarks>
    int Priority { get; }

    /// <summary>
    ///     Attempts to resolve a <see cref="RootedPath" /> for the given <paramref name="key" />.
    /// </summary>
    /// <param name="key">The key identifying the path to locate (e.g. <c>"Root"</c>, <c>"Artifacts"</c>).</param>
    /// <returns>
    ///     A <see cref="RootedPath" /> if this provider recognises <paramref name="key" />;
    ///     otherwise <c>null</c> to pass resolution to the next provider in the chain.
    /// </returns>
    RootedPath? GetPath(string key);
}
