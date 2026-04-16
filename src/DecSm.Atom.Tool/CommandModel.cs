#pragma warning disable CA1822, RCS1163

namespace DecSm.Atom.Tool;

/// <summary>
///     Defines the command-line interface (CLI) commands available in the DecSm.Atom.Tool.
/// </summary>
/// <remarks>
///     This class uses ConsoleAppFramework to expose various functionalities,
///     allowing users to interact with DecSm.Atom projects and NuGet configurations
///     directly from the command line.
/// </remarks>
[PublicAPI]
internal sealed class CommandModel
{
    /// <summary>
    ///     Runs the specified DecSm.Atom project with the given arguments.
    ///     This is the default command executed when no specific subcommand is provided.
    /// </summary>
    /// <param name="context">The console app context, providing access to raw arguments.</param>
    /// <param name="runArgs">
    ///     Optional. Arguments to pass directly to the DecSm.Atom build project.
    ///     These arguments are typically used to specify targets or parameters for the build.
    /// </param>
    /// <param name="project">
    ///     Optional. The name of the DecSm.Atom project or file-based app to run. Defaults to "_atom".
    ///     This allows specifying which build definition to execute within a multi-project solution.
    /// </param>
    /// <param name="file">
    ///     Optional. The path to a C# file to run as a file-based DecSm.Atom application.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command to complete.</param>
    /// <returns>The exit code of the executed DecSm.Atom build command.</returns>
    [Command("")]
#pragma warning disable CA1822
    public Task<int> Root(
        ConsoleAppContext context,
        [Argument] string[]? runArgs = null,
        [HideDefaultValue] string? project = null,
        [HideDefaultValue] string? file = null,
        CancellationToken cancellationToken = default) =>
        RunCommand.Handle(context
                .Arguments
                .Skip(context.EscapeIndex)
                .ToArray(),
            project is { Length: > 0 }
                ? project
                : file is { Length: > 0 }
                    ? file
                    : string.Empty,
            cancellationToken);

    /// <summary>
    ///     Adds a NuGet package source with the specified name and URL to the global NuGet configuration.
    /// </summary>
    /// <param name="name">The unique name to assign to the new NuGet package source.</param>
    /// <param name="url">The URL of the NuGet feed to add (e.g., "https://api.nuget.org/v3/index.json").</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command to complete.</param>
    /// <returns>The exit code of the NuGet add operation.</returns>
    /// <remarks>
    ///     This command modifies the user-level NuGet.Config file to include the new package source,
    ///     making it available for all .NET projects on the system.
    /// </remarks>
    [Command("nuget-add")]
    public Task<int> NugetAdd(string name, string url, CancellationToken cancellationToken = default) =>
        NugetAddCommand.Handle(name, url, cancellationToken);
}
