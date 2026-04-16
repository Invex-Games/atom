namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Registries to use for this update configuration. Can be a list of registry names or "*" for all.
/// </summary>
[UnstableAPI]
[Union]
public partial record DependabotRegistries
{
    public partial record All;

    public partial record Named(params IReadOnlyList<string> Names);
}
