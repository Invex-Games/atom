namespace DecSm.Atom.Build.BuildOptions;

[PublicAPI]
public static class BuildOptionExtensions
{
    extension<T>(T)
        where T : IBuildOption
    {
        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .ToList();

        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(IBuildDefinition definition) =>
            definition
                .Options
                .OfType<T>()
                .ToList();

        [PublicAPI]
        public static IReadOnlyList<T> GetOptionsGrouped<TKey>(
            IEnumerable<IBuildOption> options,
            Func<T, TKey> groupBy) =>
            options
                .OfType<T>()
                .GroupBy(groupBy)
                .Select(x => x.Last())
                .ToList();

        [PublicAPI]
        public static T? Get(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault();

        [PublicAPI]
        public static T? Get(IBuildDefinition definition) =>
            definition
                .Options
                .OfType<T>()
                .LastOrDefault();
    }

    extension<T>(T)
        where T : ToggleBuildOption
    {
        [PublicAPI]
        public static bool IsEnabled(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };

        [PublicAPI]
        public static bool IsEnabled(IBuildDefinition definition) =>
            definition
                .Options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };
    }
}
