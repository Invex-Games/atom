namespace DecSm.Atom.Module.GithubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowPresetsExtensions
{
    [PublicAPI]
    public sealed class Presets
    {
        internal static Presets Instance => field ??= new();

        public WorkflowDefinition Dependabot(DependabotConfig config) =>
            new("dependabot")
            {
                WorkflowOptions = [new DependabotConfigOption(config)],
                WorkflowTypes = [WorkflowTypes.Github.Dependabot],
            };
    }

    extension(WorkflowPresets)
    {
        [PublicAPI]
        public static Presets Github => Presets.Instance;
    }
}
