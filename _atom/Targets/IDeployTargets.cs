namespace Atom.Targets;

internal interface IDeployTargets : INugetHelper, IGithubReleaseHelper, ISetupBuildInfo
{
    private static IReadOnlyList<string> ReleaseArtifactNames =>
        ReleaseAssetManifest.CreateArtifactNames(IBuildTargets.ProjectsToPack,
            Projects.Invex_Atom_Tool.Name,
            ITestTargets.ProjectsToTest);

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
            .Executes(PushAllPackagesToNuget);

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

    Target DeployRelease =>
        d => d
            .DescribedAs("Publishes a validated release from a protected main-branch workflow.")
            .RequiresParam(nameof(GithubToken), nameof(NugetFeed), nameof(NugetApiKey))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildVersion))
            .ConsumesArtifacts(nameof(IBuildTargets.PackProjects), IBuildTargets.ProjectsToPack)
            .ConsumesArtifact(nameof(IBuildTargets.PackTool), Projects.Invex_Atom_Tool.Name, PlatformNames)
            .ConsumesArtifacts(nameof(ITestTargets.TestProjects),
                ITestTargets.ProjectsToTest,
                PlatformNames.SelectMany(platform => FrameworkNames.Select(framework => $"{platform}-{framework}")))
            .Executes(async cancellationToken =>
            {
                ValidateTrustedReleaseContext();
                ValidateLocalReleaseArtifacts();

                var tag = $"v{BuildVersion.Major}.{BuildVersion.Minor}.{BuildVersion.Patch}";
                var draft = await CreateRelease(tag, Github.Variables.Sha, tag, draft: true);

                if (draft is null)
                {
                    Logger.LogInformation("Release deployment dry run completed for {Tag}.", tag);

                    return;
                }

                var nugetPublicationStarted = false;

                try
                {
                    nugetPublicationStarted = true;
                    await PushAllPackagesToNuget(cancellationToken);

                    foreach (var artifactName in ReleaseArtifactNames)
                        await UploadArtifactToRelease(artifactName, draft);

                    var expectedAssets = ReleaseAssetManifest.CreateExpectedAssetNames(ReleaseArtifactNames);
                    var uploadedAssets = await GetReleaseAssetNames(draft.Id, cancellationToken: cancellationToken);
                    ReleaseAssetManifest.ValidateUploadedAssets(expectedAssets, uploadedAssets);

                    await PublishRelease(draft.Id, cancellationToken: cancellationToken);
                }
                catch (Exception deploymentException)
                {
                    try
                    {
                        await DeleteRelease(draft.Id, cancellationToken: CancellationToken.None);
                    }
                    catch (Exception rollbackException)
                    {
                        throw new StepFailedException(
                            "Release deployment and draft cleanup both failed. Inspect the draft release and " +
                            "deprecate any NuGet packages already published before issuing a new patch version.",
                            new AggregateException(deploymentException, rollbackException));
                    }

                    if (deploymentException is OperationCanceledException)
                    {
                        if (nugetPublicationStarted)
                            Logger.LogError(
                                "Release deployment was cancelled after NuGet publication started. Inspect the " +
                                "feed and deprecate any packages already published before issuing a new patch version.");

                        throw;
                    }

                    throw new StepFailedException(
                        "Release deployment failed and its draft was deleted. If any NuGet packages were already " +
                        "published, deprecate that version and issue a new patch; never overwrite it.",
                        deploymentException);
                }
            });

    private async Task PushAllPackagesToNuget(CancellationToken cancellationToken)
    {
        foreach (var project in IBuildTargets.ProjectsToPack)
            await PushProject(project, NugetFeed, NugetApiKey, cancellationToken: cancellationToken);

        foreach (var atomToolPackagePath in RootedFileSystem.Directory.GetFiles(
                     RootedFileSystem.AtomArtifactsDirectory / Projects.Invex_Atom_Tool.Name,
                     "*.nupkg",
                     SearchOption.AllDirectories))
            await PushPackageToNuget(RootedFileSystem.CreateRootedPath(atomToolPackagePath),
                NugetFeed,
                NugetApiKey,
                cancellationToken: cancellationToken);
    }

    private void ValidateTrustedReleaseContext()
    {
        if (!Github.IsGithubActions)
        {
            if (!NugetDryRun)
                throw new StepFailedException("Release deployment outside GitHub Actions requires --nuget-dry-run.");

            return;
        }

        if (!string.Equals(Github.Variables.EventName, "workflow_dispatch", StringComparison.Ordinal) ||
            !string.Equals(Github.Variables.Ref, "refs/heads/main", StringComparison.Ordinal))
            throw new StepFailedException("Production releases may only be manually dispatched from refs/heads/main.");
    }

    private void ValidateLocalReleaseArtifacts()
    {
        foreach (var artifactName in ReleaseArtifactNames)
        {
            var artifactPath = RootedFileSystem.AtomArtifactsDirectory / artifactName;

            if (!artifactPath.DirectoryExists)
                throw new StepFailedException($"Required release artifact '{artifactName}' is missing.");

            if (RootedFileSystem.Directory.GetFiles(artifactPath, "*", SearchOption.AllDirectories)
                    .Length is 0)
                throw new StepFailedException($"Required release artifact '{artifactName}' is empty.");
        }

        var toolPackageCount = RootedFileSystem.Directory.GetFiles(
                RootedFileSystem.AtomArtifactsDirectory / Projects.Invex_Atom_Tool.Name,
                "*.nupkg",
                SearchOption.AllDirectories)
            .Length;

        if (toolPackageCount < PlatformNames.Length)
            throw new StepFailedException(
                $"The tool release artifact contains {toolPackageCount} packages; expected at least " +
                $"{PlatformNames.Length}, one for each platform.");
    }
}
