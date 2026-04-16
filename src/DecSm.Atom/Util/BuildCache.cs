namespace DecSm.Atom.Util;

/// <summary>
///     Represents a cache scoped to a build, using an <see cref="IBuildAccessor" /> as the key.
/// </summary>
/// <typeparam name="T">The type of the items to be stored in the cache.</typeparam>
[PublicAPI]
public sealed class BuildCache<T>
{
    private readonly Dictionary<IBuildAccessor, T> _cache = [];

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
    [return: NotNullIfNotNull("value")]
    public T Set(IBuildAccessor key, T value) =>
        _cache[key] = value;

    /// <summary>
    ///     Removes all keys and values from the cache.
    /// </summary>
    public void Clear() =>
        _cache.Clear();
}
