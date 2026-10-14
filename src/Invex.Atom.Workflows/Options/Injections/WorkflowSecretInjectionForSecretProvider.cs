namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a secret needed by an <see cref="ISecretsProvider" /> itself.
/// </summary>
/// <param name="SecretName">The name of the secret parameter to inject.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.SecretForSecretProvider(name)</c> and added to the workflow's
///     options. It is injected into every step that requires secrets, so that a secrets provider that needs a
///     secret to function (for example, a vault client secret) does not have to look it up itself.
/// </remarks>
/// <example>
///     <code>
/// new WorkflowDefinition("Build")
/// {
///     Options = [BuildOptions.Inject.SecretForSecretProvider(nameof(AzureKeyVaultAppSecret))],
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretInjectionForSecretProvider(string SecretName) : IBuildOption;
