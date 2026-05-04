using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Atom.Targets;

internal interface IDocTargets : IDotnetCliHelper
{
    private const string GeneratedDocsArtifactName = "GeneratedDocs";

    Target BuildDocs =>
        t => t
            .DescribedAs("Generates API documentation using DocFX")
            .ProducesArtifact(GeneratedDocsArtifactName)
            .Executes(async cancellationToken =>
            {
                // First, build all projects in release mode
                await DotnetCli.Restore(AtomFileSystem.GetPath<Solution>(), cancellationToken: cancellationToken);

                await DotnetCli.Build(AtomFileSystem.GetPath<Solution>(),
                    new()
                    {
                        Configuration = "Release",
                        NoRestore = true,
                    },
                    cancellationToken: cancellationToken);

                var siteDirectory = AtomFileSystem.AtomRootDirectory / "_site";

                // If .NET 10, we can just do dotnet tool exec docfx
                if (RuntimeInformation.FrameworkDescription.StartsWith(".NET 10"))
                {
                    await ProcessRunner.RunAsync(new("dotnet", "tool exec -y docfx")
                        {
                            WorkingDirectory = AtomFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);
                }
                else
                {
                    // Otherwise, we need to restore the tools and run docfx
                    Logger.LogInformation("Acquiring DocFX tool...");

                    await ProcessRunner.RunAsync(new("dotnet", "tool update docfx -g")
                        {
                            WorkingDirectory = AtomFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);

                    Logger.LogInformation("Running DocFX...");

                    await ProcessRunner.RunAsync(new("dotnet", "docfx")
                        {
                            WorkingDirectory = AtomFileSystem.AtomRootDirectory,
                        },
                        cancellationToken);
                }

                Logger.LogInformation("DocFX site generated at {Path}", siteDirectory);

                // Copy the generated site to the artifact directory
                await CopyDirectory(siteDirectory, AtomFileSystem.AtomArtifactsDirectory / GeneratedDocsArtifactName);
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
                            WorkingDirectory = AtomFileSystem.AtomRootDirectory,
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
            .DependsOn(BuildDocs)
            .ConsumesArtifact(nameof(BuildDocs), GeneratedDocsArtifactName)
            .Executes(async cancellationToken =>
            {
                var siteArtifact = AtomFileSystem.AtomArtifactsDirectory / GeneratedDocsArtifactName;

                if (!AtomFileSystem.Directory.Exists(siteArtifact))
                    throw new StepFailedException("Site directory '_site' does not exist. Run BuildDocs first.");

                // Create a fresh temporary directory for the gh-pages checkout
                var tempDir = AtomFileSystem.AtomTempDirectory / "gh-pages-temp";

                if (AtomFileSystem.Directory.Exists(tempDir))
                    ForceDeleteDirectory(tempDir);

                AtomFileSystem.Directory.CreateDirectory(tempDir);

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
                            WorkingDirectory = AtomFileSystem.AtomRootDirectory,
                            OutputLogLevel = LogLevel.Debug,
                        },
                        cancellationToken);

                    var remoteUrl = remoteResult.Output.Trim();

                    if (string.IsNullOrEmpty(remoteUrl))
                        throw new StepFailedException("Could not determine git remote URL.");

                    // Force push to gh-pages
                    Logger.LogInformation("Pushing to gh-pages branch at {Remote}...", remoteUrl);

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
                    if (AtomFileSystem.Directory.Exists(tempDir))
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
        AtomFileSystem.Directory.CreateDirectory(destinationDirectory);

        // Copy all files
        foreach (var file in AtomFileSystem
                     .Directory
                     .GetFiles(sourceDirectory)
                     .Select(AtomFileSystem.CreateRootedPath))
            AtomFileSystem.File.Copy(file, destinationDirectory / AtomFileSystem.Path.GetFileName(file), true);

        // Recursively copy all subdirectories
        foreach (var directory in AtomFileSystem
                     .Directory
                     .GetDirectories(sourceDirectory)
                     .Select(AtomFileSystem.CreateRootedPath))
            await CopyDirectory(directory, destinationDirectory / AtomFileSystem.Path.GetFileName(directory));
    }

    /// <summary>
    ///     Recursively removes read-only attributes and deletes a directory.
    ///     Git object files are marked read-only, so a plain Directory.Delete fails.
    /// </summary>
    void ForceDeleteDirectory(string path)
    {
        foreach (var file in AtomFileSystem.Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var attrs = AtomFileSystem.File.GetAttributes(file);

            if (attrs.HasFlag(FileAttributes.ReadOnly))
                AtomFileSystem.File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }

        AtomFileSystem.Directory.Delete(path, true);
    }
}
