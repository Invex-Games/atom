namespace Invex.Atom.Workflows.Options.Injections;

/// <summary>
///     Represents a workflow option that injects a secret value into the workflow execution context.
/// </summary>
/// <remarks>
///     This provides a secure mechanism for injecting sensitive data like API keys or passwords into workflows.
///     Workflow platforms handle these secrets securely, masking them in logs and outputs.
/// </remarks>
/// <example>
///     <code>
/// // Inject a GitHub token secret
/// var githubTokenSecret = WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken));
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition().WithAddedOptions(githubTokenSecret);
///     </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretInjection(string Value) : IBuildOption;
