namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a secret parameter whose value is sourced from the workflow's environment
///     (GitHub Actions <c>vars</c>, Azure DevOps variables) into every step that requires secrets.
/// </summary>
/// <param name="SecretName">The name of the secret parameter to inject.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.SecretFromWorkflowEnvironment(name)</c> and added to the
///     workflow's options.
/// </remarks>
[PublicAPI]
public sealed record WorkflowSecretInjectionFromEnvironment(string SecretName) : IBuildOption;
