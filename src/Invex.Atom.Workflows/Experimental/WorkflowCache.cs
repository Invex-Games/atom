namespace Invex.Atom.Workflows.Experimental;

/// <summary>
///     Provides utilities for generating workflow cache step identifiers.
/// </summary>
[UnstableAPI]
public static partial class WorkflowCacheUtil
{
    /// <summary>
    ///     Matches characters that are not valid in cache step identifiers.
    /// </summary>
    [GeneratedRegex(@"[^a-zA-Z0-9_\.]")]
    public static partial Regex SanitizeRegex();

    /// <summary>
    ///     Converts a cache name into a lowercase, sanitized identifier suitable for use as a step ID.
    /// </summary>
    /// <param name="name">The cache name to convert.</param>
    /// <returns>The sanitized, lowercase identifier.</returns>
    public static string ConvertNameToId(string name) =>
        SanitizeRegex()
            .Replace(name, "-")
            .ToLowerInvariant();
}

/// <summary>
///     A workflow option that adds a step to save files to the platform's cache after the target runs.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.Cache.Save(...)</c>.
/// </remarks>
[UnstableAPI]
public sealed record WorkflowCacheSaveOption : IBuildOption
{
    /// <summary>
    ///     Gets the name of the cache, used to derive the <see cref="StepId" />.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the expression that produces the cache key.
    /// </summary>
    public required TextExpression Key { get; init; }

    /// <summary>
    ///     Gets the expressions that produce the paths to cache.
    /// </summary>
    public required TextExpressionCollection Paths { get; init; }

    /// <summary>
    ///     Gets an optional condition expression that gates whether the save step runs.
    /// </summary>
    public TextExpression? RunIf { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the save step should only run when the matching restore step
    ///     (by <see cref="Name" />) missed the cache. Defaults to <c>true</c>.
    /// </summary>
    public bool RunOnlyIfMatchingNameCacheMissed { get; init; } = true;

    /// <summary>
    ///     Gets the identifier of the generated cache-save step.
    /// </summary>
    public string StepId => $"cache-save-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

/// <summary>
///     A workflow option that adds a step to restore files from the platform's cache before the target runs.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.Cache.Restore(...)</c>.
/// </remarks>
[UnstableAPI]
public sealed record WorkflowCacheRestoreOption : IBuildOption
{
    /// <summary>
    ///     Gets the name of the cache, used to derive the <see cref="StepId" />.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the expression that produces the cache key.
    /// </summary>
    public required TextExpression Key { get; init; }

    /// <summary>
    ///     Gets the expressions that produce the paths to restore.
    /// </summary>
    public required TextExpressionCollection Paths { get; init; }

    /// <summary>
    ///     Gets an optional condition expression that gates whether the restore step runs.
    /// </summary>
    public TextExpression? RunIf { get; init; }

    /// <summary>
    ///     Gets the identifier of the generated cache-restore step.
    /// </summary>
    public string StepId => $"cache-restore-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

/// <summary>
///     Extends the <see cref="BuildOptions" /> anchor class with fluent factories for workflow cache options.
/// </summary>
[UnstableAPI]
public static class WorkflowCacheOptions
{
    /// <summary>
    ///     Provides factories for creating cache save and restore options.
    /// </summary>
    [UnstableAPI]
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public sealed class CacheOptions
    {
        internal static CacheOptions Instance => field ??= new();

        /// <summary>
        ///     Creates an option that saves the specified paths to the cache after the target runs.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="key">The expression that produces the cache key.</param>
        /// <param name="paths">The expressions that produce the paths to cache. Must contain at least one path.</param>
        /// <param name="runIf">An optional condition expression that gates whether the save step runs.</param>
        /// <param name="runOnlyIfMatchingNameCacheMissed">
        ///     Whether to run the save step only when the matching restore step missed the cache.
        /// </param>
        /// <returns>A <see cref="WorkflowCacheSaveOption" />.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="paths" /> is empty.</exception>
        public WorkflowCacheSaveOption Save(
            string name,
            TextExpression key,
            IEnumerable<TextExpression> paths,
            TextExpression? runIf = null,
            bool runOnlyIfMatchingNameCacheMissed = true) =>
            new()
            {
                Name = name,
                Key = key,
                Paths = paths.ToArray() is { Length: > 0 } pathsArray
                    ? pathsArray
                    : throw new ArgumentException("At least one path must be specified."),
                RunIf = runIf,
                RunOnlyIfMatchingNameCacheMissed = runOnlyIfMatchingNameCacheMissed,
            };

        /// <summary>
        ///     Creates an option that restores the specified paths from the cache before the target runs.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="key">The expression that produces the cache key.</param>
        /// <param name="paths">The expressions that produce the paths to restore. Must contain at least one path.</param>
        /// <param name="runIf">An optional condition expression that gates whether the restore step runs.</param>
        /// <returns>A <see cref="WorkflowCacheRestoreOption" />.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="paths" /> is empty.</exception>
        [UnstableAPI]
        public WorkflowCacheRestoreOption Restore(
            string name,
            TextExpression key,
            IEnumerable<TextExpression> paths,
            TextExpression? runIf = null) =>
            new()
            {
                Name = name,
                Key = key,
                Paths = paths.ToArray() is { Length: > 0 } pathsArray
                    ? pathsArray
                    : throw new ArgumentException("At least one path must be specified."),
                RunIf = runIf,
            };
    }

    extension(BuildOptions)
    {
        /// <summary>
        ///     Gets factories for creating workflow cache save and restore options.
        /// </summary>
        [UnstableAPI]
        public static CacheOptions Cache => CacheOptions.Instance;
    }
}
