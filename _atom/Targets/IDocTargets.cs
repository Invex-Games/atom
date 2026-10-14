namespace Atom.Targets;

internal interface IDocTargets : IDocFxHelper
{
    Target BuildDocs =>
        t => t
            .DescribedAs("Builds the DocFX documentation.")
            .ProducesArtifact(GeneratedDocsArtifactName)
            .Executes(cancellationToken => BuildDocFxDocs([
                    Projects.Invex_Atom_Build.Path(RootedFileSystem),
                    Projects.Invex_Atom_Workflows.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_AzureKeyVault.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_AzureStorage.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_DevOpsWorkflows.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_Dotnet.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_GitHubWorkflows.Path(RootedFileSystem),
                    Projects.Invex_Atom_Module_GitVersion.Path(RootedFileSystem),
                ],
                cancellationToken));

    Target ServeDocs =>
        t => t
            .DescribedAs("Serves the DocFX documentation.")
            .DependsOn(nameof(BuildDocs))
            .Executes(ServeDocFxDocs);

    Target PublishDocs =>
        t => t
            .DescribedAs("Publishes the DocFX documentation to GitHub Pages.")
            .RequiresParam(nameof(GitHubToken))
            .ConsumesArtifact(nameof(BuildDocs), GeneratedDocsArtifactName)
            .DependsOn(nameof(SetupBuildInfo))
            .Executes(cancellationToken =>
                PublishDocFxDocsToGithub(GitHubToken, GeneratedDocsArtifactName, cancellationToken));
}
