namespace DecSm.Atom.Module.DevopsWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class DevopsWorkflowTypesExtensions
{
    [PublicAPI]
    public sealed class Types
    {
        internal static Types Instance => field ??= new();

        public DevopsWorkflowType Pipeline => field ??= new();
    }

    extension(WorkflowTypes)
    {
        [PublicAPI]
        public static Types Devops => Types.Instance;
    }
}
