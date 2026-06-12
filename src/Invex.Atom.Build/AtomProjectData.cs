namespace Invex.Atom.Build;

/// <summary>
///     Describes the Atom build project being executed, including its name and application style.
/// </summary>
[PublicAPI]
public sealed record AtomProjectData
{
    /// <summary>
    ///     The name of the project, typically derived from the entry assembly.
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    ///     Whether the application is file-based (e.g., *.cs) or project-based (e.g., *.csproj).
    /// </summary>
    public required bool IsFileBasedApp { get; init; }
}
