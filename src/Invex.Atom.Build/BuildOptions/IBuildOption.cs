namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     Defines the base marker interface for build options.
/// </summary>
/// <remarks>
///     Build options are strongly-typed configuration values that can be applied globally via
///     <see cref="IBuildDefinition.Options" />, contributed by <see cref="IBuildOptionProvider" />
///     implementations, or attached to individual workflow targets. Use
///     <see cref="BuildOptionExtensions" /> to query resolved options.
/// </remarks>
[PublicAPI]
public interface IBuildOption;
