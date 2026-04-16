namespace DecSm.Atom.Artifacts;

/// <summary>
///     Defines a target for storing build artifacts using an <see cref="IArtifactProvider" />.
/// </summary>
/// <remarks>
///     This interface, when implemented by a build definition, provides the <see cref="StoreArtifact" /> target.
///     It uses the configured <see cref="IArtifactProvider" /> to upload artifacts.
///     Artifacts to be stored are specified via the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter
///     and are expected to be located in the <see cref="IAtomFileSystem.AtomPublishDirectory" />.
///     Build information from <see cref="ISetupBuildInfo" /> (e.g., <c>BuildName</c>, <c>BuildId</c>) is used
///     to categorize or tag the artifacts.
///     This target is hidden by default, primarily for internal use or custom artifact workflows.
/// </remarks>
[PublicAPI]
public interface IStoreArtifact : IAtomArtifactsParam, ISetupBuildInfo
{
    /// <summary>
    ///     Gets the configured <see cref="IArtifactProvider" /> instance.
    /// </summary>
    private IArtifactProvider ArtifactProvider => GetService<IArtifactProvider>();

    /// <summary>
    ///     Defines the target responsible for storing artifacts.
    /// </summary>
    /// <remarks>
    ///     This target orchestrates the upload of artifacts by invoking the <see cref="IArtifactProvider.StoreArtifacts" />
    ///     method.
    ///     It consumes the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter to determine which artifacts to store
    ///     and utilizes build metadata from <see cref="ISetupBuildInfo" />.
    /// </remarks>
    Target StoreArtifact =>
        t => t
            .IsHidden()
            .DescribedAs("Stores artifacts.")
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .RequiresParam(nameof(AtomArtifacts))
            .RequiresParam(ArtifactProvider.RequiredParams.ToArray())
            .Executes(async cancellationToken =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    ArtifactProvider.GetType()
                        .Name);

                await ArtifactProvider.StoreArtifacts(AtomArtifacts, cancellationToken: cancellationToken);
            });
}
