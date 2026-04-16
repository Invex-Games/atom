namespace DecSm.Atom.Artifacts;

/// <summary>
///     Provides a shared parameter for specifying artifact names.
/// </summary>
/// <remarks>
///     This interface defines the <see cref="AtomArtifacts" /> parameter, which is used by targets like
///     <see cref="IStoreArtifact" /> and <see cref="IRetrieveArtifact" /> to identify which artifacts to process.
///     The parameter is typically supplied via the <c>--atom-artifacts</c> command-line argument or an environment
///     variable.
/// </remarks>
[PublicAPI]
public interface IAtomArtifactsParam : IBuildAccessor
{
    /// <summary>
    ///     Gets the names of the artifacts to be processed.
    /// </summary>
    /// <remarks>
    ///     This parameter specifies which artifacts to store or retrieve. Multiple names can be provided as a
    ///     comma-separated string, which the framework parses into an array.
    ///     If no value is provided, it defaults to an empty array.
    /// </remarks>
    /// <example>
    ///     If the command-line argument <c>--atom-artifacts "Package,Symbols"</c> is provided, this property will return
    ///     <c>["Package", "Symbols"]</c>.
    /// </example>
    [ParamDefinition("atom-artifacts",
        "The name of the artifact/s to work with, use ',' to separate multiple artifacts")]
    string[] AtomArtifacts => GetParam(() => AtomArtifacts, []);
}
