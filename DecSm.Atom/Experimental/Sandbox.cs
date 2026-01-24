namespace DecSm.Atom.Experimental;

[UnstableAPI]
public sealed record CustomAtomCommand : IWorkflowOption
{
    public string Write(
        WorkflowModel workflow,
        WorkflowStepModel workflowStep,
        IAtomFileSystem fileSystem,
        string customArgs)
    {
        var cacheHitCommand =
            $"format('.atom/{{0}} {customArgs}-- {workflowStep.Name} --skip --headless', runner.os == 'windows' && '_atom.exe' || '_atom')";

        string cacheMissCommand;

        if (fileSystem.IsFileBasedApp)
        {
            if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                throw new InvalidOperationException("EntryPointFilePath is null");

            var filePathRelativeToRoot =
                fileSystem.FileSystem.Path.GetRelativePath(fileSystem.AtomRootDirectory, fileName);

            cacheMissCommand =
                $"'dotnet run --file {filePathRelativeToRoot} {customArgs}-- {workflowStep.Name} --skip --headless'";
        }
        else
        {
            var projectPath = FindProjectPath(fileSystem, fileSystem.ProjectName);

            cacheMissCommand =
                $"'dotnet run --project {projectPath} {customArgs}-- {workflowStep.Name} --skip --headless'";
        }

        // return $"run: ${{{{ steps.cache-restore-atom-build.outputs.cache-hit == 'true' }}}}";

        const string cacheHitVar = "steps.cache-restore-atom-build.outputs.cache-hit";

        return $$$"""
                  run: ${{ {{{cacheHitVar}}} && {{{cacheHitCommand}}} || {{{cacheMissCommand}}} }}
                  """;
    }

    private static string FindProjectPath(IAtomFileSystem fileSystem, string projectName)
    {
        var projectPath = fileSystem
            .FileSystem
            .DirectoryInfo
            .New(fileSystem.AtomRootDirectory)
            .EnumerateFiles("*.csproj",
                new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    MaxRecursionDepth = 4,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false,
                })
            .FirstOrDefault(f => f.Name.Equals($"{projectName}.csproj", StringComparison.OrdinalIgnoreCase));

        if (projectPath?.FullName is null)
            throw new InvalidOperationException($"Project '{projectName}' not found in current directory.");

        return fileSystem
            .FileSystem
            .Path
            .GetRelativePath(fileSystem.AtomRootDirectory, projectPath.FullName)
            .Replace("\\", "/");
    }
}

[UnstableAPI]
public sealed record RunTargetStepIf(WorkflowExpression Condition) : IWorkflowOption;

[UnstableAPI]
public sealed record CleanAtomDirectory : IWorkflowOption;
