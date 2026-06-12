namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     Provides extension members for querying <see cref="IBuildOption" /> instances from option
///     collections and build definitions.
/// </summary>
/// <remarks>
///     Members are exposed as static extensions on the option type itself, e.g.
///     <c>MyOption.Get(options)</c> or <c>MyToggleOption.IsEnabled(definition)</c>.
///     When multiple options of the same type are present, the last occurrence wins.
/// </remarks>
[PublicAPI]
public static class BuildOptionExtensions
{
    extension<T>(T)
        where T : IBuildOption
    {
        /// <summary>
        ///     Gets all options of type <typeparamref name="T" /> from the provided options.
        /// </summary>
        /// <param name="options">The options to search.</param>
        /// <returns>All matching options, in order of appearance.</returns>
        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .ToList();

        /// <summary>
        ///     Gets all options of type <typeparamref name="T" /> from the build definition's
        ///     <see cref="IBuildDefinition.Options" />.
        /// </summary>
        /// <param name="definition">The build definition whose options are searched.</param>
        /// <returns>All matching options, in order of appearance.</returns>
        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(IBuildDefinition definition) =>
            definition
                .Options
                .OfType<T>()
                .ToList();

        /// <summary>
        ///     Gets options of type <typeparamref name="T" />, keeping only the last occurrence per group key.
        /// </summary>
        /// <typeparam name="TKey">The type of the grouping key.</typeparam>
        /// <param name="options">The options to search.</param>
        /// <param name="groupBy">A function that selects the grouping key for each option.</param>
        /// <returns>The last matching option for each distinct key.</returns>
        [PublicAPI]
        public static IReadOnlyList<T> GetOptionsGrouped<TKey>(
            IEnumerable<IBuildOption> options,
            Func<T, TKey> groupBy) =>
            options
                .OfType<T>()
                .GroupBy(groupBy)
                .Select(x => x.Last())
                .ToList();

        /// <summary>
        ///     Gets the last option of type <typeparamref name="T" /> from the provided options.
        /// </summary>
        /// <param name="options">The options to search.</param>
        /// <returns>The last matching option, or <c>null</c> if none is present.</returns>
        [PublicAPI]
        public static T? Get(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault();

        /// <summary>
        ///     Gets the last option of type <typeparamref name="T" /> from the build definition's
        ///     <see cref="IBuildDefinition.Options" />.
        /// </summary>
        /// <param name="definition">The build definition whose options are searched.</param>
        /// <returns>The last matching option, or <c>null</c> if none is present.</returns>
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
        /// <summary>
        ///     Determines whether the last option of type <typeparamref name="T" /> in the provided options
        ///     is present and enabled.
        /// </summary>
        /// <param name="options">The options to search.</param>
        /// <returns><c>true</c> if the option is present and enabled; otherwise, <c>false</c>.</returns>
        [PublicAPI]
        public static bool IsEnabled(IEnumerable<IBuildOption> options) =>
            options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };

        /// <summary>
        ///     Determines whether the last option of type <typeparamref name="T" /> in the build definition's
        ///     <see cref="IBuildDefinition.Options" /> is present and enabled.
        /// </summary>
        /// <param name="definition">The build definition whose options are searched.</param>
        /// <returns><c>true</c> if the option is present and enabled; otherwise, <c>false</c>.</returns>
        [PublicAPI]
        public static bool IsEnabled(IBuildDefinition definition) =>
            definition
                .Options
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };
    }
}
