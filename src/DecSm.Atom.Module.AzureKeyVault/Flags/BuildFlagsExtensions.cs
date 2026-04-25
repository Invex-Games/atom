namespace DecSm.Atom.Module.AzureKeyVault.Flags;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class BuildFlagsExtensions
{
    [PublicAPI]
    public sealed class AzureKeyVaultFlags
    {
        public static AzureKeyVaultFlags Instance => field ??= new();

        public UseAzureKeyVault UseAzureKeyVault => field ??= new();

        public UseAzureKeyVault SetUseAzureKeyVault(bool value) =>
            new()
            {
                Enabled = value,
            };
    }

    extension(BuildOptions)
    {
        public static AzureKeyVaultFlags AzureKeyVault => AzureKeyVaultFlags.Instance;
    }
}
