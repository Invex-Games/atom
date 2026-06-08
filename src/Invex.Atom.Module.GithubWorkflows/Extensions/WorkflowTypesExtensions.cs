namespace Invex.Atom.Module.GithubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowTypesExtensions
{
    [PublicAPI]
    public sealed class Types
    {
        internal static Types Instance => field ??= new();

        public GithubWorkflowType Action => field ??= new();

        public DependabotWorkflowType Dependabot => field ??= new();
    }

    extension(WorkflowTypes)
    {
        [PublicAPI]
        public static Types Github => Types.Instance;
    }
}
