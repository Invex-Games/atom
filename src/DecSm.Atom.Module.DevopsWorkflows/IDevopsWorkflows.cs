namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Provides integration with Azure DevOps Pipelines for DecSm.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface configures the necessary services for generating
///     Azure DevOps Pipeline YAML files, providing Azure DevOps-specific workflow variables,
///     and adapting build paths and reporting when running within Azure DevOps Pipelines.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IDevopsWorkflows : IJobRunsOn
{
    /// <summary>
    ///     Configures the host builder to add Azure DevOps Pipelines workflow services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="DevopsWorkflowWriter" /> for generating workflow files
    ///     and <see cref="DevopsVariableProvider" /> for Azure DevOps-specific workflow variables.
    ///     When running inside Azure DevOps Pipelines, it also sets up <see cref="DevopsSummaryOutcomeReportWriter" />
    ///     for reporting and adjusts artifact and publish paths to Azure DevOps conventions.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DevopsWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(DevopsVariableProvider),
            ServiceLifetime.Singleton));

        if (Devops.IsDevopsPipelines)
            builder
                .Services
                .AddSingleton<IOutcomeReportWriter, DevopsSummaryOutcomeReportWriter>()
                .ProvidePath((key, fileSystem) => Devops.IsDevopsPipelines
                    ? key switch
                    {
                        // Corresponds to $(Build.ArtifactStagingDirectory)
                        AtomPaths.Artifacts => fileSystem.AtomRootDirectory.Parent! / "a",

                        // Corresponds to $(Build.BinariesDirectory)
                        AtomPaths.Publish => fileSystem.AtomRootDirectory.Parent! / "b",

                        _ => null,
                    }
                    : null);
    }
}
