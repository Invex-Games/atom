namespace DecSm.Atom.FileSystem;

/// <summary>
///     Provides disposable scopes for performing temporary, reversible transformations on a single
///     file's content.
/// </summary>
/// <remarks>
///     <para>
///         On creation the file's current content is captured.  The supplied transform is applied and
///         written back immediately.  On disposal the original content is restored, or — if the file
///         did not exist before the scope was opened — the file is deleted.
///     </para>
///     <para>
///         This is useful during an operation when a file (e.g. a project file or a config file) needs to
///         be temporarily modified for a specific step without permanently altering it.
///     </para>
///     <para>
///         Call <see cref="CancelRestore" /> before disposing to <em>commit</em> the transformation
///         instead of rolling it back.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// await using var scope = await TransformFileScope.CreateAsync(
///     myProjectFile,
///     content => content.Replace("1.0.0", "2.0.0"));
/// // File now contains "2.0.0". Original is restored when scope is disposed.
///     </code>
/// </example>
[PublicAPI]
public sealed class TransformFileScope : IAsyncDisposable, IDisposable
{
    private readonly RootedPath _file;
    private readonly string? _initialContent;
    private bool _cancelled;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransformFileScope" /> class.
    /// </summary>
    /// <param name="file">The file being managed.</param>
    /// <param name="initialContent">
    ///     The original content of the file before transformation, or <c>null</c> if the file did not
    ///     exist and was created by this scope.
    /// </param>
    private TransformFileScope(RootedPath file, string? initialContent)
    {
        _file = file;
        _initialContent = initialContent;
    }

    /// <summary>
    ///     Asynchronously disposes the scope and restores the file to its original state.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>If <see cref="CancelRestore" /> was called, the file is left as-is.</item>
    ///         <item>If the file did not exist when the scope was created, it is deleted.</item>
    ///         <item>Otherwise the original content is written back.</item>
    ///         <item>Calling this method more than once is safe — subsequent calls are no-ops.</item>
    ///     </list>
    ///     No cancellation token is accepted here: we prefer to wait for the write to complete so
    ///     the file is not left in a partially-written state.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        // No cancellation token here - we'd prefer to wait for the file to write so it doesn't get mangled.
        if (_initialContent is null)
            _file.FileSystem.File.Delete(_file);
        else
            await _file.FileSystem.File.WriteAllTextAsync(_file, _initialContent);
    }

    /// <summary>
    ///     Synchronously disposes the scope and restores the file to its original state.
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

        if (_initialContent is null)
            _file.FileSystem.File.Delete(_file);
        else
            _file.FileSystem.File.WriteAllText(_file, _initialContent);
    }

    /// <summary>
    ///     Creates a new <see cref="TransformFileScope" />, applying an asynchronous transformation
    ///     to the target file.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="transform">
    ///     A function that receives the file's current content (or an empty string if the file does
    ///     not yet exist) and returns the new content to write.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A <see cref="TransformFileScope" /> that will restore the file when disposed.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    ///     Thrown (and the file restored) if <paramref name="cancellationToken" /> is cancelled while
    ///     the transformed content is being written.
    /// </exception>
    public static async Task<TransformFileScope> CreateAsync(
        RootedPath file,
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        string? initialContent = null;

        if (!file.FileSystem.File.Exists(file))
            await file
                .FileSystem
                .File
                .Create(file)
                .DisposeAsync();
        else
            initialContent = await file.FileSystem.File.ReadAllTextAsync(file, cancellationToken);

        var scope = new TransformFileScope(file, initialContent);

        try
        {
            await file.FileSystem.File.WriteAllTextAsync(file,
                transform(initialContent ?? string.Empty),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await scope.DisposeAsync();

            throw;
        }

        return scope;
    }

    /// <summary>
    ///     Applies an additional transformation to the file within an already-open scope,
    ///     using asynchronous I/O.
    /// </summary>
    /// <param name="transform">
    ///     A function that receives the file's <em>current</em> (already-transformed) content and
    ///     returns the next content to write.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The same <see cref="TransformFileScope" /> instance, for fluent chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the scope has already been disposed.</exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown (and the file restored) if <paramref name="cancellationToken" /> is cancelled during
    ///     the write.
    /// </exception>
    /// <remarks>
    ///     The scope still restores to the <em>original</em> content (captured at creation time),
    ///     not the content at the point of the last <c>Add</c> call.  Multiple calls build up
    ///     transformations; disposal always unwinds them all at once.
    /// </remarks>
    public async Task<TransformFileScope> AddAsync(
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        try
        {
            var currentContent = await _file.FileSystem.File.ReadAllTextAsync(_file, cancellationToken);
            await _file.FileSystem.File.WriteAllTextAsync(_file, transform(currentContent), cancellationToken);

            return this;
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();

            throw;
        }
    }

    /// <summary>
    ///     Creates a new <see cref="TransformFileScope" />, applying a synchronous transformation
    ///     to the target file.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="transform">
    ///     A function that receives the file's current content (or an empty string if the file does
    ///     not yet exist) and returns the new content to write.
    /// </param>
    /// <returns>
    ///     A <see cref="TransformFileScope" /> that will restore the file when disposed.
    /// </returns>
    public static TransformFileScope Create(RootedPath file, Func<string, string> transform)
    {
        string? initialContent = null;

        if (!file.FileSystem.File.Exists(file))
            file
                .FileSystem
                .File
                .Create(file)
                .Dispose();
        else
            initialContent = file.FileSystem.File.ReadAllText(file);

        var scope = new TransformFileScope(file, initialContent);

        file.FileSystem.File.WriteAllText(file, transform(initialContent ?? string.Empty));

        return scope;
    }

    /// <summary>
    ///     Applies an additional transformation to the file within an already-open scope,
    ///     using synchronous I/O.
    /// </summary>
    /// <param name="transform">
    ///     A function that receives the file's <em>current</em> (already-transformed) content and
    ///     returns the next content to write.
    /// </param>
    /// <returns>The same <see cref="TransformFileScope" /> instance, for fluent chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the scope has already been disposed.</exception>
    /// <remarks>
    ///     <inheritdoc cref="AddAsync(Func{string,string},CancellationToken)" select="remarks" />
    /// </remarks>
    public TransformFileScope Add(Func<string, string> transform)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        var currentContent = _file.FileSystem.File.ReadAllText(_file);
        _file.FileSystem.File.WriteAllText(_file, transform(currentContent));

        return this;
    }

    /// <summary>
    ///     Prevents the file from being restored to its original content when the scope is disposed.
    /// </summary>
    /// <remarks>
    ///     Call this when the transformation should be made permanent — for example after a build
    ///     step has successfully consumed the transformed file and the change no longer needs to be
    ///     rolled back.  Once called it cannot be undone.
    /// </remarks>
    public void CancelRestore() =>
        _cancelled = true;
}
