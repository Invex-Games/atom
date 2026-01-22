namespace DecSm.Atom.Workflows.Options.Injections;

/// <summary>
///     Represents a workflow option for injecting secrets into the environment during workflow execution.
/// </summary>
/// <remarks>
///     This allows workflows to securely access sensitive configuration values through environment variables.
///     The actual secret values are not stored in this configuration object but are resolved at runtime.
/// </remarks>
/// <example>
///     <code>
/// // Inject a database connection string secret
/// var dbSecret = WorkflowSecretsEnvironmentInjection.Create("DATABASE_CONNECTION_STRING");
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition().WithAddedOptions(dbSecret);
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretsInjectionFromEnvironment(string SecretName) : IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating that multiple instances of this option are allowed.
    /// </summary>
    public bool AllowMultiple => true;
}
