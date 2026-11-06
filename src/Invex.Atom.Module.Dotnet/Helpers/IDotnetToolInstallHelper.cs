namespace Invex.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides helper methods for installing and managing .NET CLI tools.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IBuildAccessor" /> to gain access to core build services
///     like <see cref="IProcessRunner" /> and <see cref="IRootedFileSystem" />.
/// </remarks>
[PublicAPI]
public interface IDotnetToolInstallHelper : IBuildAccessor
{
    /// <summary>
    ///     Gets the installed version of a .NET CLI tool.
    /// </summary>
    /// <param name="toolName">The package ID of the tool (e.g., "GitVersion.Tool"), matched case-insensitively.</param>
    /// <param name="global">
    ///     If <c>true</c>, the global tools are searched. If <c>false</c>, the local tool manifest of the current
    ///     directory is searched.
    /// </param>
    /// <returns>
    ///     The installed version as reported by `dotnet tool list` (e.g. <c>6.8.2</c>), or <c>null</c> if the tool is
    ///     not installed.
    /// </returns>
    /// <remarks>
    ///     Runs `dotnet tool list &lt;toolName&gt;` on every call; the result is not cached. The package ID column
    ///     must equal <paramref name="toolName" />, so tools whose IDs only start with it are ignored.
    /// </remarks>
    [PublicAPI]
    string? GetInstalledToolVersion(string toolName, bool global = true)
    {
        var listResult = ProcessRunner.Run(new("dotnet",
            [
                "tool", "list", toolName, global
                    ? "-g"
                    : string.Empty,
            ])
            {
                AllowFailedResult = true,
                InvocationLogLevel = LogLevel.Debug,
            });

        return DotnetToolVersion.FindInstalledVersion(listResult.Output, toolName);
    }

    /// <summary>
    ///     Asynchronously gets the installed version of a .NET CLI tool.
    /// </summary>
    /// <param name="toolName">The package ID of the tool (e.g., "GitVersion.Tool"), matched case-insensitively.</param>
    /// <param name="global">
    ///     If <c>true</c>, the global tools are searched. If <c>false</c>, the local tool manifest of the current
    ///     directory is searched.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task whose result is the installed version as reported by `dotnet tool list` (e.g. <c>6.8.2</c>), or
    ///     <c>null</c> if the tool is not installed.
    /// </returns>
    /// <remarks>
    ///     Runs `dotnet tool list &lt;toolName&gt;` on every call; the result is not cached. The package ID column
    ///     must equal <paramref name="toolName" />, so tools whose IDs only start with it are ignored.
    /// </remarks>
    [PublicAPI]
    async Task<string?> GetInstalledToolVersionAsync(
        string toolName,
        bool global = true,
        CancellationToken cancellationToken = default)
    {
        var listResult = await ProcessRunner.RunAsync(new("dotnet",
                [
                    "tool", "list", toolName, global
                        ? "-g"
                        : string.Empty,
                ])
                {
                    AllowFailedResult = true,
                    InvocationLogLevel = LogLevel.Debug,
                },
                cancellationToken);

        return DotnetToolVersion.FindInstalledVersion(listResult.Output, toolName);
    }

    /// <summary>
    ///     Installs or updates a .NET CLI tool.
    /// </summary>
    /// <param name="toolName">The name of the tool to install (e.g., "dotnet-format").</param>
    /// <param name="version">
    ///     Optional. The version of the tool to install: an exact version (e.g. <c>6.8.2</c>) or a floating version
    ///     with a trailing wildcard (e.g. <c>6.*</c>, which installs the latest stable 6.x). If not specified, the
    ///     latest stable version is used.
    /// </param>
    /// <param name="global">
    ///     If <c>true</c>, the tool is installed globally. If <c>false</c>, it's installed locally to the
    ///     current directory.
    /// </param>
    /// <param name="forceReinstall">If <c>true</c>, the tool will be reinstalled even if already present.</param>
    /// <remarks>
    ///     <para>
    ///         This method first checks if the tool is already installed (unless <paramref name="forceReinstall" /> is
    ///         <c>true</c>
    ///         or running in headless mode). If not installed or reinstallation is forced, it executes `dotnet tool update`.
    ///     </para>
    ///     <para>
    ///         The tool counts as installed when <see cref="GetInstalledToolVersion" /> reports a version that
    ///         satisfies <paramref name="version" />: any version when <paramref name="version" /> is <c>null</c> or
    ///         empty, an equal version for an exact version (<c>1.2</c> equals <c>1.2.0</c>, but not
    ///         <c>1.2.3</c>), or a stable version with the same leading components for a floating version
    ///         (<c>6.*</c> matches <c>6.8.2</c> but not <c>7.0.0</c>).
    ///     </para>
    ///     <para>
    ///         When <paramref name="version" /> is specified, `dotnet tool update` is run with `--allow-downgrade`, so
    ///         a newer installed version is replaced by the requested one instead of failing the update.
    ///     </para>
    ///     <para>
    ///         If installing locally (<paramref name="global" /> is <c>false</c>) and a tool manifest
    ///         (`.config/dotnet-tools.json`)
    ///         does not exist, it will be created.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    void InstallTool(string toolName, string? version = null, bool global = true, bool forceReinstall = false)
    {
        var globalFlag = global
            ? "-g"
            : string.Empty;

        if (!global &&
            !RootedFileSystem.File.Exists(RootedFileSystem.CurrentDirectory / ".config" / "dotnet-tools.json"))
            ProcessRunner.Run(new("dotnet", "new tool-manifest")
            {
                InvocationLogLevel = LogLevel.Debug,
            });

        if (!forceReinstall &&
            !GetService<CommandLineArgs>()
                .HasHeadless)
        {
            var installedVersion = GetInstalledToolVersion(toolName, global);

            if (installedVersion is not null && DotnetToolVersion.IsMatch(installedVersion, version))
            {
                Logger.LogDebug("Tool '{ToolName}' {Version} is already installed.", toolName, installedVersion);

                return;
            }
        }

        var versionFlag = !string.IsNullOrWhiteSpace(version)
            ? $"--version {version} --allow-downgrade"
            : string.Empty;

        ProcessRunner.Run(new("dotnet", $"tool update {toolName} {versionFlag} {globalFlag}")
        {
            InvocationLogLevel = LogLevel.Debug,
        });
    }

