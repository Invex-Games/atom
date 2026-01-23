namespace DecSm.Atom.Util.Scope;

/// <summary>
///     Represents a disposable scope that performs no action upon disposal.
/// </summary>
/// <remarks>
///     This is useful in scenarios where a disposable object is required but no cleanup is necessary.
/// </remarks>
[PublicAPI]
public readonly struct NullScope : IDisposable
{
    /// <summary>
    ///     Gets a singleton instance of the <see cref="NullScope" />.
    /// </summary>
    public static readonly NullScope Instance = new();

    /// <summary>
    ///     Performs no action.
    /// </summary>
    public void Dispose() { }
}
