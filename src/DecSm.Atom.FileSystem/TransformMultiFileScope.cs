namespace DecSm.Atom.FileSystem;

/// <summary>
///     Provides disposable scopes for performing temporary, reversible transformations on
///     <em>multiple</em> files simultaneously.
///     Upon disposal, the original content of every managed file is restored unless restoration
///     is explicitly cancelled.
/// </summary>
/// <remarks>
///     <para>
///         The behaviour mirrors <see cref="TransformFileScope" /> but operates on a collection of
///         files in parallel (via <c>Task.WhenAll</c>).  Each file's original content is captured
///         independently: files that did not exist before the scope was created are deleted on
///         disposal, while existing files have their content written back.
///     </para>
///     <para>
///         This is useful when a build step needs to simultaneously patch a set of related files —
///         for example injecting a shared version number into multiple project files — and must
///         guarantee that all files are restored even if the step fails partway through.
///     </para>
///     <para>
///         Call <see cref="CancelRestore" /> before disposing to commit all transformations
///         permanently instead of rolling them back.
///     </para>
/// </remarks>
[PublicAPI]
public sealed class TransformMultiFileScope : IAsyncDisposable, IDisposable
{
    private readonly IEnumerable<RootedPath> _files;
    private readonly string?[] _initialContents;
    private bool _cancelled;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransformMultiFileScope" /> class.
    /// </summary>
    /// <param name="files">The collection of files being managed.</param>
    /// <param name="initialContents">
    ///     The original content of each file before transformation, in the same order as
    ///     <paramref name="files" />.  A <c>null</c> entry indicates the corresponding file did not
    ///     exist and was created by this scope; it will be deleted on disposal.
    /// </param>
    private TransformMultiFileScope(IEnumerable<RootedPath> files, string?[] initialContents)
    {
        _files = files;
        _initialContents = initialContents;
    }

