namespace Invex.Atom.Module.DevopsWorkflows.Workflows.Devops;

internal sealed class DevopsWorkflowFileWriter(
    IRootedFileSystem fileSystem,
    DevopsWorkflowBuilder workflowBuilder,
    ILogger<DevopsWorkflowFileWriter> logger
) : WorkflowFileWriter<DevopsWorkflowType>(fileSystem, logger)
{
    private readonly IRootedFileSystem _fileSystem = fileSystem;
    private readonly DevopsPipelineWriter _pipelineWriter = new();

    protected override string FileExtension => "yml";

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".devops" / "workflows";

    protected override string WriteWorkflow(WorkflowModel workflow)
    {
        var devopsPipeline = workflowBuilder.Build(workflow);
        _pipelineWriter.Write(devopsPipeline);

        return _pipelineWriter.TextWriter.ToString();
    }
}
