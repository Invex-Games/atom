namespace DecSm.Atom.Module.AzureKeyVault.Options;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class AzureKeyVaultBuildOptionsExtensions
{
    [PublicAPI]
    public sealed class AzureKeyVaultBuildOptions
    {
        public static AzureKeyVaultBuildOptions Instance => field ??= new();

        public UseAzureKeyVault UseAzureKeyVault => field ??= new();

        public UseAzureKeyVault SetUseAzureKeyVault(bool value) =>
            new()
            {
                Enabled = value,
            };
    }

    extension(BuildOptions)
    {
        public static AzureKeyVaultBuildOptions AzureKeyVault => AzureKeyVaultBuildOptions.Instance;
    }
}
