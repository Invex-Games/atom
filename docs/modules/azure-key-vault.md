# Module: Azure Key Vault

**Package:** `DecSm.Atom.Module.AzureKeyVault`

Integrates Azure Key Vault as a secrets provider, allowing your build to resolve `[SecretDefinition]` parameters from a
vault.

## Installation

```shell
dotnet add package DecSm.Atom.Module.AzureKeyVault
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IAzureKeyVault
{
    // All IAzureKeyVault parameters are now available.
}
```

## Configuration Parameters

| Parameter             | CLI Arg                    | Description                              |
|-----------------------|----------------------------|------------------------------------------|
| `AzureVaultAddress`   | `--azure-vault-address`    | URI of the Azure Key Vault               |
| `AzureVaultTenantId`  | `--azure-vault-tenant-id`  | Azure AD Tenant ID                       |
| `AzureVaultAppId`     | `--azure-vault-app-id`     | App Registration Client ID               |
| `AzureVaultAppSecret` | `--azure-vault-app-secret` | App Registration Client Secret           |
| `AzureVaultAuthPort`  | `--azure-vault-auth-port`  | Local auth redirect port (default: 3421) |

These parameters can be provided via environment variables, `appsettings.json`, or the command line.

## How It Works

When `IAzureKeyVault` is implemented, the module registers:

- `AzureKeySecretsProvider` as an `ISecretsProvider` — queries the vault for secret values.
- `AzureKeyOptionsProvider` as an `IBuildOptionProvider` — contributes workflow options for injecting vault credentials
  in CI.

## Value Injection Customisation

Control how each vault parameter is injected in workflows:

```csharp
AzureKeyVaultValueInjections AzureKeyVaultValueInjections => new(
    Address: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    TenantId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AppId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    AppSecret: AzureKeyVaultValueInjectionType.Secret);
```

## Workflow Option

Enable vault integration at the build level:

```csharp
public override IReadOnlyList<IBuildOption> Options =>
[
    BuildOptions.AzureKeyVault.UseAzureKeyVault,
];
```

