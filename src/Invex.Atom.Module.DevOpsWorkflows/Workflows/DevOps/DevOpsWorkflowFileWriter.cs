namespace Invex.Atom.Module.DevOpsWorkflows.Workflows.DevOps;

internal sealed class DevOpsWorkflowFileWriter(
    IRootedFileSystem fileSystem,
    DevOpsWorkflowBuilder workflowBuilder,
    ILogger<DevOpsWorkflowFileWriter> logger
) : WorkflowFileWriter<DevOpsWorkflowType>(fileSystem, logger)
{
    private readonly IRootedFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".devops" / "workflows";

    protected override string WriteWorkflow(WorkflowModel workflow)
    {
        var devopsPipeline = workflowBuilder.Build(workflow);

        var writer = new DevopsPipelineWriter();
        writer.Write(devopsPipeline);

        return writer.TextWriter.ToString();
    }
}
