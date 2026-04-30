namespace DecSm.Atom.Workflows;

[PublicAPI]
public sealed record DebugWorkflowType : IWorkflowType
{
    public bool IsRunning => true;
}

[PublicAPI]
public class DebugWorkflowWriter(IAtomFileSystem atomFileSystem, ILogger<WorkflowFileWriter<DebugWorkflowType>> logger)
    : WorkflowFileWriter<DebugWorkflowType>(atomFileSystem, logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;

    protected override string FileExtension => "md";

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".debug-workflows";

    protected override string WriteWorkflow(WorkflowModel workflow) =>
        JsonSerializer.Serialize(workflow, JsonSerializerOptions);
}
