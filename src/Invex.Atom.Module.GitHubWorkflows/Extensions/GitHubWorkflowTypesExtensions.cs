namespace Invex.Atom.Module.GitHubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class GitHubWorkflowTypesExtensions
{
    [PublicAPI]
    public sealed class Types
    {
        internal static Types Instance => field ??= new();

        public GitHubWorkflowType Action => field ??= new();

        public DependabotWorkflowType Dependabot => field ??= new();
    }

    extension(WorkflowTypes)
    {
        [PublicAPI]
        public static Types GitHub => Types.Instance;
    }
}