    /// <summary>
    ///     Asynchronously installs or updates a .NET CLI tool.
    /// </summary>
    /// <param name="toolName">The name of the tool to install (e.g., "dotnet-format").</param>
    /// <param name="version">
    ///     Optional. The version of the tool to install: an exact version (e.g. <c>6.8.2</c>) or a floating version
    ///     with a trailing wildcard (e.g. <c>6.*</c>, which installs the latest stable 6.x). If not specified, the
    ///     latest stable version is used.
    /// </param>
    /// <param name="global">
    ///     If <c>true</c>, the tool is installed globally. If <c>false</c>, it's installed locally to the
    ///     current directory.
    /// </param>
    /// <param name="forceReinstall">If <c>true</c>, the tool will be reinstalled even if already present.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This method first checks if the tool is already installed (unless <paramref name="forceReinstall" /> is
    ///         <c>true</c>
    ///         or running in headless mode). If not installed or reinstallation is forced, it executes `dotnet tool update`.
    ///     </para>
    ///     <para>
    ///         The tool counts as installed when <see cref="GetInstalledToolVersion" /> reports a version that
    ///         satisfies <paramref name="version" />: any version when <paramref name="version" /> is <c>null</c> or
    ///         empty, an equal version for an exact version (<c>1.2</c> equals <c>1.2.0</c>, but not
    ///         <c>1.2.3</c>), or a stable version with the same leading components for a floating version
    ///         (<c>6.*</c> matches <c>6.8.2</c> but not <c>7.0.0</c>).
    ///     </para>
    ///     <para>
    ///         When <paramref name="version" /> is specified, `dotnet tool update` is run with `--allow-downgrade`, so
    ///         a newer installed version is replaced by the requested one instead of failing the update.
    ///     </para>
    ///     <para>
    ///         If installing locally (<paramref name="global" /> is <c>false</c>) and a tool manifest
    ///         (`.config/dotnet-tools.json`)
    ///         does not exist, it will be created.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    async Task InstallToolAsync(
        string toolName,
        string? version = null,
        bool global = true,
        bool forceReinstall = false,
        CancellationToken cancellationToken = default)
    {
        var globalFlag = global
            ? "-g"
            : string.Empty;

        if (!global &&
            !RootedFileSystem.File.Exists(RootedFileSystem.CurrentDirectory / ".config" / "dotnet-tools.json"))
            await ProcessRunner.RunAsync(new("dotnet", "new tool-manifest")
                {
                    InvocationLogLevel = LogLevel.Debug,
                },
                cancellationToken);

        if (!forceReinstall &&
            !GetService<CommandLineArgs>()
                .HasHeadless)
        {
            var installedVersion = await GetInstalledToolVersionAsync(toolName, global, cancellationToken);

            if (installedVersion is not null && DotnetToolVersion.IsMatch(installedVersion, version))
            {
                Logger.LogInformation("Tool '{ToolName}' {Version} is already installed.", toolName, installedVersion);

                return;
            }
        }

        var versionFlag = !string.IsNullOrWhiteSpace(version)
            ? $"--version {version} --allow-downgrade"
            : string.Empty;

        await ProcessRunner.RunAsync(new("dotnet", $"tool update {toolName} {versionFlag} {globalFlag}")
            {
                InvocationLogLevel = LogLevel.Debug,
            },
            cancellationToken);
    }
}
