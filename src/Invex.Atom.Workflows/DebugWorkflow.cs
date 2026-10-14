namespace Invex.Atom.Workflows;

/// <summary>
///     A workflow type used for debugging workflow generation. Workflows of this type are written as JSON to the
///     <c>.debug-workflows</c> directory.
/// </summary>
[PublicAPI]
public sealed record DebugWorkflowType : IWorkflowType;

/// <summary>
///     Writes resolved workflow models as indented JSON files to the <c>.debug-workflows</c> directory,
///     allowing the output of workflow resolution to be inspected.
/// </summary>
/// <remarks>
///     Debug output is never treated as outdated, so it does not fail headless builds.
/// </remarks>
/// <param name="fileSystem">The file system used to locate the output directory and write files.</param>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class DebugWorkflowWriter(IRootedFileSystem fileSystem, ILogger<WorkflowFileWriter<DebugWorkflowType>> logger)
    : WorkflowFileWriter<DebugWorkflowType>(fileSystem, logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IRootedFileSystem _fileSystem = fileSystem;

    /// <inheritdoc />
    protected override string FileExtension => "json";

    /// <inheritdoc />
    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".debug-workflows";

    /// <inheritdoc />
    public override Task<bool> CheckForOutdatedWorkflow(
        WorkflowModel workflow,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    /// <inheritdoc />
    protected override string WriteWorkflow(WorkflowModel workflow) =>
        JsonSerializer.Serialize(workflow, JsonSerializerOptions);
}
