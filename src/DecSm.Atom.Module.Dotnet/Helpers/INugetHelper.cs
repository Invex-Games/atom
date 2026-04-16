namespace DecSm.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides helper methods for interacting with NuGet, such as pushing packages and managing NuGet configuration.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IBuildInfo" /> to leverage build version information
///     and provides functionality for common NuGet operations within a DecSm.Atom build.
/// </remarks>
[PublicAPI]
public interface INugetHelper : IBuildInfo
{
    /// <summary>
    ///     Gets a value indicating whether NuGet write operations (like pushing packages) should be performed as a dry run.
    /// </summary>
    /// <remarks>
    ///     When <c>true</c>, NuGet operations that modify remote feeds will be simulated and logged,
    ///     but no actual changes will be made. This is useful for testing build configurations.
    /// </remarks>
    [ParamDefinition("nuget-dry-run", "Whether to perform a dry run of nuget write operations.")]
    bool NugetDryRun => GetParam(() => NugetDryRun);

    /// <summary>
    ///     Gets the path to the default NuGet configuration file (`NuGet.Config`) for the current operating system.
    /// </summary>
    /// <remarks>
    ///     This property determines the standard location where NuGet configuration files are expected.
    ///     On Windows, it's typically `%APPDATA%\NuGet\NuGet.Config`.
    ///     On Linux/macOS, it's typically `$HOME/.nuget/NuGet.Config`.
    /// </remarks>
    RootedPath NugetConfigPath
    {
        get
        {
            // Windows: %APPDATA%\NuGet\NuGet.Config
            // Linux: $HOME/.nuget/NuGet.Config
            // Mac: $HOME/.nuget/NuGet.Config
            var appDataPath =
                FileSystem.CreateRootedPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => appDataPath / "NuGet" / "NuGet.Config",
                _ => appDataPath / ".nuget" / "NuGet.Config",
            };
        }
    }

    /// <summary>
    ///     Pushes a NuGet package for a specific project to a NuGet feed.
    /// </summary>
    /// <param name="projectName">The name of the project whose package is to be pushed.</param>
    /// <param name="feed">The NuGet feed URL to push the package to.</param>
    /// <param name="apiKey">The API key for authenticating with the NuGet feed.</param>
    /// <param name="configFile">Optional path to a NuGet configuration file to use instead of the default one.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This method expects the project to have been built and its NuGet package (`.nupkg`)
    ///         to be available in the Atom artifacts directory, named according to the project name
    ///         and the <see cref="IBuildInfo.BuildVersion" />.
    ///     </para>
    ///     <para>
    ///         It requires the `dotnet` CLI to be installed and available in the system's PATH.
    ///     </para>
    ///     <para>
    ///         If <see cref="NugetDryRun" /> is <c>true</c>, the push operation will be simulated.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    async Task PushProject(
        string projectName,
        string feed,
        string apiKey,
        RootedPath? configFile = null,
        CancellationToken cancellationToken = default)
    {
        var packageBuildDir = FileSystem.AtomArtifactsDirectory / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");

        if (packages.Length == 0)
        {
            Logger.LogWarning("No packages found in {PackageBuildDir}", packageBuildDir);

            return;
        }

        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{BuildVersion}.nupkg");

        await PushPackageToNuget(packageBuildDir / matchingPackage, feed, apiKey, configFile, cancellationToken);
    }

    /// <summary>
    ///     Pushes a specific NuGet package file to a NuGet feed.
    /// </summary>
    /// <param name="packagePath">The full path to the `.nupkg` file to push.</param>
    /// <param name="feed">The NuGet feed URL to push the package to.</param>
    /// <param name="apiKey">The API key for authenticating with the NuGet feed.</param>
    /// <param name="configFile">Optional path to a NuGet configuration file to use instead of the default one.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     If <see cref="NugetDryRun" /> is <c>true</c>, the push operation will be simulated.
    /// </remarks>
    [PublicAPI]
    async Task PushPackageToNuget(
        RootedPath packagePath,
        string feed,
        string apiKey,
        RootedPath? configFile = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Attempting to push NuGet package '{PackagePath}' to feed '{Feed}'.", packagePath, feed);

        if (NugetDryRun)
        {
            Logger.LogInformation("Dry run enabled: Skipping actual NuGet push for '{PackagePath}' to '{Feed}'.",
                packagePath,
                feed);

            return;
        }

        var configFileFlag = configFile is not null
            ? $" --configfile \"{configFile}\""
            : string.Empty;

        var processRunResult = await ProcessRunner.RunAsync(new("dotnet",
                $"nuget push \"{packagePath}\"{configFileFlag} --source {feed} --api-key {apiKey}"),
            cancellationToken);

        if (processRunResult.ExitCode is not 0)
            Logger.LogError("Failed to push package to Nuget: {ProcessRunResult}", processRunResult.Error);

        Logger.LogInformation("Package pushed");
    }

    /// <summary>
    ///     Checks if a specific version of a NuGet package is already published to a given feed.
    /// </summary>
    /// <param name="projectName">The name of the NuGet package (e.g., "MyProject.Core").</param>
    /// <param name="version">The semantic version of the package (e.g., "1.0.0").</param>
    /// <param name="feedUrl">The URL of the NuGet feed (e.g., "https://api.nuget.org/v3/index.json").</param>
    /// <param name="feedKey">Optional API key for authenticated feeds.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns <c>true</c> if the package version is published, otherwise <c>false</c>
    ///     .
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the NuGet feed's registration base URL cannot be found or if the
    ///     status check fails unexpectedly.
    /// </exception>
    [PublicAPI]
    async Task<bool> IsNugetPackageVersionPublished(
        string projectName,
        string version,
        string feedUrl,
        string? feedKey = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Checking if NuGet package '{ProjectName}' version '{Version}' is published to feed '{FeedUrl}'.",
            projectName,
            version,
            feedUrl);

        using var httpClient = new HttpClient();

        if (feedKey is { Length: > 0 })
            httpClient.DefaultRequestHeaders.Add("X-NuGet-ApiKey", feedKey);

        var index = await httpClient.GetStringAsync(feedUrl, cancellationToken);

        // Parse the NuGet service index to find the RegistrationsBaseUrl
        var registrationsBaseUrl = JsonNode
            .Parse(index)?["resources"]
            ?.AsArray()
            .Select(x => x)
            .FirstOrDefault(x => x?["@type"]
                                     ?.ToString() ==
                                 "RegistrationsBaseUrl/Versioned")?["@id"]
            ?.ToString();

        if (registrationsBaseUrl is null)
            throw new InvalidOperationException(
                $"Could not find the 'RegistrationsBaseUrl/Versioned' resource in the NuGet feed service index at '{feedUrl}'.");

        // Construct the URL to check for the specific package version
        var packageUrl = $"{registrationsBaseUrl}{projectName.ToLowerInvariant()}/{version}.json";

        var response = await httpClient.GetAsync(packageUrl, cancellationToken);

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.NotFound))
            throw new InvalidOperationException(
                $"Failed to check if version '{version}' of '{projectName}' is published to '{feedUrl}'. Unexpected status code: {response.StatusCode}.");

        var isPublished = response.StatusCode is HttpStatusCode.OK;

        Logger.LogInformation("NuGet package '{ProjectName}' version '{Version}' is {Status} on feed '{FeedUrl}'.",
            projectName,
            version,
            isPublished
                ? "published"
                : "not published",
            feedUrl);

        return isPublished;
    }

    /// <summary>
    ///     Creates a <see cref="TransformFileScope" /> that temporarily overwrites the default NuGet.Config file
    ///     with the provided content.
    /// </summary>
    /// <param name="contents">The new content for the NuGet.Config file.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns a <see cref="TransformFileScope" /> instance.
    ///     Disposing this scope will restore the original NuGet.Config file.
    /// </returns>
    [PublicAPI]
    async Task<TransformFileScope> CreateNugetConfigOverwriteScope(
        string contents,
        CancellationToken cancellationToken = default) =>
        await TransformFileScope.CreateAsync(NugetConfigPath, _ => contents, cancellationToken);

    /// <summary>
    ///     Creates a <see cref="TransformFileScope" /> that temporarily overwrites the default NuGet.Config file
    ///     with a configuration that includes the specified NuGet feeds and their credentials.
    /// </summary>
    /// <param name="feeds">An array of <see cref="NugetFeed" /> objects to include in the configuration.</param>
    /// <param name="skipIfExists">
    ///     If <c>true</c>, and all specified feeds are already present in the existing NuGet.Config,
    ///     no overwrite will occur, and an empty scope will be returned.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns a <see cref="TransformFileScope" /> instance.
    ///     Disposing this scope will restore the original NuGet.Config file.
    /// </returns>
    /// <remarks>
    ///     This method generates a `NuGet.Config` XML structure including `&lt;packageSources&gt;` and `&lt;
    ///     packageSourceCredentials&gt;` based on the provided feed information. Passwords are included as clear text if
    ///     specified via <see cref="NugetFeed.PlainTextPassword" />.
    /// </remarks>
    [PublicAPI]
    async Task CreateNugetConfigOverwriteScope(
        NugetFeed[] feeds,
        bool skipIfExists = false,
        CancellationToken cancellationToken = default)
    {
        if (skipIfExists)
        {
            var nugetContents = await FileSystem.File.ReadAllTextAsync(NugetConfigPath, cancellationToken);

            if (feeds.All(f => nugetContents.Contains(f.Url, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.LogInformation("Nuget config already contains package sources, skipping");

                return;
            }
        }

        var sources = string.Join(Environment.NewLine,
            feeds.Select((x, i) => $"<add key=\"{x.Name ?? $"Feed{i}"}\" value=\"{x.Url}\" />"));

        var credentials = string.Join(Environment.NewLine,
            feeds.Select((x, i) =>
            {
                var result = new StringBuilder();

                result.AppendLine($"    <{x.Name ?? $"Feed{i}"}>");

                var username = x.Username();

                if (username is not null)
                    result.AppendLine($"      <add key=\"Username\" value=\"{username}\" />");

                var password = x.Password();

                if (password is not null)
                    result.AppendLine($"      <add key=\"Password\" value=\"{password}\" />");

                var plainTextPassword = x.PlainTextPassword();

                if (plainTextPassword is not null)
                    result.AppendLine($"      <add key=\"ClearTextPassword\" value=\"{plainTextPassword}\" />");

                result.AppendLine($"    </{x.Name ?? $"Feed{i}"}>");

                return result.ToString();
            }));

        var configText = $"""
                          <configuration>
                            <packageSources>
                          {sources}
                            </packageSources>

                            <packageSourceCredentials>
                          {credentials}
                            </packageSourceCredentials>
                          </configuration>
                          """;

        await CreateNugetConfigOverwriteScope(configText, cancellationToken);
    }
}
