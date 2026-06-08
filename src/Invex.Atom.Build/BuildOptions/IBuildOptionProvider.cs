namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     Defines a contract for contributing build options to the resolved option set.
/// </summary>
/// <remarks>
///     Implementations receive the base options from <see cref="IBuildDefinition.Options" /> and may
///     return additional options to be merged in. Uses two-phase resolution: providers only receive the base
///     options, not other providers' contributions, preventing inter-provider dependencies.
///     Register implementations with DI as <see cref="IBuildOptionProvider" /> to have them automatically
///     included.
/// </remarks>
[PublicAPI]
public interface IBuildOptionProvider
{
    /// <summary>
    ///     Gets additional build options based on the current base options from <see cref="IBuildDefinition" />.
    /// </summary>
    /// <param name="baseOptions">The base options from <see cref="IBuildDefinition.Options" />.</param>
    /// <returns>Additional build options to merge into the resolved set.</returns>
    IReadOnlyList<IBuildOption> GetBuildOptions(IReadOnlyList<IBuildOption> baseOptions);
}
