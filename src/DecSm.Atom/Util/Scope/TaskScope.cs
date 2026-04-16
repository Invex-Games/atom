namespace DecSm.Atom.Util.Scope;

/// <summary>
///     A disposable scope that asynchronously executes a provided function upon disposal.
/// </summary>
/// <remarks>
///     This is useful for ensuring cleanup logic runs at the end of a `using` block, especially for asynchronous
///     operations.
/// </remarks>
/// <param name="OnDispose">The asynchronous function to execute upon disposal.</param>
[PublicAPI]
public sealed record TaskScope(Func<Task>? OnDispose = null) : IAsyncDisposable
{
    /// <summary>
    ///     Asynchronously executes the disposal function if it is not null.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (OnDispose is not null)
            await OnDispose()
                .ConfigureAwait(false);
    }
}
