namespace DecSm.Atom.Module.Dotnet.Cli;

/// <summary>
///     Represents the base interface for interacting with the .NET CLI.
/// </summary>
/// <remarks>
///     This interface is extended by generated partial interfaces to provide strongly-typed
///     methods for various .NET CLI commands (e.g., `dotnet build`, `dotnet test`).
/// </remarks>
[PublicAPI]
public partial interface IDotnetCli;

/// <summary>
///     Provides an implementation of <see cref="IDotnetCli" /> for executing .NET CLI commands.
/// </summary>
/// <param name="processRunner">The process runner used to execute external commands.</param>
/// <remarks>
///     This internal class is responsible for invoking the `dotnet` executable with the
///     appropriate arguments and handling its output.
/// </remarks>
[PublicAPI]
internal partial class DotnetCli(IProcessRunner processRunner) : IDotnetCli;
