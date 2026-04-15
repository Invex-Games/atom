namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Allow or deny code execution in manifest files.
/// </summary>
[UnstableAPI]
public enum InsecureExternalCodeExecution
{
    Allow,
    Deny,
}
