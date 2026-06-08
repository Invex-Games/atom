namespace Invex.Atom.Tool.Commands;

/// <summary>
///     Handles the execution of a Invex.Atom build project.
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
    ///     Indicates whether the most recent <see cref="Execute" /> call passed <c>--no-restore</c> to
    ///     <c>dotnet run</c> (i.e. the restore was skipped because of the restore cache). Exposed for testing.
    /// </summary>
    [UsedImplicitly(Reason = "Used in tests")]
    public static bool LastUsedNoRestore { get; private set; }

    /// <summary>
    ///     Indicates whether the most recent <see cref="Execute" /> call passed <c>--no-build</c> to
    ///     <c>dotnet run</c> (i.e. the build was skipped because of the build cache). Exposed for testing.
    /// </summary>
    [UsedImplicitly(Reason = "Used in tests")]
    public static bool LastUsedNoBuild { get; private set; }

    /// <summary>
    ///     Executes the specified Invex.Atom project.
    /// </summary>
    /// <param name="runArgs">Arguments to pass directly to the Invex.Atom project.</param>
    /// <param name="subject">The name of the Invex.Atom project or file-based app to run (e.g., "_atom").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The exit code of the executed `dotnet run` command.</returns>
    public static Task<int> Handle(string[] runArgs, string subject, CancellationToken cancellationToken) =>
        Handle(runArgs, subject, false, cancellationToken);

    /// <summary>
    ///     Executes the specified Invex.Atom project.
    /// </summary>
    /// <param name="runArgs">Arguments to pass directly to the Invex.Atom project.</param>
    /// <param name="subject">The name of the Invex.Atom project or file-based app to run (e.g., "_atom").</param>
    /// <param name="noRestoreCache">
    ///     When <c>true</c>, the restore cache is bypassed and a normal restore is always performed.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The exit code of the executed `dotnet run` command.</returns>
    public static async Task<int> Handle(
        string[] runArgs,
        string subject,
        bool noRestoreCache,
        CancellationToken cancellationToken)
    {
        LastUsedNoRestore = false;
        LastUsedNoBuild = false;

        var forceRestore = noRestoreCache || IsRestoreCacheDisabledByEnv();

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
                var knownProjectResult =
                    await FindAndExecuteKnownProject(subject, runArgs, forceRestore, cancellationToken);

                if (knownProjectResult is not null)
                    return knownProjectResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find project file '{subject}'.");

                return 1;
            }

            case SubjectInputType.File:
            {
                var knownFileResult = await FindAndExecuteKnownFile(subject, runArgs, forceRestore, cancellationToken);

                if (knownFileResult is not null)
                    return knownFileResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find cs file '{subject}'.");

                return 1;
            }

            case SubjectInputType.Either:
            {
                var eitherResult = await FindAndExecuteKnownEither(subject, runArgs, forceRestore, cancellationToken);

                if (eitherResult is not null)
                    return eitherResult.Value;

                await Console.Error.WriteLineAsync($"Error: Could not find project or cs file '{subject}'.");

                return 1;
            }

            case SubjectInputType.None:
            {
                var noneResult = await FindAndExecuteUnknown(runArgs, forceRestore, cancellationToken);

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
        bool forceRestore,
        CancellationToken cancellationToken)
    {
        if (!name.EndsWith(".csproj"))
            name = $"{name}.csproj";

        var foundPath = FileFinder.FindFile(FileSystem, FileSystem.Directory.GetCurrentDirectory(), [name], true);

        if (foundPath is not null)
            return await Execute(foundPath, runArgs, false, forceRestore, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteKnownFile(
        string name,
        string[] runArgs,
        bool forceRestore,
        CancellationToken cancellationToken)
    {
        if (!name.EndsWith(".cs"))
            name = $"{name}.cs";

        var foundPath = FileFinder.FindFile(FileSystem, FileSystem.Directory.GetCurrentDirectory(), [name], false);

        if (foundPath is not null)
            return await Execute(foundPath, runArgs, true, forceRestore, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteKnownEither(
        string name,
        string[] runArgs,
        bool forceRestore,
        CancellationToken cancellationToken)
    {
        var currentDirectory = FileSystem.Directory.GetCurrentDirectory();

        var projectPath = FileFinder.FindFile(FileSystem, currentDirectory, [name, $"{name}.csproj"], true);

        if (projectPath is not null)
            return await Execute(projectPath, runArgs, false, forceRestore, cancellationToken);

        var csPath = FileFinder.FindFile(FileSystem, currentDirectory, [name, $"{name}.cs"], false);

        if (csPath is not null)
            return await Execute(csPath, runArgs, true, forceRestore, cancellationToken);

        return null;
    }

    private static async Task<int?> FindAndExecuteUnknown(
        string[] runArgs,
        bool forceRestore,
        CancellationToken cancellationToken)
    {
        // If on nix, we want to duplicate defaultNames for case-sensitivity
        string[] defaultNames = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ["_atom", "_build", "Atom", "Build"]
            : ["_atom", "_build", "Atom", "atom", "Build", "build"];

        foreach (var name in defaultNames)
        {
            var result = await FindAndExecuteKnownEither(name, runArgs, forceRestore, cancellationToken);

            if (result is not null)
                return result;
        }

        return null;
    }

    private static async Task<int> Execute(
        IFileInfo path,
        string[] runArgs,
        bool isCsFile,
        bool forceRestore,
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

        // Decide whether the build and/or restore can be skipped based on hashes of their inputs.
        // '--no-build' implies '--no-restore', so it takes precedence when applicable.
        RestoreCache.RestoreDecision? restoreDecision = null;
        BuildCache.BuildDecision? buildDecision = null;
        var useNoBuild = false;
        var useNoRestore = false;

        if (!forceRestore)
        {
            // The build cache only applies to compiled projects, not file-based (.cs) programs.
            if (!isCsFile)
            {
                buildDecision = BuildCache.Evaluate(FileSystem, path);
                useNoBuild = buildDecision.CanSkipBuild;
            }

            if (!useNoBuild)
            {
                restoreDecision = RestoreCache.Evaluate(FileSystem, path);
                useNoRestore = restoreDecision.CanSkipRestore;
            }
        }

        if (useNoBuild)
            args.Add("--no-build");
        else if (useNoRestore)
            args.Add("--no-restore");

        LastUsedNoBuild = useNoBuild;
        LastUsedNoRestore = useNoRestore;

        args.Add("--");
        args.AddRange(sanitizedArgs);

        if (MockDotnetCli)
        {
            // Simulate a successful run: persist the hashes so the next run can skip work.
            PersistCaches(path, isCsFile, useNoBuild, useNoRestore, restoreDecision, buildDecision);

            return 0;
        }

        var atomProcess = Process.Start("dotnet", args);
        await atomProcess.WaitForExitAsync(cancellationToken);

        // If work actually ran and the build succeeded, persist the hashes so the next run can skip it.
        if (atomProcess.ExitCode is 0)
            PersistCaches(path, isCsFile, useNoBuild, useNoRestore, restoreDecision, buildDecision);

        return atomProcess.ExitCode;
    }

    private static void PersistCaches(
        IFileInfo path,
        bool isCsFile,
        bool useNoBuild,
        bool useNoRestore,
        RestoreCache.RestoreDecision? restoreDecision,
        BuildCache.BuildDecision? buildDecision)
    {
        // A restore ran unless it (or the whole build) was skipped.
        if (!useNoBuild && !useNoRestore)
            RestoreCache.Save(FileSystem, restoreDecision ?? RestoreCache.Evaluate(FileSystem, path));

        // A build ran unless it was skipped (build cache applies to compiled projects only).
        if (!useNoBuild && !isCsFile)
            BuildCache.Save(FileSystem, buildDecision ?? BuildCache.Evaluate(FileSystem, path));
    }

    private static IEnumerable<string> SanitizeArgs(IEnumerable<string> runArgs) =>
        runArgs.Select(arg => arg
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty));

    private static bool IsRestoreCacheDisabledByEnv()
    {
        var value = Environment.GetEnvironmentVariable("ATOM_NO_RESTORE_CACHE");

        return value is { Length: > 0 } &&
               !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private enum SubjectInputType
    {
        None,
        Either,
        File,
        Project,
    }
}
