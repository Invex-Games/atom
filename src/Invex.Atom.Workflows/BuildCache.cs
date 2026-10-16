using System.Collections.Concurrent;

namespace Invex.Atom.Workflows;

/// <summary>
///     Represents a cache scoped to a build, using an <see cref="IBuildAccessor" /> as the key.
/// </summary>
/// <typeparam name="T">The type of the items to be stored in the cache.</typeparam>
/// <remarks>
///     <para>
///         Keys are compared by their default equality, which for build definitions and helpers is reference
///         equality. The cache is thread-safe and holds its entries for the lifetime of the cache instance, so
///         typically it is stored in a <c>static readonly</c> field.
///     </para>
///     <para>
///         Typical use is to compute an expensive per-build value once, e.g. via <see cref="GetOrAdd" />.
///     </para>
/// </remarks>
[PublicAPI]
public sealed class BuildCache<T>
{
    private readonly ConcurrentDictionary<IBuildAccessor, T> _cache = [];

    /// <summary>
    ///     Attempts to get the value associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="result">
    ///     When this method returns, contains the object from the cache that has the specified key,
    ///     or the default value of the type if the operation failed.
    /// </param>
    /// <returns><c>true</c> if the key was found in the cache; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(IBuildAccessor key, [MaybeNullWhen(false)] out T result) =>
        _cache.TryGetValue(key, out result);

    /// <summary>
    ///     Adds or updates a value in the cache with the provided key.
    /// </summary>
    /// <param name="key">The key of the value to set.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <returns>The value that was set.</returns>
    [return: NotNullIfNotNull(nameof(value))]
    public T Set(IBuildAccessor key, T value) =>
        _cache[key] = value;

    /// <summary>
    ///     Gets the value associated with the specified key, or adds one created by <paramref name="valueFactory" />
    ///     if the key is not present.
    /// </summary>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="valueFactory">The function used to create the value when the key is not present.</param>
    /// <returns>The existing or newly created value.</returns>
    /// <remarks>
    ///     Under concurrent access <paramref name="valueFactory" /> may be invoked more than once for the same key,
    ///     but only one result is stored and returned to all callers.
    /// </remarks>
    public T GetOrAdd(IBuildAccessor key, Func<IBuildAccessor, T> valueFactory) =>
        _cache.GetOrAdd(key, valueFactory);

    /// <summary>
    ///     Removes the value associated with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns><c>true</c> if the key was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(IBuildAccessor key) =>
        _cache.TryRemove(key, out _);

    /// <summary>
    ///     Removes all keys and values from the cache.
    /// </summary>
    public void Clear() =>
        _cache.Clear();
}
