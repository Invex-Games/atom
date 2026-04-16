namespace DecSm.Atom.Workflows.Options.Injections;

/// <summary>
///     Represents a workflow option for injecting a secret value that is specifically intended for
///     <see cref="ISecretsProvider" /> implementations.
/// </summary>
/// <remarks>
///     This option is similar to <see cref="WorkflowSecretInjection" /> but prevents <see cref="ISecretsProvider" />
///     instances from attempting to look up the secret value themselves. This is crucial for scenarios where
///     the secrets provider itself requires a secret (e.g., a vault access token) to function,
///     avoiding circular dependencies or infinite loops during secret resolution.
/// </remarks>
/// <example>
///     <code>
/// // Inject a secret for an Azure Key Vault provider
/// var vaultClientSecret = WorkflowSecretsSecretInjection.Create("AZURE_CLIENT_SECRET");
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition().WithAddedOptions(vaultClientSecret);
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretInjectionForSecretProvider(string SecretName) : IWorkflowOption
{
    /// <summary>
    ///     Gets a value indicating that multiple instances of this option are allowed.
    /// </summary>
    public bool AllowMultiple => true;
}
