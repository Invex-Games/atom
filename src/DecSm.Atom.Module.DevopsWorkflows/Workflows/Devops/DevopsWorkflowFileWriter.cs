namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

internal sealed class DevopsWorkflowFileWriter(
    IAtomFileSystem atomFileSystem,
    DevopsWorkflowBuilder workflowBuilder,
    ILogger<DevopsWorkflowFileWriter> logger
) : WorkflowFileWriter<DevopsWorkflowType>(atomFileSystem, logger)
{
    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;
    private readonly DevopsPipelineWriter _pipelineWriter = new();

    protected override string FileExtension => "yml";

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".devops" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        var devopsPipeline = workflowBuilder.Build(workflow);
        _pipelineWriter.Write(devopsPipeline);
    }
}
