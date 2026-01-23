namespace DecSm.Atom.Paths;

/// <summary>
///     A concrete implementation of <see cref="IPathProvider" /> that uses a delegate to locate paths.
/// </summary>
[PublicAPI]
public sealed class FunctionPathProvider : IPathProvider
{
    /// <summary>
    ///     Gets the function that implements the path location logic.
    /// </summary>
    public required Func<string, RootedPath?> Resolver { get; init; }

    /// <inheritdoc />
    public required int Priority { get; init; }

    /// <inheritdoc />
    public RootedPath? GetPath(string key) =>
        Resolver(key);
}
