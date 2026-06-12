namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     The base class for build options that represent an on/off toggle.
/// </summary>
/// <remarks>
///     Use the <c>IsEnabled</c> extension in <see cref="BuildOptionExtensions" /> to check whether a
///     toggle option is present and enabled in a set of options. When the same option type appears
///     multiple times, the last occurrence wins.
/// </remarks>
[PublicAPI]
public abstract record ToggleBuildOption : IBuildOption
{
    /// <summary>
    ///     Gets a value indicating whether the option is enabled. Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
