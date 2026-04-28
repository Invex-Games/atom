namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Allow or deny code execution in manifest files.
/// </summary>
[PublicAPI]
public enum InsecureExternalCodeExecution
{
    Allow,
    Deny,
}
