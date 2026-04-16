namespace DecSm.Atom.Util.Scope;

/// <summary>
///     A disposable scope that executes a provided action upon disposal.
/// </summary>
/// <remarks>
///     This is useful for ensuring cleanup logic runs at the end of a `using` block.
/// </remarks>
/// <param name="OnDispose">The action to execute upon disposal.</param>
[PublicAPI]
public sealed record ActionScope(Action? OnDispose = null) : IDisposable
{
    /// <summary>
    ///     Executes the disposal action if it is not null.
    /// </summary>
    public void Dispose() =>
        OnDispose?.Invoke();
}
