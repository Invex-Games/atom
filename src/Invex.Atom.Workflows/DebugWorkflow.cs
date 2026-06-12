namespace Invex.Atom.Workflows;

/// <summary>
///     A workflow type used for debugging workflow generation; it is always considered to be running.
/// </summary>
[PublicAPI]
public sealed record DebugWorkflowType : IWorkflowType
{
    /// <inheritdoc />
    public bool IsRunning => true;
}

/// <summary>
///     Writes resolved workflow models as indented JSON files to the <c>.debug-workflows</c> directory,
///     allowing the output of workflow resolution to be inspected.
/// </summary>
/// <param name="fileSystem">The file system used to locate the output directory and write files.</param>
/// <param name="logger">The logger for diagnostics.</param>
[PublicAPI]
public class DebugWorkflowWriter(IRootedFileSystem fileSystem, ILogger<WorkflowFileWriter<DebugWorkflowType>> logger)
    : WorkflowFileWriter<DebugWorkflowType>(fileSystem, logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IRootedFileSystem _fileSystem = fileSystem;

    /// <inheritdoc />
    protected override string FileExtension => "md";

    /// <inheritdoc />
    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".debug-workflows";

    /// <inheritdoc />
    protected override string WriteWorkflow(WorkflowModel workflow) =>
        JsonSerializer.Serialize(workflow, JsonSerializerOptions);
}
