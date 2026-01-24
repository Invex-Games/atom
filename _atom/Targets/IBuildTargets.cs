using Environment = System.Environment;

namespace Atom.Targets;

internal interface IBuildTargets : IDotnetPackHelper, IDotnetPublishHelper
{
    static readonly string[] ProjectsToPack =
    [
        Projects.DecSm_Atom.Name,
        Projects.DecSm_Atom_Module_AzureKeyVault.Name,
        Projects.DecSm_Atom_Module_AzureStorage.Name,
        Projects.DecSm_Atom_Module_DevopsWorkflows.Name,
        Projects.DecSm_Atom_Module_Dotnet.Name,
        Projects.DecSm_Atom_Module_GitVersion.Name,
        Projects.DecSm_Atom_Module_GithubWorkflows.Name,
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
            .ProducesArtifact(Projects.DecSm_Atom_Tool.Name)
            .Executes(async cancellationToken =>
            {
                var runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;

                Logger.LogInformation("Packing AOT Atom tool for runtime {RuntimeIdentifier}", runtimeIdentifier);

                await DotnetPackAndStage(FileSystem.GetPath<Projects.DecSm_Atom_Tool>(),
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

                    await DotnetPackAndStage(FileSystem.GetPath<Projects.DecSm_Atom_Tool>(),
                        new()
                        {
                            ClearPublishDirectory = false,
                        },
                        cancellationToken);
                }
            });

    Target BuildAtom =>
        t => t.Executes(async cancellationToken =>
        {
            await DotnetPublishAndStage(Projects._atom.Name, cancellationToken: cancellationToken);

            var atomExecutableName = Environment.OSVersion.Platform is PlatformID.Win32NT
                ? "_atom.exe"
                : "_atom";

            var atomExecutablePath = FileSystem.AtomPublishDirectory / Projects._atom.Name / atomExecutableName;

            if (!atomExecutablePath.FileExists)
            {
                Logger.LogError("Atom executable not found at {Path}", atomExecutablePath);

                return;
            }

            var cachedDirectory = FileSystem.AtomRootDirectory / ".atom";

            if (cachedDirectory.DirectoryExists)
                FileSystem.Directory.Delete(cachedDirectory, true);

            FileSystem.Directory.CreateDirectory(cachedDirectory);

            FileSystem.File.Move(atomExecutablePath, cachedDirectory / atomExecutableName);
        });
}
