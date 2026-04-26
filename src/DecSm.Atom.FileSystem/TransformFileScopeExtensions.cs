namespace DecSm.Atom.FileSystem;

/// <summary>
///     Provides extension methods for fluently chaining additional transformations onto a
///     <see cref="Task{T}" /> that yields a scope returned by one of the static
///     <c>CreateAsync</c> factory methods.
/// </summary>
/// <remarks>
///     These extensions exist so that <c>CreateAsync</c> and one or more <c>AddAsync</c> calls can
///     be written as a single expression without intermediate variables:
///     <code>
/// await using var scope = await TransformFileScope
///     .CreateAsync(file, _ => "v1")
///     .AddAsync(c => c + "-patched");
///     </code>
///     Without them, the caller would need to <c>await</c> <c>CreateAsync</c> into a local and then
///     <c>await AddAsync</c> separately.
/// </remarks>
[PublicAPI]
public static class TransformFileScopeExtensions
{
    /// <summary>
    ///     Applies an additional asynchronous transformation to each file within the
    ///     <see cref="TransformMultiFileScope" /> returned by <paramref name="scopeTask" />.
    /// </summary>
    /// <param name="scopeTask">A task that produces a <see cref="TransformMultiFileScope" />.</param>
    /// <param name="transform">A function to apply to each file's current content.</param>
    /// <returns>
    ///     A task that completes with the same <see cref="TransformMultiFileScope" /> instance after
    ///     the transformation has been applied.
    /// </returns>
    public static async Task<TransformMultiFileScope> AddAsync(
        this Task<TransformMultiFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);

    /// <summary>
    ///     Applies an additional asynchronous transformation to the file within the
    ///     <see cref="TransformFileScope" /> returned by <paramref name="scopeTask" />.
    /// </summary>
    /// <param name="scopeTask">A task that produces a <see cref="TransformFileScope" />.</param>
    /// <param name="transform">A function to apply to the file's current content.</param>
    /// <returns>
    ///     A task that completes with the same <see cref="TransformFileScope" /> instance after the
    ///     transformation has been applied.
    /// </returns>
    public static async Task<TransformFileScope> AddAsync(
        this Task<TransformFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);
}
