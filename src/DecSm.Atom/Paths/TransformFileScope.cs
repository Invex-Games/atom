namespace DecSm.Atom.Paths;

/// <summary>
///     Represents a disposable scope for performing temporary transformations on a file's content.
///     Upon disposal, the file's original content is restored unless restoration is explicitly canceled.
/// </summary>
/// <remarks>
///     This is useful for temporarily modifying project files during a build without permanent changes.
/// </remarks>
/// <example>
///     <code>
/// using (var scope = await TransformFileScope.CreateAsync(myFile, content => content + "\n&lt;NewProperty&gt;Value&lt;/NewProperty&gt;"))
/// {
///     // The file is now modified. It will be restored when the scope is disposed.
/// }
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
    /// <param name="file">The file to be managed.</param>
    /// <param name="initialContent">The original content of the file before transformation.</param>
    private TransformFileScope(RootedPath file, string? initialContent)
    {
        _file = file;
        _initialContent = initialContent;
    }

    /// <summary>
    ///     Asynchronously disposes the scope and restores the file to its original content, unless canceled.
    /// </summary>
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
    ///     Disposes the scope and restores the file to its original content, unless canceled.
    /// </summary>
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
    ///     Creates a new <see cref="TransformFileScope" /> for a file and applies an asynchronous transformation.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="transform">A function to apply to the file's content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new <see cref="TransformFileScope" /> instance managing the transformed file.</returns>
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
    ///     Applies an additional asynchronous transformation to the file within the current scope.
    /// </summary>
    /// <param name="transform">A function to apply to the file's current content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The current <see cref="TransformFileScope" /> instance.</returns>
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
    ///     Creates a new <see cref="TransformFileScope" /> for a file and applies a synchronous transformation.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="transform">A function to apply to the file's content.</param>
    /// <returns>A new <see cref="TransformFileScope" /> instance managing the transformed file.</returns>
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
    ///     Applies an additional synchronous transformation to the file within the current scope.
    /// </summary>
    /// <param name="transform">A function to apply to the file's current content.</param>
    /// <returns>The current <see cref="TransformFileScope" /> instance.</returns>
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
    ///     Prevents the file from being restored to its original content upon disposal.
    /// </summary>
    public void CancelRestore() =>
        _cancelled = true;
}
