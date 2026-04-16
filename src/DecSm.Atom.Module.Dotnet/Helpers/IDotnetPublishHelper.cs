namespace DecSm.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides helper methods for publishing .NET projects and staging their output.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IDotnetCliHelper" /> to utilize the .NET CLI for publishing operations
///     and <see cref="IBuildInfo" /> to leverage build version information.
/// </remarks>
[PublicAPI]
public interface IDotnetPublishHelper : IDotnetCliHelper, IBuildInfo
{
    /// <summary>
    ///     Publishes a .NET project by its name and stages the output.
    /// </summary>
    /// <param name="projectName">The name of the project to publish (e.g., "MyWebApp").</param>
    /// <param name="options">Optional. Configuration options for the publishing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="StepFailedException">Thrown if the project file cannot be located.</exception>
    /// <remarks>
    ///     This method first locates the project file based on its name, then calls
    ///     <see cref="DotnetPublishAndStage(RootedPath, DotnetPublishAndStageOptions?, CancellationToken)" />
    ///     with the resolved path.
    /// </remarks>
    [PublicAPI]
    Task DotnetPublishAndStage(
        string projectName,
        DotnetPublishAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetPublishAndStage(projectPath, options, cancellationToken);
    }

    /// <summary>
    ///     Publishes a .NET project specified by its path and stages the output.
    /// </summary>
    /// <param name="projectPath">The full path to the project file (e.g., "src/MyWebApp/MyWebApp.csproj").</param>
    /// <param name="options">Optional. Configuration options for the publishing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This method performs the following steps:
    ///         <list type="number">
    ///             <item>
    ///                 <description>Determines the output directory for the published application.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Cleans up existing publish directories if specified.</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Applies version transformations to project files if
    ///                     <see cref="DotnetPublishAndStageOptions.SetVersionsFromProviders" /> is <c>true</c>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Executes the `dotnet publish` command using
    ///                     <see cref="IDotnetCli.Publish(RootedPath, PublishOptions?, ProcessRunOptions?, CancellationToken)" />
    ///                     .
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Moves the published output to the Atom publish directory for the project.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         The <see cref="IBuildInfo.BuildVersion" /> is used for version transformation.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    async Task DotnetPublishAndStage(
        RootedPath projectPath,
        DotnetPublishAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new();
        var configuration = options.PublishOptions?.Configuration ?? "Release";

        var projectName = projectPath.FileNameWithoutExtension;

        var buildDirectory = options.PublishOptions?.Output is { Length: > 0 }
            ? FileSystem.CreateRootedPath(options.PublishOptions?.Output!)
            : FileSystem.AtomRootDirectory / projectName / configuration / "atom-publish";

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        Logger.LogInformation("Publishing project {Project}", projectName);

        if (FileSystem.Directory.Exists(buildDirectory))
        {
            Logger.LogDebug("Deleting existing build directory {BuildDirectory}", buildDirectory);
            FileSystem.Directory.Delete(buildDirectory, true);
        }

        Logger.LogDebug(
            "Transforming project properties: SetVersionsFromProviders={SetVersionsFromProviders}, CustomPropertiesTransform={CustomPropertiesTransform}",
            options.SetVersionsFromProviders,
            options.CustomPropertiesTransform is not null
                ? "true"
                : "false");

        await using var transformFilesScope =
            (options.SetVersionsFromProviders, options.CustomPropertiesTransform) switch
            {
                (true, not null) => await TransformProjectVersionScope
                    .CreateAsync(DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                        BuildVersion,
                        cancellationToken)
                    .AddAsync(options.CustomPropertiesTransform),

                (true, null) => await TransformProjectVersionScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    BuildVersion,
                    cancellationToken),

                (false, not null) => await TransformMultiFileScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    options.CustomPropertiesTransform!,
                    cancellationToken),

                _ => null,
            };

        await DotnetCli.Publish(projectPath,
            options.PublishOptions is null
                ? new()
                {
                    Output = buildDirectory,
                }
                : options.PublishOptions with
                {
                    Output = buildDirectory,
                },
            cancellationToken: cancellationToken);

        Logger.LogDebug(
            "Moving files from temporary publish directory {BuiltDirectory} to Atom publish directory {PublishedDirectory}",
            buildDirectory,
            publishDirectory);

        if (FileSystem.Directory.Exists(publishDirectory))
        {
            Logger.LogDebug("Deleting existing Atom publish directory {PublishDirectory}", publishDirectory);
            FileSystem.Directory.Delete(publishDirectory, true);
        }

        if (!FileSystem.Directory.Exists(publishDirectory.Parent!))
            FileSystem.Directory.CreateDirectory(publishDirectory.Parent!);

        FileSystem.Directory.Move(buildDirectory, publishDirectory);

        Logger.LogInformation("Published project {Project}", projectName);
    }
}

/// <summary>
///     Represents options for the
///     <see
///         cref="IDotnetPublishHelper.DotnetPublishAndStage(RootedPath, DotnetPublishAndStageOptions?, CancellationToken)" />
///     operation.
/// </summary>
[PublicAPI]
public sealed record DotnetPublishAndStageOptions
{
    /// <summary>
    ///     Gets or sets the specific options to pass to the `dotnet publish` command.
    /// </summary>
    public PublishOptions? PublishOptions { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to automatically set project versions
    ///     from the build version providers (<see cref="IBuildInfo.BuildVersion" />).
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, the module will attempt to inject the build version into project files
    ///     before publishing. Defaults to <c>true</c>.
    /// </remarks>
    public bool SetVersionsFromProviders { get; init; } = true;

    /// <summary>
    ///     Gets or sets a custom transformation function to apply to project property files
    ///     before publishing.
    /// </summary>
    /// <remarks>
    ///     This function can be used to modify the content of `.csproj` or `.props` files
    ///     (e.g., to inject custom properties or modify existing ones) during the publishing process.
    /// </remarks>
    public Func<string, string>? CustomPropertiesTransform { get; init; }
}
