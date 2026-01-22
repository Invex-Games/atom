namespace DecSm.Atom.Workflows.Options.Injections;

/// <summary>
///     Represents a workflow option that injects an environment variable into the workflow execution context.
/// </summary>
/// <remarks>
///     This allows workflows to access custom environment variables that are not part of the default runtime environment.
///     For sensitive values, consider using <see cref="WorkflowSecretsInjectionFromEnvironment" />.
/// </remarks>
/// <example>
///     <code>
/// // Inject a build configuration environment variable
/// var buildConfig = WorkflowEnvironmentInjection.Create("BUILD_CONFIGURATION", "Release");
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition().WithAddedOptions(buildConfig);
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowParamInjectionFromEnvironment(string Value) : IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating that multiple instances of this option are allowed.
    /// </summary>
    public bool AllowMultiple => true;
}
