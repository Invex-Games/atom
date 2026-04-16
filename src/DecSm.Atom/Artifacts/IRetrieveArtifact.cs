namespace DecSm.Atom.Artifacts;

/// <summary>
///     Defines a target for retrieving build artifacts using an <see cref="IArtifactProvider" />.
/// </summary>
/// <remarks>
///     This interface, when implemented by a build definition, provides the <see cref="RetrieveArtifact" /> target.
///     It uses the configured <see cref="IArtifactProvider" /> to download artifacts.
///     Artifacts to be retrieved are specified via the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter.
///     Build information from <see cref="ISetupBuildInfo" /> (e.g., <c>BuildName</c>, <c>BuildId</c>) is used
///     to identify the correct artifacts to download.
///     This target is hidden by default, primarily for internal use or custom artifact workflows.
/// </remarks>
[PublicAPI]
public interface IRetrieveArtifact : IAtomArtifactsParam, ISetupBuildInfo
{
    /// <summary>
    ///     Gets the configured <see cref="IArtifactProvider" /> instance.
    /// </summary>
    private IArtifactProvider ArtifactProvider => GetService<IArtifactProvider>();

    /// <summary>
    ///     Defines the target responsible for retrieving artifacts.
    /// </summary>
    /// <remarks>
    ///     This target orchestrates the download of artifacts by invoking the
    ///     <see cref="IArtifactProvider.RetrieveArtifacts" />
    ///     method.
    ///     It consumes the <see cref="IAtomArtifactsParam.AtomArtifacts" /> parameter to determine which artifacts to retrieve
    ///     and utilizes build metadata from <see cref="ISetupBuildInfo" />.
    /// </remarks>
    Target RetrieveArtifact =>
        t => t
            .IsHidden()
            .DescribedAs("Retrieves artifacts.")
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .RequiresParam(nameof(AtomArtifacts))
            .RequiresParam(ArtifactProvider.RequiredParams.ToArray())
            .Executes(async cancellationToken =>
            {
                Logger.LogInformation("Using artifact provider: {Provider}",
                    ArtifactProvider.GetType()
                        .Name);

                await ArtifactProvider.RetrieveArtifacts(AtomArtifacts, cancellationToken: cancellationToken);
            });
}
