namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Defines a provider for determining the semantic version of the build.
/// </summary>
/// <remarks>
///     Implementations of this interface should consistently return the build version
///     in accordance with Semantic Versioning (SemVer) standards.
/// </remarks>
[PublicAPI]
public interface IBuildVersionProvider
{
    /// <summary>
    ///     Gets the semantic version of the current build.
    /// </summary>
    SemVer Version { get; }
}
