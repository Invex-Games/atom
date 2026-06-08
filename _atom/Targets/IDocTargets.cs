using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Atom.Targets;

internal interface IDocTargets : IDotnetCliHelper, IGithubHelper, ISetupBuildInfo
{
    private const string GeneratedDocsArtifactName = "GeneratedDocs";

    Target BuildDocs =>
        t => t
            .DescribedAs("Generates API documentation using DocFX")
            .ProducesArtifact(GeneratedDocsArtifactName)
            .Executes(async cancellationToken =>
            {
                // First, build analyzers in release mode
                await DotnetCli.Build([RootedFileSystem.GetPath<Projects.Invex_Atom_Build_Analyzers>()],
                    new()
                    {
                        Configuration = "Release",
                    },
                    cancellationToken: cancellationToken);

                await DotnetCli.Build([RootedFileSystem.GetPath<Projects.Invex_Atom_Build_SourceGenerators>()],
                    new()
                    {
                        Configuration = "Release",
                    },
                    cancellationToken: cancellationToken);

                var siteDirectory = RootedFileSystem.AtomRootDirectory / "_site";

                // If .NET 10, we can just do dotnet tool exec docfx
                if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 10"))
                {
                    await ProcessRunner.RunAsync(new("dotnet", "tool exec -y docfx")
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);
                }
                else
                {
                    // Otherwise, we need to restore the tools and run docfx
                    Logger.LogInformation("Acquiring DocFX tool...");

                    await ProcessRunner.RunAsync(new("dotnet", "tool update docfx -g")
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);

                    Logger.LogInformation("Running DocFX...");

                    await ProcessRunner.RunAsync(new("dotnet", "docfx")
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);
                }

                Logger.LogInformation("DocFX site generated at {Path}", siteDirectory);

                // Copy the generated site to the publish directory
                await CopyDirectory(siteDirectory, RootedFileSystem.AtomPublishDirectory / GeneratedDocsArtifactName);
            });

    Target ServeDocs =>
        t => t
            .DescribedAs("Builds and serves the DocFX documentation site locally")
            .DependsOn(BuildDocs)
            .ConsumesArtifact(nameof(BuildDocs), GeneratedDocsArtifactName)
            .Executes(async cancellationToken =>
            {
                Logger.LogInformation("Serving DocFX site at http://localhost:8080/README.html");
                Logger.LogInformation("Press Ctrl+C to stop the server.");

                try
                {
                    await ProcessRunner.RunAsync(new("dotnet", "docfx serve _site --port 8080")
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Logger.LogInformation("DocFX server stopped.");
                }
            });

    Target PublishDocs =>
        t => t
            .DescribedAs("Publishes the generated DocFX site to GitHub Pages via the gh-pages branch")
            .RequiresParam(nameof(GithubToken))
            .DependsOn(BuildDocs)
            .DependsOn(SetupBuildInfo)
            .ConsumesArtifact(nameof(BuildDocs), GeneratedDocsArtifactName)
            .Executes(async cancellationToken =>
            {
                var siteArtifact = RootedFileSystem.AtomArtifactsDirectory / GeneratedDocsArtifactName;

                if (!RootedFileSystem.Directory.Exists(siteArtifact))
                    throw new StepFailedException("Site directory '_site' does not exist. Run BuildDocs first.");

                // Create a fresh temporary directory for the gh-pages checkout
                var tempDir = RootedFileSystem.AtomTempDirectory / "gh-pages-temp";

                if (RootedFileSystem.Directory.Exists(tempDir))
                    ForceDeleteDirectory(tempDir);

                RootedFileSystem.Directory.CreateDirectory(tempDir);

                try
                {
                    // Init a fresh repo
                    await ProcessRunner.RunAsync(new("git", "init")
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    await ProcessRunner.RunAsync(new("git", "checkout --orphan gh-pages")
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    // Set git user details for the commit
                    await ProcessRunner.RunAsync(new("git", ["config", "user.name", "\"github-actions[bot]\""])
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    await ProcessRunner.RunAsync(new("git",
                            ["config", "user.email", "\"41898282+github-actions[bot]@users.noreply.github.com\""])
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    // Copy the generated site to the gh-pages branch
                    await CopyDirectory(siteArtifact, tempDir);

                    // Commit
                    await ProcessRunner.RunAsync(new("git", "add .")
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    await ProcessRunner.RunAsync(
                        new("git", ["commit", "-m", "\"Deploy documentation to GitHub Pages\""])
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    // Get the remote URL from the main repo
                    var remoteResult = await ProcessRunner.RunAsync(new("git", "remote get-url origin")
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                            OutputLogLevel = LogLevel.Debug,
                        },
                        cancellationToken);

                    var remoteUrl = remoteResult.Output.Trim();

                    if (string.IsNullOrEmpty(remoteUrl))
                        throw new StepFailedException("Could not determine git remote URL.");

                    // Inject the GitHub token into the remote URL for authentication
                    if (!string.IsNullOrEmpty(GithubToken) && remoteUrl.StartsWith("https://"))
                        remoteUrl = remoteUrl.Replace("https://", $"https://x-access-token:{GithubToken}@");

                    // Force push to gh-pages
                    Logger.LogInformation("Pushing to gh-pages branch...");

                    await ProcessRunner.RunAsync(new("git", ["push", "--force", remoteUrl, "gh-pages"])
                        {
                            WorkingDirectory = tempDir,
                        },
                        cancellationToken);

                    Logger.LogInformation("Documentation published to GitHub Pages successfully.");
                }
                finally
                {
                    // Cleanup
                    if (RootedFileSystem.Directory.Exists(tempDir))
                        ForceDeleteDirectory(tempDir);
                }
            });

    /// <summary>
    ///     Copies all files and subdirectories from a source directory to a destination directory.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="destinationDirectory">The destination directory.</param>
    async Task CopyDirectory(RootedPath sourceDirectory, RootedPath destinationDirectory)
    {
        // Ensure the destination directory exists
        RootedFileSystem.Directory.CreateDirectory(destinationDirectory);

        // Copy all files
        foreach (var file in RootedFileSystem
                     .Directory
                     .GetFiles(sourceDirectory)
                     .Select(RootedFileSystem.CreateRootedPath))
            RootedFileSystem.File.Copy(file, destinationDirectory / RootedFileSystem.Path.GetFileName(file), true);

        // Recursively copy all subdirectories
        foreach (var directory in RootedFileSystem
                     .Directory
                     .GetDirectories(sourceDirectory)
                     .Select(RootedFileSystem.CreateRootedPath))
            await CopyDirectory(directory, destinationDirectory / RootedFileSystem.Path.GetFileName(directory));
    }

    /// <summary>
    ///     Recursively removes read-only attributes and deletes a directory.
    ///     Git object files are marked read-only, so a plain Directory.Delete fails.
    /// </summary>
    void ForceDeleteDirectory(string path)
    {
        foreach (var file in RootedFileSystem.Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var attrs = RootedFileSystem.File.GetAttributes(file);

            if (attrs.HasFlag(FileAttributes.ReadOnly))
                RootedFileSystem.File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }

        RootedFileSystem.Directory.Delete(path, true);
    }
}