    /// <summary>
    ///     Asynchronously disposes the scope and restores all managed files to their original state.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>If <see cref="CancelRestore" /> was called, all files are left as-is.</item>
    ///         <item>Files that did not exist when the scope was created are deleted.</item>
    ///         <item>All other files have their original content written back in parallel.</item>
    ///         <item>Calling this method more than once is safe — subsequent calls are no-ops.</item>
    ///     </list>
    ///     No cancellation token is accepted: restoring files should always complete to avoid
    ///     leaving the repository in a partially-modified state.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        await Task.WhenAll(_files.Select(async (x, i) =>
        {
            if (_initialContents[i] is null)
                x.FileSystem.File.Delete(x);
            else
                await x.FileSystem.File.WriteAllTextAsync(x, _initialContents[i]);
        }));
    }

    /// <summary>
    ///     Synchronously disposes the scope and restores all managed files to their original state.
    /// </summary>
    /// <remarks>
    ///     <inheritdoc cref="DisposeAsync" />
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        // No cancellation token here - we'd prefer to wait for the files to write so they don't get mangled.
        foreach (var (file, i) in _files.Select((x, i) => (x, i)))
            if (_initialContents[i] is null)
                file.FileSystem.File.Delete(file);
            else
                file.FileSystem.File.WriteAllText(file, _initialContents[i]);
    }

    /// <summary>
    ///     Creates a new <see cref="TransformMultiFileScope" />, applying an asynchronous
    ///     transformation to each file in <paramref name="files" />.
    /// </summary>
    /// <param name="files">The files to transform.</param>
    /// <param name="transform">
    ///     A function applied to each file's current content (or an empty string for files that do
    ///     not yet exist).  The same function is called for every file; if you need per-file logic
    ///     consider multiple <see cref="TransformFileScope" /> instances instead.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A <see cref="TransformMultiFileScope" /> that will restore all files when disposed.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown (and all files restored) if <paramref name="cancellationToken" /> is cancelled
    ///     while the transformed content is being written.
    /// </exception>
    public static async Task<TransformMultiFileScope> CreateAsync(
        IEnumerable<RootedPath> files,
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        var filesArray = files.ToArray();

        var initialContents = await Task.WhenAll(filesArray.Select(async x =>
        {
            if (x.FileSystem.File.Exists(x))
                return await x.FileSystem.File.ReadAllTextAsync(x, cancellationToken);

            await x
                .FileSystem
                .File
                .Create(x)
                .DisposeAsync();

            return null;
        }));

        var scope = new TransformMultiFileScope(filesArray, initialContents);

        try
        {
            await Task.WhenAll(filesArray.Select((x, i) =>
                x.FileSystem.File.WriteAllTextAsync(x,
                    transform(initialContents[i] ?? string.Empty),
                    cancellationToken)));
        }
        catch (OperationCanceledException)
        {
            await scope.DisposeAsync();

            throw;
        }

        return scope;
    }

    /// <summary>
    ///     Applies an additional asynchronous transformation to every file within an already-open
    ///     scope.
    /// </summary>
    /// <param name="transform">
    ///     A function that receives each file's <em>current</em> (already-transformed) content and
    ///     returns the next content to write.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The same <see cref="TransformMultiFileScope" /> instance, for fluent chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the scope has already been disposed.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown (and all files restored) if <paramref name="cancellationToken" /> is cancelled
    ///     during the write.
    /// </exception>
    /// <remarks>
    ///     The scope always restores to the <em>original</em> contents captured at creation time,
    ///     not the state after the most recent <c>Add</c> call.
    /// </remarks>
    public async Task<TransformMultiFileScope> AddAsync(
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        try
        {
            await Task.WhenAll(_files.Select(async x =>
            {
                var currentContent = await x.FileSystem.File.ReadAllTextAsync(x, cancellationToken);
                await x.FileSystem.File.WriteAllTextAsync(x, transform(currentContent), cancellationToken);
            }));

            return this;
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();

            throw;
        }
    }

    /// <summary>
    ///     Creates a new <see cref="TransformMultiFileScope" />, applying a synchronous
    ///     transformation to each file in <paramref name="files" />.
    /// </summary>
    /// <param name="files">The files to transform.</param>
    /// <param name="transform">
    ///     A function applied to each file's current content (or an empty string for files that do
    ///     not yet exist).
    /// </param>
    /// <returns>
    ///     A <see cref="TransformMultiFileScope" /> that will restore all files when disposed.
    /// </returns>
    public static TransformMultiFileScope Create(IEnumerable<RootedPath> files, Func<string, string> transform)
    {
        var filesArray = files.ToArray();

        var initialContents = filesArray
            .Select(x =>
            {
                if (x.FileSystem.File.Exists(x))
                    return x.FileSystem.File.ReadAllText(x);

                x
                    .FileSystem
                    .File
                    .Create(x)
                    .Dispose();

                return null;
            })
            .ToArray();

        var scope = new TransformMultiFileScope(filesArray, initialContents);

        foreach (var (file, i) in filesArray.Select((x, i) => (x, i)))
            file.FileSystem.File.WriteAllText(file, transform(initialContents[i] ?? string.Empty));

        return scope;
    }

    /// <summary>
    ///     Applies an additional synchronous transformation to every file within an already-open
    ///     scope.
    /// </summary>
    /// <param name="transform">
    ///     A function that receives each file's <em>current</em> (already-transformed) content and
    ///     returns the next content to write.
    /// </param>
    /// <returns>The same <see cref="TransformMultiFileScope" /> instance, for fluent chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the scope has already been disposed.</exception>
    /// <remarks>
    ///     <inheritdoc cref="AddAsync(Func{string,string},CancellationToken)" select="remarks" />
    /// </remarks>
    public TransformMultiFileScope Add(Func<string, string> transform)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        foreach (var file in _files)
        {
            var currentContent = file.FileSystem.File.ReadAllText(file);
            file.FileSystem.File.WriteAllText(file, transform(currentContent));
        }

        return this;
    }

    /// <summary>
    ///     Prevents all files from being restored to their original content when the scope is
    ///     disposed.
    /// </summary>
    /// <remarks>
    ///     <inheritdoc cref="TransformFileScope.CancelRestore" />
    /// </remarks>
    public void CancelRestore() =>
        _cancelled = true;
}
