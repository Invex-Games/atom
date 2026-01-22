namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides integration with GitHub Actions workflows for DecSm.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface configures the necessary services for generating
///     GitHub Actions workflow files, providing GitHub-specific workflow variables,
///     and adapting build paths and reporting when running within GitHub Actions.
/// </remarks>
[ConfigureHostBuilder]
public partial interface IGithubWorkflows : IJobRunsOn
{
    /// <summary>
    ///     Configures the host builder to add GitHub Actions workflow services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="GithubWorkflowWriter" /> and <see cref="DependabotWorkflowWriter" />
    ///     for generating workflow files, and <see cref="GithubVariableProvider" /> for GitHub-specific
    ///     workflow variables. When running inside GitHub Actions, it also configures verbose logging,
    ///     sets up <see cref="GithubSummaryOutcomeReportWriter" /> for reporting, and adjusts
    ///     artifact and publish paths to GitHub Actions conventions.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowExpressionWriter),
            typeof(GithubExpressionWriter),
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
