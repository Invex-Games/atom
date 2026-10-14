namespace Invex.Atom.Module.GitHubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowPresetsExtensions
{
    [PublicAPI]
    public sealed class Presets
    {
        internal static Presets Instance => field ??= new();

        public WorkflowDefinition Dependabot(
            StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig config) =>
            new("dependabot")
            {
                Options = [new DependabotConfigOption(config)],
                Types = [WorkflowTypes.GitHub.Dependabot],
            };
    }

    extension(WorkflowPresets)
    {
        [PublicAPI]
        public static Presets GitHub => Presets.Instance;
    }
}
