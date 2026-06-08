namespace Invex.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a trigger that can initiate a workflow.
/// </summary>
/// <remarks>
///     This is a marker interface for different types of workflow triggers, such as:
///     <list type="bullet">
///         <item><see cref="GitPullRequestTrigger" />: Triggers on pull request events.</item>
///         <item><see cref="GitPushTrigger" />: Triggers on code pushes to a Git repository.</item>
///         <item><see cref="ManualTrigger" />: Allows a workflow to be triggered manually.</item>
///     </list>
///     Workflows can be configured with a list of triggers, any of which can initiate execution.
/// </remarks>
[PublicAPI]
public interface IWorkflowTrigger;
