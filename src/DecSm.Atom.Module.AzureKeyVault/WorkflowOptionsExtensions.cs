namespace DecSm.Atom.Module.AzureKeyVault;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public class AzureKeyVaultOptions
    {
        internal static AzureKeyVaultOptions Instance { get; } = new();

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
        public static AzureKeyVaultOptions AzureKeyVault => AzureKeyVaultOptions.Instance;
    }
}
