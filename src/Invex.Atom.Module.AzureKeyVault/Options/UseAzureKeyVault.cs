namespace Invex.Atom.Module.AzureKeyVault.Options;

/// <summary>
///     A build option that enables Azure Key Vault as a secrets provider for the build.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.AzureKeyVault.UseAzureKeyVault</c>. When enabled, secret
///     parameters can be resolved from the configured Azure Key Vault (see <see cref="IAzureKeyVault" />).
/// </remarks>
[PublicAPI]
public sealed record UseAzureKeyVault : ToggleBuildOption;
