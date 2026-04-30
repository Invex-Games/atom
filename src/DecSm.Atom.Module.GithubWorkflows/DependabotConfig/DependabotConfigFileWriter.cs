namespace DecSm.Atom.Module.GithubWorkflows.DependabotConfig;

/// <summary>
///     Writes Dependabot configuration files in YAML format.
/// </summary>
[PublicAPI]
public sealed class DependabotConfigFileWriter(
    IAtomFileSystem atomFileSystem,
    ILogger<DependabotConfigFileWriter> logger
) : WorkflowFileWriter<DependabotWorkflowType>(atomFileSystem, logger)
{
    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;
    private readonly DependabotConfigWriter _configWriter = new();

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".github";

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
