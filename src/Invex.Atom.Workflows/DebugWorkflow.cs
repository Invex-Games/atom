namespace Invex.Atom.Workflows;

[PublicAPI]
public sealed record DebugWorkflowType : IWorkflowType
{
    public bool IsRunning => true;
}

[PublicAPI]
public class DebugWorkflowWriter(IRootedFileSystem fileSystem, ILogger<WorkflowFileWriter<DebugWorkflowType>> logger)
    : WorkflowFileWriter<DebugWorkflowType>(fileSystem, logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IRootedFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "md";

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".debug-workflows";

    protected override string WriteWorkflow(WorkflowModel workflow) =>
        JsonSerializer.Serialize(workflow, JsonSerializerOptions);
}
