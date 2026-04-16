namespace DecSm.Atom.Module.AzureKeyVault;

/// <summary>
///     Represents a workflow option to enable or disable the use of Azure Key Vault.
/// </summary>
/// <remarks>
///     When this option is enabled, the build process will attempt to connect to Azure Key Vault
///     using the configured parameters to retrieve secrets.
/// </remarks>
[PublicAPI]
public sealed record UseAzureKeyVault : ToggleWorkflowOption<UseAzureKeyVault>;
