namespace Invex.Atom.Build;

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
///         <c>Invex.Atom.Build.Hosting.HostExtensions</c> class.
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
///         Executing a target:
///         <code>
/// atom Build
/// </code>
///         This will lead <c>AtomService</c> to use <see cref="BuildExecutor.Execute" /> to run the "Build" target.
///         Running in headless mode (e.g., in a CI environment):
///         <code>
/// atom Build --headless
/// </code>
///         In this mode, workflow generation is typically skipped. If workflows are found to be outdated,
///         <c>AtomService</c> will raise an error, prompting regeneration.
///     </example>
///     <seealso cref="HostExtensions.AddAtom{TBuilder,TBuild}" />
/// </remarks>
internal sealed class AtomService(
    CommandLineArgs args,
    BuildExecutor executor,
    IHelpService helpService,
    IHostApplicationLifetime lifetime,
    ReportService reportService,
    IEnumerable<IAtomLifecycleHook> lifecycleHooks,
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

                throw new CommandLineException(errorMessage);
            }

            if (args.HasHelp || !args.Commands.Any())
                helpService.ShowHelp();

            if (args is { HasHelp: true } or { HasHelp: false, HasHeadless: false, Commands.Count: 0, Params.Count: 0 })
                return;

            foreach (var hook in lifecycleHooks)
                await hook.BeforeExecute(stoppingToken);

            await executor.Execute(stoppingToken);

            foreach (var hook in lifecycleHooks)
                await hook.AfterExecute(stoppingToken);
        }
        catch (CommandLineException ex)
        {
            logger.LogError("Invalid command-line arguments. {Message}", ex.Message);

            if (ex.ArgumentName is not null)
                logger.LogError("Problematic argument: {ArgumentName}", ex.ArgumentName);

            Environment.ExitCode = 1;
        }
        catch (BuildConfigurationException ex)
        {
            logger.LogCritical("Build configuration error: {Message}", ex.Message);

            if (ex.ReportData is not null)
                reportService.AddReportData(ex.ReportData);

            Environment.ExitCode = 1;
        }
        catch (StepFailedException)
        {
            // Already handled by BuildExecutor, just set exit code
            Environment.ExitCode = 1;
        }
        catch (AtomException ex)
        {
            logger.LogCritical("{Message}", ex.Message);
            Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An unexpected error occurred. Please report this issue.");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
