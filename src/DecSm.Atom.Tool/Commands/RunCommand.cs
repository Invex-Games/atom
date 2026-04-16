namespace DecSm.Atom.Tool.Commands;

/// <summary>
///     Handles the execution of a DecSm.Atom build project.
/// </summary>
/// <remarks>
///     This command locates the specified Atom project in the current directory or its parent directories,
///     then executes it using `dotnet run`. It sanitizes arguments to prevent shell injection.
/// </remarks>
internal static class RunCommand
{
    [UsedImplicitly(Reason = "Used in tests")]
    public static IFileSystem FileSystem { get; set; } = new FileSystem();

    [UsedImplicitly(Reason = "Used in tests")]
    public static bool MockDotnetCli { get; set; }

    /// <summary>
    ///     Executes the specified DecSm.Atom project.
    /// </summary>
    /// <param name="runArgs">Arguments to pass directly to the DecSm.Atom project.</param>
    /// <param name="subject">The name of the DecSm.Atom project or file-based app to run (e.g., "_atom").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The exit code of the executed `dotnet run` command.</returns>
    public static async Task<int> Handle(string[] runArgs, string subject, CancellationToken cancellationToken)
    {
        subject = subject
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Trim('"', '\'', ' ');

        var subjectInputType = subject switch
        {
            var s when s.EndsWith(".cs") => SubjectInputType.File,
            var s when s.EndsWith(".csproj") => SubjectInputType.Project,
            { Length: > 0 } => SubjectInputType.Either,
            _ => SubjectInputType.None,
        };

        switch (subjectInputType)
        {
            case SubjectInputType.Project:
            {
                var knownProjectResult = await FindAndExecuteKnownProject(subject, runArgs, cancellationToken);

                if (knownProjectResult is not null)
                    return knownProjectResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find project file '{subject}'.");

                return 1;
            }

            case SubjectInputType.File:
            {
                var knownFileResult = await FindAndExecuteKnownFile(subject, runArgs, cancellationToken);

                if (knownFileResult is not null)
                    return knownFileResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find cs file '{subject}'.");

                return 1;
            }

            case SubjectInputType.Either:
            {
                var eitherResult = await FindAndExecuteKnownEither(subject, runArgs, cancellationToken);

                if (eitherResult is not null)
                    return eitherResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find project or cs file '{subject}'.");

                return 1;
            }

            case SubjectInputType.None:
            {
                var noneResult = await FindAndExecuteUnknown(runArgs, cancellationToken);

                if (noneResult is not null)
                    return noneResult.Value;

                await Console.Error.WriteLineAsync(
                    "Error: Could not find project or cs file (searched names: _atom, _build, Atom, Build).");

                return 1;
            }

            default:
                throw new UnreachableException();
        }
    }

    private static async Task<int?> FindAndExecuteKnownProject(
        string name,
        string[] runArgs,
        CancellationToken cancellationToken)
    {
        if (!name.EndsWith(".csproj"))
            name = $"{name}.csproj";

        var foundPath = FileFinder.FindFile(FileSystem, FileSystem.Directory.GetCurrentDirectory(), [name], true);

        if (foundPath is not null)
            return await Execute(foundPath, runArgs, false, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteKnownFile(
        string name,
        string[] runArgs,
        CancellationToken cancellationToken)
    {
        if (!name.EndsWith(".cs"))
            name = $"{name}.cs";

        var foundPath = FileFinder.FindFile(FileSystem, FileSystem.Directory.GetCurrentDirectory(), [name], false);

        if (foundPath is not null)
            return await Execute(foundPath, runArgs, true, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteKnownEither(
        string name,
        string[] runArgs,
        CancellationToken cancellationToken)
    {
        var currentDirectory = FileSystem.Directory.GetCurrentDirectory();

        var projectPath = FileFinder.FindFile(FileSystem, currentDirectory, [name, $"{name}.csproj"], true);

        if (projectPath is not null)
            return await Execute(projectPath, runArgs, false, cancellationToken);

        var csPath = FileFinder.FindFile(FileSystem, currentDirectory, [name, $"{name}.cs"], false);

        if (csPath is not null)
            return await Execute(csPath, runArgs, true, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteUnknown(string[] runArgs, CancellationToken cancellationToken)
    {
        // If on nix, we want to duplicate defaultNames for case-sensitivity
        string[] defaultNames = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ["_atom", "_build", "Atom", "Build"]
            : ["_atom", "_build", "Atom", "atom", "Build", "build"];

        foreach (var name in defaultNames)
        {
            var result = await FindAndExecuteKnownEither(name, runArgs, cancellationToken);

            if (result is not null)
                return result;
        }

        return null;
    }

    private static async Task<int> Execute(
        IFileInfo path,
        string[] runArgs,
        bool isCsFile,
        CancellationToken cancellationToken)
    {
        var sanitizedArgs = SanitizeArgs(runArgs);

        var args = new List<string>
        {
            "run",
        };

        if (!isCsFile)
            args.Add("--project");

        args.Add(path.FullName);

        args.Add("--");
        args.AddRange(sanitizedArgs);

        if (MockDotnetCli)
            return 0;

        var atomProcess = Process.Start("dotnet", args);
        await atomProcess.WaitForExitAsync(cancellationToken);

        return atomProcess.ExitCode;
    }

    private static IEnumerable<string> SanitizeArgs(IEnumerable<string> runArgs) =>
        runArgs.Select(arg => arg
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty));

    private enum SubjectInputType
    {
        None,
        Either,
        File,
        Project,
    }
}
