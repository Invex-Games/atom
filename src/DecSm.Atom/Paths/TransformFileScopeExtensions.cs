namespace DecSm.Atom.Paths;

/// <summary>
///     Provides extension methods for chaining asynchronous transformations on <see cref="TransformFileScope" />
///     and <see cref="TransformMultiFileScope" />.
/// </summary>
[PublicAPI]
public static class TransformFileScopeExtensions
{
    /// <summary>
    ///     Applies an additional asynchronous transformation to each file within the scope returned by a task.
    /// </summary>
    /// <param name="scopeTask">A task that returns a <see cref="TransformMultiFileScope" />.</param>
    /// <param name="transform">A function to apply to each file's current content.</param>
    /// <returns>
    ///     A task that represents the completion of the transformation, yielding the same
    ///     <see cref="TransformMultiFileScope" /> instance.
    /// </returns>
    public static async Task<TransformMultiFileScope> AddAsync(
        this Task<TransformMultiFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);

    /// <summary>
    ///     Applies an additional asynchronous transformation to the file within the scope returned by a task.
    /// </summary>
    /// <param name="scopeTask">A task that returns a <see cref="TransformFileScope" />.</param>
    /// <param name="transform">A function to apply to the file's current content.</param>
    /// <returns>
    ///     A task that represents the completion of the transformation, yielding the same
    ///     <see cref="TransformFileScope" /> instance.
    /// </returns>
    public static async Task<TransformFileScope> AddAsync(
        this Task<TransformFileScope> scopeTask,
        Func<string, string> transform) =>
        await (await scopeTask).AddAsync(transform);
}
