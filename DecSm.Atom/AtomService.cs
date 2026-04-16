namespace DecSm.Atom;

/// <summary>
///     Represents the core background service for the Atom build framework.
///     This service orchestrates the build process based on parsed command-line arguments,
///     handling tasks such as displaying help, generating workflow files, and executing build targets.
///     It is an internal service, managed by the application host.
/// </summary>
/// <remarks>
///     <para>
///         The <c>AtomService</c> is responsible for the main lifecycle of an Atom build execution.
///         It interprets command-line arguments to determine the appropriate actions, such as:
///         <list type="bullet">
///             <item>
///                 <description>Displaying help information via <see cref="IHelpService" />.</description>
///             </item>
///             <item>
///                 <description>
///                     Generating CI/CD workflow files (e.g., GitHub Actions) using <see cref="WorkflowGenerator" /> if
///                     specified or
///                     if not running in headless mode.
///                 </description>
///             </item>
///             <item>
///                 <description>Executing the defined build targets using <see cref="BuildExecutor" />.</description>
///             </item>
///             <item>
///                 <description>Validating command-line arguments and workflow state.</description>
///             </item>
///             <item>
///                 <description>Setting the application's exit code based on the success or failure of the build.</description>
///             </item>
///         </list>
///     </para>
///     <para>
///         This service is registered and managed by the .NET Generic Host (
///         <see cref="Microsoft.Extensions.Hosting.IHost" />)
///         and is typically configured via the <c>AddAtom</c> extension method in the
///         <c>DecSm.Atom.Hosting.HostExtensions</c> class.
///     </para>
///     <example>
///         While <c>AtomService</c> is internal, its operation is primarily influenced by command-line arguments passed to
///         the Atom
///         application.
///         Here are some conceptual examples of how command-line usage translates to <c>AtomService</c> behavior:
///         Running with no arguments (or only project argument):
///         <code>
/// atom
/// # or
/// atom -p MyProject
/// </code>
///         This typically triggers workflow generation (if applicable) and displays the help information.
///         Requesting help:
///         <code>
/// atom --help
/// </code>
///         This will cause <c>AtomService</c> to invoke <see cref="IHelpService.ShowHelp" />.
///         Generating workflows:
///         <code>
/// atom --gen
/// </code>
///         This instructs <c>AtomService</c> to regenerate workflow files via
///         <see cref="WorkflowGenerator.GenerateWorkflows" />.
///         Executing a specific build target:
///         <code>
/// atom Build
/// </code>
///         This will lead <c>AtomService</c> to use <see cref="BuildExecutor.Execute" /> to run the "Build" target.
///         Running in headless mode (e.g., in a CI environment):
///         <code>
/// atom Build --headless
/// </code>
///         In this mode, workflow generation is typically skipped. If workflows are found to be "dirty" (outdated),
///         <c>AtomService</c> will raise an error, prompting regeneration.
///     </example>
///     <seealso cref="DecSm.Atom.Hosting.HostExtensions.AddAtom{TBuilder, TBuild}(TBuilder, string[])" />
/// </remarks>
internal sealed class AtomService(
    CommandLineArgs args,
    BuildExecutor executor,
    IHelpService helpService,
    WorkflowGenerator workflowGenerator,
    IHostApplicationLifetime lifetime,
    ILogger<AtomService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!args.IsValid)
            {
                var errors = args.GetValidationErrors();

                var errorMessage = string.Join(Environment.NewLine,
                    "Invalid command-line arguments:",
                    string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")),
                    "",
                    "Run with --help for usage information.");

                throw new ArgumentException(errorMessage);
            }

            if (args is { HasHelp: false, HasHeadless: false, HasGen: false, Commands.Count: 0, Params.Count: 0 })
            {
                await workflowGenerator.GenerateWorkflows(stoppingToken);
                helpService.ShowHelp();

                return;
            }

            if (args.HasHelp)
            {
                helpService.ShowHelp();

                return;
            }

            if (args.HasGen || !args.HasHeadless)
                await workflowGenerator.GenerateWorkflows(stoppingToken);
            else if (await workflowGenerator.WorkflowsDirty(stoppingToken))
                throw new InvalidOperationException(
                    "One or more workflows are out of date. To regenerate workflows, run the build with the --gen flag.");

            await executor.Execute(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stopped");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
