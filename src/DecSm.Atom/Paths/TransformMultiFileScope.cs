namespace DecSm.Atom.Paths;

/// <summary>
///     Represents a disposable scope for performing temporary transformations on multiple files.
///     Upon disposal, the original content of all files is restored unless restoration is explicitly canceled.
/// </summary>
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
    /// <param name="files">The collection of files to be managed.</param>
    /// <param name="initialContents">The original contents of the files before transformation.</param>
    private TransformMultiFileScope(IEnumerable<RootedPath> files, string?[] initialContents)
    {
        _files = files;
        _initialContents = initialContents;
    }

    /// <summary>
    ///     Asynchronously disposes the scope and restores all managed files to their original content, unless canceled.
    /// </summary>
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
    ///     Disposes the scope and restores all managed files to their original content, unless canceled.
    /// </summary>
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
    ///     Creates a new <see cref="TransformMultiFileScope" /> for a collection of files and applies an asynchronous
    ///     transformation to each.
    /// </summary>
    /// <param name="files">The files to transform.</param>
    /// <param name="transform">A function to apply to each file's content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new <see cref="TransformMultiFileScope" /> instance managing the transformed files.</returns>
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
        }

        return scope;
    }

    /// <summary>
    ///     Applies an additional asynchronous transformation to each file within the current scope.
    /// </summary>
    /// <param name="transform">A function to apply to each file's current content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The current <see cref="TransformMultiFileScope" /> instance.</returns>
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
    ///     Creates a new <see cref="TransformMultiFileScope" /> for a collection of files and applies a synchronous
    ///     transformation to each.
    /// </summary>
    /// <param name="files">The files to transform.</param>
    /// <param name="transform">A function to apply to each file's content.</param>
    /// <returns>A new <see cref="TransformMultiFileScope" /> instance managing the transformed files.</returns>
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
    ///     Applies an additional synchronous transformation to each file within the current scope.
    /// </summary>
    /// <param name="transform">A function to apply to each file's current content.</param>
    /// <returns>The current <see cref="TransformMultiFileScope" /> instance.</returns>
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
    ///     Prevents the files from being restored to their original content upon disposal.
    /// </summary>
    public void CancelRestore() =>
        _cancelled = true;
}
