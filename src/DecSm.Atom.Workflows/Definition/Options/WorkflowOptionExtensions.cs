namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public static class WorkflowOptionExtensions
{
    extension<T>(T)
        where T : IWorkflowOption
    {
        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(IEnumerable<IWorkflowOption> options) =>
            options
                .OfType<T>()
                .ToList();

        [PublicAPI]
        public static IReadOnlyList<T> GetOptionsGrouped<TKey>(
            IEnumerable<IWorkflowOption> options,
            Func<T, TKey> groupBy) =>
            options
                .OfType<T>()
                .GroupBy(groupBy)
                .Select(x => x.Last())
                .ToList();
    }

    extension<T>(T)
        where T : IToggleWorkflowOption
    {
        [PublicAPI]
        public static bool IsEnabled(IEnumerable<IWorkflowOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };
    }
}
