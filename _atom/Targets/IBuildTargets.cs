namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper, IDotnetPublishHelper
{
    static readonly string[] ProjectsToPack =
    [
        Projects.Invex_Atom_Build.Name,
        Projects.Invex_Atom_Workflows.Name,
        Projects.Invex_Atom_Module_AzureKeyVault.Name,
        Projects.Invex_Atom_Module_AzureStorage.Name,
        Projects.Invex_Atom_Module_DevopsWorkflows.Name,
        Projects.Invex_Atom_Module_Dotnet.Name,
        Projects.Invex_Atom_Module_GitVersion.Name,
        Projects.Invex_Atom_Module_GithubWorkflows.Name,
    ];

    Target PackProjects =>
        t => t
            .DescribedAs("Packs the Atom projects (excluding the tool) into nuget packages")
            .ProducesArtifacts(ProjectsToPack)
            .Executes(async cancellationToken =>
            {
                foreach (var project in ProjectsToPack)
                    await DotnetPackAndStage(project, cancellationToken: cancellationToken);
            });

    Target PackTool =>
        t => t
            .DescribedAs("Packs the Atom tool into a nuget package")
            .ProducesArtifact(Projects.Invex_Atom_Tool.Name)
            .Executes(async cancellationToken =>
            {
                var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;

                Logger.LogInformation("Packing AOT Atom tool for runtime {RuntimeIdentifier}", runtimeIdentifier);

                await DotnetPackAndStage(RootedFileSystem.GetPath<Projects.Invex_Atom_Tool>(),
                    new()
                    {
                        PackOptions = new()
                        {
                            Runtime = runtimeIdentifier,
                            Property = new Dictionary<string, string>
                            {
                                { "PublishAot", "true" },
                            },
                        },
                    },
                    cancellationToken);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Logger.LogInformation("Packing Atom tool for non-native AOT");

                    await DotnetPackAndStage(RootedFileSystem.GetPath<Projects.Invex_Atom_Tool>(),
                        new()
                        {
                            ClearPublishDirectory = false,
                        },
                        cancellationToken);
                }
            });
}
