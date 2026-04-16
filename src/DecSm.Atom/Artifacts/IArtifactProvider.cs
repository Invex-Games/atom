namespace DecSm.Atom.Artifacts;

/// <summary>
///     Defines a provider for storing and retrieving build artifacts.
/// </summary>
/// <remarks>
///     Implementations of this interface abstract the underlying storage mechanism, allowing for different backends
///     such as cloud storage or local file systems. The Atom framework uses these providers to manage artifacts
///     for targets like <see cref="IStoreArtifact" /> and <see cref="IRetrieveArtifact" />.
/// </remarks>
[PublicAPI]
public interface IArtifactProvider
{
    /// <summary>
    ///     Gets a list of parameter names required by the artifact provider to function correctly.
    /// </summary>
    /// <remarks>
    ///     These parameters typically include configuration settings or credentials needed to access the artifact storage.
    /// </remarks>
    IReadOnlyList<string> RequiredParams => [];

    /// <summary>
    ///     Uploads one or more build artifacts to the configured storage.
    /// </summary>
    /// <param name="artifactNames">A list of artifact names to upload. Each name typically corresponds to a file or directory.</param>
    /// <param name="buildId">
    ///     The unique identifier for the current build. If null, the provider may use a default or
    ///     environment-provided ID.
    /// </param>
    /// <param name="buildSlice">
    ///     An optional identifier for a specific build variation (e.g., in a matrix build) to associate
    ///     the artifacts with.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous upload operation.</returns>
    /// <remarks>
    ///     Artifacts are typically sourced from the directory specified by <see cref="IAtomFileSystem.AtomPublishDirectory" />
    ///     .
    ///     This method is called by targets like <see cref="IStoreArtifact.StoreArtifact" />.
    /// </remarks>
    Task StoreArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Downloads one or more build artifacts from the configured storage.
    /// </summary>
    /// <param name="artifactNames">A list of artifact names to download.</param>
    /// <param name="buildId">
    ///     The unique identifier of the build from which to retrieve artifacts. If null, the provider may
    ///     assume the current run or retrieve the latest version.
    /// </param>
    /// <param name="buildSlice">An optional identifier for a specific build variation to retrieve artifacts from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous download operation.</returns>
    /// <remarks>
    ///     Downloaded artifacts are placed in the directory specified by <see cref="IAtomFileSystem.AtomArtifactsDirectory" />
    ///     .
    ///     This method is called by targets like <see cref="IRetrieveArtifact.RetrieveArtifact" />.
    /// </remarks>
    Task RetrieveArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all artifacts associated with the specified build run identifiers.
    /// </summary>
    /// <param name="runIdentifiers">A list of build run identifiers whose artifacts should be deleted.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous cleanup operation.</returns>
    /// <remarks>
    ///     This is useful for managing storage by removing old or temporary artifacts.
    /// </remarks>
    Task Cleanup(IReadOnlyList<string> runIdentifiers, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves a list of all stored run identifiers from the artifact storage.
    /// </summary>
    /// <param name="artifactName">An optional artifact name to filter the run identifiers by.</param>
    /// <param name="buildSlice">An optional build slice to filter the run identifiers by.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that completes with a read-only list of stored run identifiers.
    /// </returns>
    /// <remarks>
    ///     This method can be used to list available builds for retrieval or cleanup.
    /// </remarks>
    Task<IReadOnlyList<string>> GetStoredRunIdentifiers(
        string? artifactName = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default);
}
