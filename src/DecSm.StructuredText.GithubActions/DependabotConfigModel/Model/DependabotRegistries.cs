namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Registries to use for this update configuration. Can be a list of registry names or "*" for all.
/// </summary>
[PublicAPI]
[Union]
public partial record DependabotRegistries
{
    public partial record All;

    public partial record Named(params IReadOnlyList<string> Names);
}
