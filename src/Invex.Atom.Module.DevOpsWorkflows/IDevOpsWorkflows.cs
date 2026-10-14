namespace Invex.Atom.Module.DevOpsWorkflows;

/// <summary>
///     Provides integration with Azure DevOps Pipelines for Invex.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface configures the necessary services for generating
///     Azure DevOps Pipeline YAML files, providing Azure DevOps-specific workflow variables,
///     and adapting build paths and reporting when running within Azure DevOps Pipelines.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IDevOpsWorkflows : IJobRunsOn
{
    /// <summary>
    ///     Configures the host builder to add Azure DevOps Pipelines workflow services.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="DevOpsWorkflowFileWriter" /> for generating workflow files
    ///     and <see cref="DevOpsVariableProvider" /> for Azure DevOps-specific workflow variables.
    ///     When running inside Azure DevOps Pipelines, it also sets up <see cref="DevOpsSummaryOutcomeReportWriter" />
    ///     for reporting and adjusts artifact and publish paths to Azure DevOps conventions.
    /// </remarks>
    protected static partial void ConfigureHostBuilderFromIDevOpsWorkflows(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DevOpsWorkflowFileWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IVariableProvider),
            typeof(DevOpsVariableProvider),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowContextProvider),
            typeof(DevOpsWorkflowContextProvider),
            ServiceLifetime.Singleton));

        builder.Services.TryAddSingleton<DevOpsWorkflowBuilder>();

        if (DevOps.IsDevOpsPipelines)
            builder
                .Services
                .AddSingleton<IOutcomeReportWriter, DevOpsSummaryOutcomeReportWriter>()
                .ProvidePath((key, fileSystem) => DevOps.IsDevOpsPipelines
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
