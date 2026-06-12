namespace Invex.Atom.Module.AzureKeyVault.Options;

/// <summary>
///     Extends the <see cref="BuildOptions" /> anchor class with fluent factories for Azure Key Vault options.
/// </summary>
[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class AzureKeyVaultBuildOptionsExtensions
{
    /// <summary>
    ///     Provides factories for configuring Azure Key Vault integration.
    /// </summary>
    [PublicAPI]
    public sealed class AzureKeyVaultBuildOptions
    {
        /// <summary>
        ///     Gets the singleton instance of the options factory.
        /// </summary>
        public static AzureKeyVaultBuildOptions Instance => field ??= new();

        /// <summary>
        ///     Gets an option that enables Azure Key Vault as a secrets provider for the build.
        /// </summary>
        public UseAzureKeyVault UseAzureKeyVault => field ??= new();

        /// <summary>
        ///     Creates an option that enables or disables Azure Key Vault as a secrets provider.
        /// </summary>
        /// <param name="value"><c>true</c> to enable Azure Key Vault; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="UseAzureKeyVault" /> option.</returns>
        public UseAzureKeyVault SetUseAzureKeyVault(bool value) =>
            new()
            {
                Enabled = value,
            };
    }

    extension(BuildOptions)
    {
        /// <summary>
        ///     Gets factories for configuring Azure Key Vault integration.
        /// </summary>
        public static AzureKeyVaultBuildOptions AzureKeyVault => AzureKeyVaultBuildOptions.Instance;
    }
}
