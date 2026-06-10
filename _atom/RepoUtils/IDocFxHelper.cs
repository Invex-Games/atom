using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Atom.RepoUtils;

/// <summary>
///     Provides functionality for generating, serving, and publishing
///     <see href="https://dotnet.github.io/docfx/">DocFX</see> documentation for the repository.
///     The helper builds the static documentation site, can host it locally for previewing, and
///     publishes the generated output to a project's <c>gh-pages</c> branch for GitHub Pages hosting.
/// </summary>
internal interface IDocFxHelper : IDotnetCliHelper, IGithubHelper, ISetupBuildInfo
{
    /// <summary>
    ///     The name of the build artifact (and corresponding output sub-directory) that contains the
    ///     generated DocFX site. Used both as the destination folder under the publish directory and as
    ///     the artifact name resolved under the artifacts directory when publishing.
    /// </summary>
    const string GeneratedDocsArtifactName = "GeneratedDocs";

    /// <summary>
    ///     Builds the DocFX documentation site and copies the generated output into the publish
    ///     directory under <see cref="GeneratedDocsArtifactName" />.
    /// </summary>
    /// <param name="projectsToPrebuild">
    ///     An optional collection of projects to build in <c>Release</c> configuration before running
    ///     DocFX. This ensures any metadata DocFX extracts from the compiled assemblies and XML
    ///     documentation is up to date. When <see langword="null" />, no projects are pre-built.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <remarks>
    ///     On .NET 10 or newer the tool is executed directly via <c>dotnet tool exec docfx</c>. On
    ///     earlier runtimes the DocFX global tool is updated/installed first and then invoked via
    ///     <c>dotnet docfx</c>. The site is generated into the <c>_site</c> directory beneath the Atom
    ///     root before being copied to the publish directory.
    /// </remarks>
    async Task BuildDocFxDocs(
        IEnumerable<RootedPath>? projectsToPrebuild = null,
        CancellationToken cancellationToken = default)
    {
        // First, build projects in release mode
        foreach (var project in projectsToPrebuild ?? Array.Empty<RootedPath>())
            await DotnetCli.Build([project],
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
    }

    /// <summary>
    ///     Serves the generated DocFX site locally for previewing, hosting it at
    ///     <c>http://localhost:8080</c> until the operation is cancelled.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token used to stop the local server. Cancellation is handled gracefully and results in the
    ///     server shutting down without surfacing an error.
    /// </param>
    /// <remarks>
    ///     This serves the contents of the <c>_site</c> directory beneath the Atom root, so
    ///     <see cref="BuildDocFxDocs" /> should be run beforehand to ensure the site exists.
    /// </remarks>
    async Task ServeDocFxDocs(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Serving DocFX site at http://localhost:8080/README.html");
        Logger.LogInformation("Press Ctrl+C to stop the server.");

        try
        {
            await ProcessRunner.RunAsync(new("dotnet",
                    $"tool exec docfx -- serve {RootedFileSystem.AtomRootDirectory / "_site"} --port 8080"),
                cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Logger.LogInformation("DocFX server stopped.");
        }
    }

    /// <summary>
    ///     Publishes a previously generated DocFX site to the repository's <c>gh-pages</c> branch,
    ///     making it available via GitHub Pages.
    /// </summary>
    /// <param name="githubToken">
    ///     The GitHub token used to authenticate the push. When the remote uses HTTPS, the token is
    ///     injected into the remote URL as an <c>x-access-token</c> credential.
    /// </param>
    /// <param name="generatedDocsArtifactName">
    ///     The name of the artifact (under the artifacts directory) that contains the generated site to
    ///     publish, typically <see cref="GeneratedDocsArtifactName" />.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <remarks>
    ///     The site is published by creating a fresh orphan <c>gh-pages</c> branch in a temporary
    ///     directory, committing the generated files, and force-pushing to the origin remote. The
    ///     temporary checkout is always removed once the operation completes, even on failure.
    /// </remarks>
    /// <exception cref="StepFailedException">
    ///     Thrown when the generated site artifact does not exist, or when the origin remote URL cannot
    ///     be determined.
    /// </exception>
    async Task PublishDocFxDocsToGithub(
        string githubToken,
        string generatedDocsArtifactName,
        CancellationToken cancellationToken = default)
    {
        var siteArtifact = RootedFileSystem.AtomArtifactsDirectory / generatedDocsArtifactName;

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

            await ProcessRunner.RunAsync(new("git", ["commit", "-m", "\"Deploy documentation to GitHub Pages\""])
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
            if (!string.IsNullOrEmpty(githubToken) && remoteUrl.StartsWith("https://"))
                remoteUrl = remoteUrl.Replace("https://", $"https://x-access-token:{githubToken}@");

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
    }

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
    /// <param name="path">The path of the directory to delete.</param>
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
