namespace DecSm.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides helper methods for packing .NET projects into NuGet packages and staging them for publishing.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IDotnetCliHelper" /> to utilize the .NET CLI for packing operations
///     and <see cref="IBuildInfo" /> to leverage build version information.
/// </remarks>
[PublicAPI]
public interface IDotnetPackHelper : IDotnetCliHelper, IBuildInfo
{
    /// <summary>
    ///     Packs a .NET project by its name and stages the resulting NuGet package.
    /// </summary>
    /// <param name="projectName">The name of the project to pack (e.g., "MyProject.Core").</param>
    /// <param name="options">Optional. Configuration options for the packing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="StepFailedException">Thrown if the project file cannot be located.</exception>
    /// <remarks>
    ///     This method first locates the project file based on its name, then calls
    ///     <see cref="DotnetPackAndStage(RootedPath, DotnetPackAndStageOptions?, CancellationToken)" />
    ///     with the resolved path.
    /// </remarks>
    [PublicAPI]
    Task DotnetPackAndStage(
        string projectName,
        DotnetPackAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetPackAndStage(projectPath, options, cancellationToken);
    }

    /// <summary>
    ///     Packs a .NET project specified by its path and stages the resulting NuGet package.
    /// </summary>
    /// <param name="projectPath">The full path to the project file (e.g., "src/MyProject/MyProject.csproj").</param>
    /// <param name="options">Optional. Configuration options for the packing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="StepFailedException">Thrown if the packed NuGet package is not found after the operation.</exception>
    /// <remarks>
    ///     <para>
    ///         This method performs the following steps:
    ///         <list type="number">
    ///             <item>
    ///                 <description>Determines the output directory for the NuGet package.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Cleans up existing package directories if specified.</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Applies version transformations to project files if
    ///                     <see cref="DotnetPackAndStageOptions.SetVersionsFromProviders" /> is <c>true</c>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Executes the `dotnet pack` command using
    ///                     <see cref="IDotnetCli.Pack(RootedPath, PackOptions?, ProcessRunOptions?, CancellationToken)" />.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Locates the generated `.nupkg` file.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Moves the `.nupkg` file to the Atom publish directory for the project.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         The <see cref="IBuildInfo.BuildVersion" /> is used for naming the NuGet package and for version transformation.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    async Task DotnetPackAndStage(
        RootedPath projectPath,
        DotnetPackAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new();
        var configuration = options.PackOptions?.Configuration ?? "Release";

        var projectName = projectPath.FileNameWithoutExtension;

        var buildDirectory = options.PackOptions?.Output is { Length: > 0 }
            ? FileSystem.CreateRootedPath(options.PackOptions?.Output!)
            : projectPath.Parent! / "bin" / configuration;

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        Logger.LogInformation("Packing project {Project}", projectName);

        if (FileSystem.Directory.Exists(buildDirectory))
        {
            Logger.LogDebug("Deleting existing pack directory {PackDirectory}", buildDirectory);
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

        await DotnetCli.Pack(projectPath, options.PackOptions, cancellationToken: cancellationToken);

        var packagedFile = FileSystem.CreateRootedPath(FileSystem
            .Directory
            .GetFiles(buildDirectory, $"{projectName}.*.nupkg")
            .OrderDescending()
            .First());

        if (!packagedFile.FileExists)
            throw new StepFailedException($"Package {packagedFile} does not exist.");

        Logger.LogDebug("Found packaged file {PackagedFile}", packagedFile);

        var publishedFile = publishDirectory / packagedFile.FileName!;

        Logger.LogDebug("Moving package {PackagedFile} to {PublishedFile}", packagedFile, publishedFile);

        if (options.ClearPublishDirectory && FileSystem.Directory.Exists(publishDirectory))
        {
            Logger.LogDebug("Deleting existing publish directory {PublishDirectory}", publishDirectory);
            FileSystem.Directory.Delete(publishDirectory, true);
        }

        FileSystem.Directory.CreateDirectory(publishDirectory);

        if (FileSystem.File.Exists(publishedFile))
        {
            Logger.LogDebug("Deleting existing published file {PublishedFile}", publishedFile);
            FileSystem.File.Delete(publishedFile);
        }

        FileSystem.File.Move(packagedFile, publishedFile);

        Logger.LogInformation("Packed project {Project}", projectName);
    }
}

/// <summary>
///     Represents options for the
///     <see cref="IDotnetPackHelper.DotnetPackAndStage(RootedPath, DotnetPackAndStageOptions?, CancellationToken)" />
///     operation.
/// </summary>
[PublicAPI]
public sealed record DotnetPackAndStageOptions
{
    /// <summary>
    ///     Gets or sets the specific options to pass to the `dotnet pack` command.
    /// </summary>
    public PackOptions? PackOptions { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to automatically set project versions
    ///     from the build version providers (<see cref="IBuildInfo.BuildVersion" />).
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, the module will attempt to inject the build version into project files
    ///     before packing. Defaults to <c>true</c>.
    /// </remarks>
    public bool SetVersionsFromProviders { get; init; } = true;

    /// <summary>
    ///     Gets or sets a custom transformation function to apply to project property files
    ///     before packing.
    /// </summary>
    /// <remarks>
    ///     This function can be used to modify the content of `.csproj` or `.props` files
    ///     (e.g., to inject custom properties or modify existing ones) during the packing process.
    /// </remarks>
    public Func<string, string>? CustomPropertiesTransform { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to clear the publish directory before staging the new package.
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, the target publish directory for the NuGet package will be deleted
    ///     and recreated before moving the new package. Defaults to <c>true</c>.
    /// </remarks>
    public bool ClearPublishDirectory { get; init; } = true;
}
