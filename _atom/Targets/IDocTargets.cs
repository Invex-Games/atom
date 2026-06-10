namespace Atom.Targets;

internal interface IDocTargets : IDocFxHelper
{
    Target BuildDocs =>
        t => t
            .DescribedAs("Builds the DocFX documentation.")
            .ProducesArtifact(GeneratedDocsArtifactName)
            .Executes(cancellationToken => BuildDocFxDocs([
                    Projects.Invex_Atom_Build_Analyzers.Path(RootedFileSystem),
                    Projects.Invex_Atom_Build_SourceGenerators.Path(RootedFileSystem),
                ],
                cancellationToken));

    Target ServeDocs =>
        t => t
            .DescribedAs("Serves the DocFX documentation.")
            .DependsOn(nameof(BuildDocs))
            .Executes(ServeDocFxDocs);

    Target PublishDocs =>
        t => t
            .DescribedAs("Publishes the DocFX documentation to Github Pages.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesArtifact(nameof(BuildDocs), GeneratedDocsArtifactName)
            .DependsOn(nameof(SetupBuildInfo))
            .Executes(cancellationToken =>
                PublishDocFxDocsToGithub(GithubToken, GeneratedDocsArtifactName, cancellationToken));
}
