namespace Invex.Atom.Module.DevOpsWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class DevOpsWorkflowTypesExtensions
{
    [PublicAPI]
    public sealed class Types
    {
        internal static Types Instance => field ??= new();

        public DevOpsWorkflowType Pipeline => field ??= new();
    }

    extension(WorkflowTypes)
    {
        [PublicAPI]
        public static Types DevOps => Types.Instance;
    }
}
