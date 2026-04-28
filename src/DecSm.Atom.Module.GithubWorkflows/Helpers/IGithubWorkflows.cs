namespace DecSm.Atom.Module.GithubWorkflows.Helpers;

/// <summary>
///     Provides integration with GitHub Actions workflows for DecSm.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface configures the necessary services for generating
///     GitHub Actions workflow files, providing GitHub-specific workflow variables,
///     and adapting build paths and reporting when running within GitHub Actions.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGithubWorkflows : IJobRunsOn
{
    /// <summary>
    ///     Configures the host builder to add GitHub Actions workflow services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="GithubWorkflowFileWriter" /> and <see cref="DependabotConfigFileWriter" />
    ///     for generating workflow files, and <see cref="GithubVariableProvider" /> for GitHub-specific
    ///     workflow variables. When running inside GitHub Actions, it also configures verbose logging,
    ///     sets up <see cref="GithubSummaryOutcomeReportWriter" /> for reporting, and adjusts
    ///     artifact and publish paths to GitHub Actions conventions.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(GithubWorkflowFileWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotConfigFileWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowContextProvider),
            typeof(GithubWorkflowContextProvider),
            ServiceLifetime.Singleton));

        if (!Github.IsGithubActions)
            return;

        if (Github.Variables.RunnerDebug is "1")
            LogOptions.IsVerboseEnabled = true;

        builder
            .Services
            .AddSingleton<IOutcomeReportWriter, GithubSummaryOutcomeReportWriter>()
            .ProvidePath((key, fileSystem) => Github.IsGithubActions
                ? key switch
                {
                    AtomPaths.Artifacts => fileSystem.AtomRootDirectory / ".github" / "artifacts",
                    AtomPaths.Publish => fileSystem.AtomRootDirectory / ".github" / "publish",
                    _ => null,
                }
                : null);
    }
}
