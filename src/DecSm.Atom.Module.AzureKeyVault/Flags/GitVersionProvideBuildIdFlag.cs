namespace DecSm.Atom.Module.AzureKeyVault.Flags;

[PublicAPI]
public sealed record UseAzureKeyVault(bool Enabled) : IBuildFlag;
