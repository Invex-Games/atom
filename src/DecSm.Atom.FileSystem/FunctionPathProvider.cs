namespace DecSm.Atom.FileSystem;

/// <summary>
///     A lightweight, delegate-based implementation of <see cref="IPathProvider" /> that resolves
///     paths using a caller-supplied function rather than requiring a dedicated class.
/// </summary>
/// <remarks>
///     Use <see cref="FunctionPathProvider" /> (or the <c>ProvidePath</c> extension in
///     <see cref="FileSystemHostExtensions" />) when path logic is simple enough to express inline
///     rather than in a separate class.  For complex resolution logic — such as searching the file
///     system or combining multiple keys — a dedicated <see cref="IPathProvider" /> implementation
///     is clearer.
/// </remarks>
[PublicAPI]
public sealed class FunctionPathProvider : IPathProvider
{
    /// <summary>
    ///     Gets the delegate that implements the path resolution logic.
    /// </summary>
    /// <remarks>
    ///     The delegate receives the key string and should return a <see cref="RootedPath" /> when it
    ///     recognizes the key, or <c>null</c> to defer resolution to the next provider in the chain.
    /// </remarks>
    public required Func<string, RootedPath?> Provider { get; init; }

    /// <inheritdoc />
    public required int Priority { get; init; }

    /// <inheritdoc />
    public RootedPath? GetPath(string key) =>
        Provider(key);
}
