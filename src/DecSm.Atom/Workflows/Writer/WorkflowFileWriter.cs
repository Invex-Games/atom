namespace DecSm.Atom.Workflows.Writer;

/// <summary>
///     An abstract base class for generating platform-specific workflow files (e.g., for GitHub Actions or Azure DevOps).
/// </summary>
/// <typeparam name="T">The workflow type this writer handles, which must implement <see cref="IWorkflowType" />.</typeparam>
/// <param name="fileSystem">The file system service for file operations.</param>
/// <param name="logger">The logger for diagnostics.</param>
/// <remarks>
///     This class provides a template for writing structured workflow files, handling file I/O, change detection,
///     and indentation. Concrete implementations must override <see cref="WriteWorkflow" /> to define the specific
///     file format and <see cref="FileExtension" /> to specify the file extension.
/// </remarks>
[PublicAPI]
public abstract class WorkflowFileWriter<T>(IAtomFileSystem fileSystem, ILogger<WorkflowFileWriter<T>> logger)
    : IWorkflowWriter<T>
    where T : IWorkflowType
{
    protected StructuredWriter Writer => field ??= new(TabSize);

    /// <summary>
    ///     Gets the number of spaces to use for each indentation level. Defaults to 2.
    /// </summary>
    protected virtual int TabSize => 2;

    /// <summary>
    ///     Gets the root directory where workflow files should be generated. Defaults to the Atom root directory.
    /// </summary>
    protected virtual RootedPath FileLocation => fileSystem.AtomRootDirectory;

    /// <summary>
    ///     Gets the file extension for the generated workflow file (e.g., "yml").
    /// </summary>
    protected abstract string FileExtension { get; }

    /// <summary>
    ///     Generates a workflow file from the provided model, writing it only if the content has changed.
    /// </summary>
    /// <param name="workflow">The workflow model to generate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = Writer.ToString();
        Writer.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath, cancellationToken)
            : string.Empty;

        if (existingText == newText)
            return;

        switch (existingText.Length)
        {
            case > 0:
                logger.LogInformation("Updating workflow file: {FilePath}", filePath);

                break;
            default:
                logger.LogInformation("Writing new workflow file: {FilePath}", filePath);

                break;
        }

        if (!fileSystem.Directory.Exists(FileLocation))
            fileSystem.Directory.CreateDirectory(FileLocation);

        await fileSystem.File.WriteAllTextAsync(filePath, newText, cancellationToken);
    }

    /// <summary>
    ///     Checks if the existing workflow file is outdated and needs to be regenerated.
    /// </summary>
    /// <param name="workflow">The workflow model to compare against the existing file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the workflow file is missing or outdated; otherwise, <c>false</c>.</returns>
    public async Task<bool> CheckForOutdatedWorkflow(
        WorkflowModel workflow,
        CancellationToken cancellationToken = default)
    {
        var filePath = FileLocation / $"{workflow.Name}.{FileExtension}";

        WriteWorkflow(workflow);

        var newText = Writer
            .ToString()
            .ReplaceLineEndings();

        Writer.Clear();

        var existingText = fileSystem.File.Exists(filePath)
            ? await fileSystem.File.ReadAllTextAsync(filePath, cancellationToken)
            : string.Empty;

        if (existingText
            .ReplaceLineEndings()
            .Equals(newText.ReplaceLineEndings(), StringComparison.CurrentCulture))
            return false;

        logger.LogInformation(
            "Workflow file is outdated and needs to be regenerated: {FilePath}\nExisting:\n{Existing}\nNew:\n{New}",
            filePath,
            existingText,
            newText);

        return true;
    }

    /// <summary>
    ///     When overridden in a derived class, writes the content of the workflow file using the provided helper methods.
    /// </summary>
    /// <param name="workflow">The workflow model to write.</param>
    protected abstract void WriteWorkflow(WorkflowModel workflow);

    protected static TOption? GetOption<TOption>(WorkflowModel workflow)
        where TOption : IWorkflowOption =>
        workflow
            .Options
            .OfType<TOption>()
            .LastOrDefault();

    protected static TOption? GetOption<TOption>(WorkflowModel workflow, WorkflowStepModel step)
        where TOption : IWorkflowOption =>
        step
            .Options
            .Concat(workflow.Options)
            .OfType<TOption>()
            .LastOrDefault();

    protected static IEnumerable<TOption> GetOptions<TOption>(WorkflowModel workflow)
        where TOption : IWorkflowOption =>
        workflow.Options.OfType<TOption>();

    protected static IEnumerable<TOption> GetOptions<TOption>(WorkflowModel workflow, WorkflowStepModel step)
        where TOption : IWorkflowOption =>
        workflow
            .Options
            .Concat(step.Options)
            .OfType<TOption>();

    protected static bool IsOptionEnabled<TOption>(WorkflowModel workflow)
        where TOption : ToggleWorkflowOption<TOption> =>
        workflow
            .Options
            .OfType<TOption>()
            .LastOrDefault() is { Value: true };

    protected static bool IsOptionEnabled<TOption>(WorkflowModel workflow, WorkflowStepModel step)
        where TOption : ToggleWorkflowOption<TOption> =>
        workflow
            .Options
            .Concat(step.Options)
            .OfType<TOption>()
            .LastOrDefault() is { Value: true };
}
