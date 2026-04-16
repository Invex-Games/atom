namespace DecSm.Atom.Module.AzureKeyVault;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public class Options
    {
        internal static Options Instance { get; } = new();

        public UseAzureKeyVault Use =>
            new()
            {
                Value = true,
            };

        public UseAzureKeyVault DoNotUse =>
            new()
            {
                Value = false,
            };
    }

    extension(WorkflowOptions)
    {
        [PublicAPI]
        public static Options AzureKeyVault => Options.Instance;
    }
}
