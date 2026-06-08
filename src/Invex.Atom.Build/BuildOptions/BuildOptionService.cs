namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     Provides access to the fully resolved set of build options, merging
///     <see cref="IBuildDefinition.Options" /> with contributions from all registered
///     <see cref="IBuildOptionProvider" /> instances.
/// </summary>
[PublicAPI]
public interface IBuildOptionService
{
    /// <summary>
    ///     Gets the merged set of build options from <see cref="IBuildDefinition.Options" />
    ///     and all registered <see cref="IBuildOptionProvider" /> instances.
    /// </summary>
    IReadOnlyList<IBuildOption> Options { get; }
}

internal sealed class BuildOptionService(IBuildDefinition buildDefinition, IEnumerable<IBuildOptionProvider> providers)
    : IBuildOptionService
{
    private readonly IReadOnlyList<IBuildOptionProvider> _providers = providers.ToList();

    public IReadOnlyList<IBuildOption> Options =>
        field ??= buildDefinition
            .Options
            .Concat(_providers.SelectMany(p => p.GetBuildOptions(buildDefinition.Options)))
            .ToList();
}
