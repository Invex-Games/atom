namespace DecSm.Atom.Logging;

/// <summary>
///     An internal logger provider that creates instances of <see cref="SpectreLogger" />.
/// </summary>
internal sealed class SpectreLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SpectreLogger> _loggers = new();
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    /// <summary>
    ///     Creates a new <see cref="SpectreLogger" /> instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new <see cref="SpectreLogger" /> instance.</returns>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, new SpectreLogger(categoryName, _scopeProvider));

    /// <summary>
    ///     Disposes the provider and clears all cached logger instances.
    /// </summary>
    public void Dispose() =>
        _loggers.Clear();
}
