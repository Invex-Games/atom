namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     A workflow option that injects a secret from the workflow platform's secret store into a target's step.
/// </summary>
/// <param name="SecretName">The name of the secret parameter to inject.</param>
/// <remarks>
///     Typically created via <c>BuildOptions.Inject.Secret(name)</c>. Workflow platforms mask these secrets in logs
///     and outputs.
/// </remarks>
/// <example>
///     <code>
/// new WorkflowTargetDefinition(nameof(PushToRelease))
/// {
///     Options = [BuildOptions.Inject.Secret(nameof(GithubToken))],
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretInjection(string SecretName) : IBuildOption;
