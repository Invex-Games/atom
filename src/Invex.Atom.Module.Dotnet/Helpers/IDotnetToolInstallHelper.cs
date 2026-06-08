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
    ///     Installs or updates a .NET CLI tool.
    /// </summary>
    /// <param name="toolName">The name of the tool to install (e.g., "dotnet-format").</param>
    /// <param name="version">
    ///     Optional. The specific version of the tool to install. If not specified, the latest stable
    ///     version is used.
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
            var globalListResult = ProcessRunner.Run(new("dotnet", $"tool list {toolName} {globalFlag}")
            {
                AllowFailedResult = true,
                InvocationLogLevel = LogLevel.Debug,
            });

            var toolVersion = globalListResult
                .Output
                .Split(Environment.NewLine)
                .FirstOrDefault(x =>
                    x.StartsWith(toolName, StringComparison.OrdinalIgnoreCase) &&
                    (version is null or "" || x.Contains(version, StringComparison.OrdinalIgnoreCase)));

            if (toolVersion != null)
            {
                Logger.LogDebug("Tool '{ToolName}' {Version} is already installed.", toolName, version);

                return;
            }
        }

        var versionFlag = version != null
            ? $"--version {version}"
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
    ///     Optional. The specific version of the tool to install. If not specified, the latest stable
    ///     version is used.
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
            var globalListResult = await ProcessRunner.RunAsync(new("dotnet", $"tool list {toolName} {globalFlag}")
                {
                    AllowFailedResult = true,
                    InvocationLogLevel = LogLevel.Debug,
                },
                cancellationToken);

            var toolVersion = globalListResult
                .Output
                .Split(Environment.NewLine)
                .FirstOrDefault(x =>
                    x.StartsWith(toolName, StringComparison.OrdinalIgnoreCase) &&
                    (version is null or "" || x.Contains(version, StringComparison.OrdinalIgnoreCase)));

            if (toolVersion != null)
            {
                Logger.LogInformation("Tool '{ToolName}' {Version} is already installed.", toolName, version);

                return;
            }
        }

        var versionFlag = version != null
            ? $"--version {version}"
            : string.Empty;

        await ProcessRunner.RunAsync(new("dotnet", $"tool update {toolName} {versionFlag} {globalFlag}")
            {
                InvocationLogLevel = LogLevel.Debug,
            },
            cancellationToken);
    }
}
