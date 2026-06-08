namespace Invex.Atom.Workflows.Experimental;

[UnstableAPI]
public static partial class WorkflowCacheUtil
{
    [GeneratedRegex(@"[^a-zA-Z0-9_\.]")]
    public static partial Regex SanitizeRegex();

    public static string ConvertNameToId(string name) =>
        SanitizeRegex()
            .Replace(name, "-")
            .ToLowerInvariant();
}

[UnstableAPI]
public sealed record WorkflowCacheSaveOption : IBuildOption
{
    public required string Name { get; init; }

    public required TextExpression Key { get; init; }

    public required TextExpressionCollection Paths { get; init; }

    public TextExpression? RunIf { get; init; }

    public bool RunOnlyIfMatchingNameCacheMissed { get; init; } = true;

    public string StepId => $"cache-save-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

[UnstableAPI]
public sealed record WorkflowCacheRestoreOption : IBuildOption
{
    public required string Name { get; init; }

    public required TextExpression Key { get; init; }

    public required TextExpressionCollection Paths { get; init; }

    public TextExpression? RunIf { get; init; }

    public string StepId => $"cache-restore-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

[UnstableAPI]
public static class WorkflowCacheOptions
{
    [UnstableAPI]
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public sealed class CacheOptions
    {
        internal static CacheOptions Instance => field ??= new();

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
        [UnstableAPI]
        public static CacheOptions Cache => CacheOptions.Instance;
    }
}
