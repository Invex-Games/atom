namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IGithubReleaseHelper, ISetupBuildInfo
{
    [ParamDefinition("nuget-push-feed", "The Nuget feed to push to.")]
    string NugetFeed => GetParam(() => NugetFeed, "https://api.nuget.org/v3/index.json");

    [SecretDefinition("nuget-push-api-key", "The API key to use to push to Nuget.")]
    string NugetApiKey => GetParam(() => NugetApiKey)!;

    Target PushToNuget =>
        t => t
            .DescribedAs("Pushes the packages to Nuget")
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), IBuildTargets.ProjectsToPack)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.Invex_Atom_Tool.Name, PlatformNames)
            .DependsOn(nameof(ITestTargets.TestProjects))
            .Executes(async cancellationToken =>
            {
                // Push project packages
                foreach (var project in IBuildTargets.ProjectsToPack)
                    await PushProject(project, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);

                // Push Atom tool package - platform-specific + multi-targeted
                foreach (var atomToolPackagePath in RootedFileSystem.Directory.GetFiles(
                             RootedFileSystem.AtomArtifactsDirectory / Projects.Invex_Atom_Tool.Name,
                             "*.nupkg",
                             SearchOption.AllDirectories))
                    await PushPackageToNuget(
                        RootedFileSystem.AtomArtifactsDirectory / Projects.Invex_Atom_Tool.Name / atomToolPackagePath,
                        NugetFeed,
                        NugetApiKey,
                        cancellationToken: cancellationToken);
            });

    Target PushToNugetDevops =>
        d => d
            .DescribedAs("Pushes the packages to Nuget")
            .RequiresParam(nameof(NugetFeed))
            .RequiresParam(nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), IBuildTargets.ProjectsToPack)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.Invex_Atom_Tool.Name, DevopsPlatformNames)
            .DependsOn(nameof(ITestTargets.TestProjects))
            .Executes(() => Logger.LogInformation("Simulating push to Nuget feed"));

    Target PushToRelease =>
        d => d
            .DescribedAs("Pushes the packages to the release feed.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), IBuildTargets.ProjectsToPack)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.Invex_Atom_Tool.Name, PlatformNames)
            .ConsumesArtifacts(nameof(ITestTargets.TestProjects),
                ITestTargets.ProjectsToTest,
                PlatformNames.SelectMany(platform => FrameworkNames.Select(framework => $"{platform}-{framework}")))
            .Executes(async () =>
            {
                foreach (var artifact in IBuildTargets.ProjectsToPack.Concat(ITestTargets.ProjectsToTest))
                    await UploadArtifactToRelease(artifact, $"v{BuildVersion}");
            });

    Target CreateGithubRelease =>
        d => d
            .DescribedAs("Creates a release on GitHub.")
            .RequiresParam(nameof(GithubToken))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .Executes(async () => await CreateRelease(
                $"v{BuildVersion.Major}.{BuildVersion.Minor}.{BuildVersion.Patch}",
                "main",
                $"v{BuildVersion.Major}.{BuildVersion.Minor}.{BuildVersion.Patch}"));
}
