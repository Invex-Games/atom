namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper, IDotnetPublishHelper
{
    static readonly string[] ProjectsToPack =
    [
        Projects.Invex_Atom_Build.Name,
        Projects.Invex_Atom_Workflows.Name,
        Projects.Invex_Atom_Module_AzureKeyVault.Name,
        Projects.Invex_Atom_Module_AzureStorage.Name,
        Projects.Invex_Atom_Module_DevOpsWorkflows.Name,
        Projects.Invex_Atom_Module_Dotnet.Name,
        Projects.Invex_Atom_Module_GitVersion.Name,
        Projects.Invex_Atom_Module_GitHubWorkflows.Name,
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

                // `dotnet pack` can write several packages for a RID-specific tool (the .NET 11 SDK also writes the
                // current runtime's package when packing without a runtime), so name the package to stage each time.
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
                        PackageFileName = $"{Projects.Invex_Atom_Tool.Name}.{runtimeIdentifier}.{BuildVersion}.nupkg",
                    },
                    cancellationToken);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Logger.LogInformation("Packing Atom tool top-level package");

                    // The top-level Invex.Atom.Tool package, which points `dotnet tool install` at the RID packages.
                    await DotnetPackAndStage(RootedFileSystem.GetPath<Projects.Invex_Atom_Tool>(),
                        new()
                        {
                            ClearPublishDirectory = false,
                            PackageFileName = $"{Projects.Invex_Atom_Tool.Name}.{BuildVersion}.nupkg",
                        },
                        cancellationToken);
                }
            });
}
