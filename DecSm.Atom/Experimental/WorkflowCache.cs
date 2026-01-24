namespace DecSm.Atom.Experimental;

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
public sealed record WorkflowCacheSaveOption : IWorkflowOption
{
    public required string Name { get; init; }

    public required WorkflowExpression Key { get; init; }

    public required IReadOnlyList<WorkflowExpression> Paths { get; init; }

    public WorkflowExpression? RunIf { get; init; }

    public bool RunOnlyIfMatchingNameCacheMissed { get; init; } = true;

    public string StepId => $"cache-save-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

[UnstableAPI]
public sealed record WorkflowCacheRestoreOption : IWorkflowOption
{
    public required string Name { get; init; }

    public required WorkflowExpression Key { get; init; }

    public required IReadOnlyList<WorkflowExpression> Paths { get; init; }

    public WorkflowExpression? RunIf { get; init; }

    public string StepId => $"cache-restore-{WorkflowCacheUtil.ConvertNameToId(Name)}";
}

[UnstableAPI]
public static class WorkflowCacheOptions
{
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public sealed class Options
    {
        internal static Options Instance => field ??= new();

        public WorkflowCacheSaveOption Save(
            string name,
            WorkflowExpression key,
            IEnumerable<WorkflowExpression> paths,
            WorkflowExpression? runIf = null,
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

        public WorkflowCacheRestoreOption Restore(
            string name,
            WorkflowExpression key,
            IEnumerable<WorkflowExpression> paths,
            WorkflowExpression? runIf = null) =>
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

    extension(WorkflowOptions)
    {
        public static Options Cache => Options.Instance;
    }
}
