namespace Invex.Atom.Module.GitHubWorkflows.Helpers;

/// <summary>
///     Provides integration with GitHub Actions workflows for Invex.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface configures the necessary services for generating
///     GitHub Actions workflow files, providing GitHub-specific workflow variables,
///     and adapting build paths and reporting when running within GitHub Actions.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGitHubWorkflows : IJobRunsOn
{
    /// <summary>
    ///     Configures the host builder to add GitHub Actions workflow services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="GitHubWorkflowFileWriter" /> and <see cref="DependabotConfigFileWriter" />
    ///     for generating workflow files, and <see cref="GitHubVariableProvider" /> for GitHub-specific
    ///     workflow variables. When running inside GitHub Actions, it also configures verbose logging,
    ///     sets up <see cref="GitHubSummaryOutcomeReportWriter" /> for reporting, and adjusts
    ///     artifact and publish paths to GitHub Actions conventions.
    /// </remarks>
    protected static partial void ConfigureHostBuilderFromIGitHubWorkflows(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(GitHubWorkflowFileWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotConfigFileWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IVariableProvider),
            typeof(GitHubVariableProvider),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowContextProvider),
            typeof(GitHubWorkflowContextProvider),
            ServiceLifetime.Singleton));

        if (!GitHub.IsGitHubActions)
            return;

        if (GitHub.Variables.RunnerDebug is "1")
            LogOptions.IsVerboseEnabled = true;

        builder
            .Services
            .AddSingleton<IOutcomeReportWriter, GitHubSummaryOutcomeReportWriter>()
            .ProvidePath((key, fileSystem) => GitHub.IsGitHubActions
                ? key switch
                {
                    AtomPaths.Artifacts => fileSystem.AtomRootDirectory / ".github" / "artifacts",
                    AtomPaths.Publish => fileSystem.AtomRootDirectory / ".github" / "publish",
                    _ => null,
                }
                : null);
    }
}
