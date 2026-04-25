namespace DecSm.Atom.Build.BuildOptions;

[PublicAPI]
public interface IBuildOption;

[PublicAPI]
public interface IToggleBuildOption : IBuildOption
{
    bool Enabled { get; }
}

[PublicAPI]
public abstract record ToggleBuildOption : IToggleBuildOption
{
    public bool Enabled { get; init; } = true;
}

[PublicAPI]
public static class BuildOptions;

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
        public static IReadOnlyList<T> GetOptionsGrouped<TKey>(
            IEnumerable<IBuildOption> options,
            Func<T, TKey> groupBy) =>
            options
                .OfType<T>()
                .GroupBy(groupBy)
                .Select(x => x.Last())
                .ToList();
    }

    extension<T>(T)
        where T : IToggleBuildOption
    {
        [PublicAPI]
        public static bool IsEnabled(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };
    }
}
