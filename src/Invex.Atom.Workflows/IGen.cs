namespace Invex.Atom.Workflows;

/// <summary>
///     Provides the <see cref="Gen" /> target, which generates workflow files for the configured CI/CD platforms.
/// </summary>
/// <remarks>
///     Workflow generation also runs automatically before target execution when the build is not in headless mode
///     (see <see cref="WorkflowOutdatedException" /> for the headless-mode behavior).
/// </remarks>
[PublicAPI]
public interface IGen : IBuildAccessor
{
    /// <summary>
    ///     Gets the build target that generates workflow files from the workflows defined in
    ///     <see cref="IWorkflowBuildDefinition.Workflows" />.
    /// </summary>
    /// <example>
    ///     <code>
    /// dotnet run --project _atom Gen
    ///     </code>
    /// </example>
    Target Gen => t => t.DescribedAs("Generates workflow files");
}
