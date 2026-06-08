namespace Invex.Atom.Module.GithubWorkflows.DependabotConfig;

/// <summary>
///     Writes Dependabot configuration files in YAML format.
/// </summary>
[PublicAPI]
public sealed class DependabotConfigFileWriter(IRootedFileSystem fileSystem, ILogger<DependabotConfigFileWriter> logger)
    : WorkflowFileWriter<DependabotWorkflowType>(fileSystem, logger)
{
    private readonly IRootedFileSystem _fileSystem = fileSystem;
    private readonly DependabotConfigWriter _configWriter = new();

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".github";

    protected override string WriteWorkflow(WorkflowModel workflow)
    {
        var config = workflow
            .Options
            .OfType<DependabotConfigOption>()
            .FirstOrDefault();

        if (config is null)
            throw new InvalidOperationException(
                $"Dependabot workflow '{workflow.Name}' is missing a {nameof(DependabotConfigOption)}.");

        _configWriter.WriteConfig(config.Config);

        return _configWriter.TextWriter.ToString();
    }
}
