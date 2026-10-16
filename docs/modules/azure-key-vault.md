# Azure Key Vault

Integrates Azure Key Vault as a secrets provider, allowing your build to resolve `[SecretDefinition]` parameters from a
vault. Use it when your secrets live in Azure Key Vault rather than in CI secret stores or user secrets.

**Package:** `Invex.Atom.Module.AzureKeyVault`

## Installation

```shell
dotnet add package Invex.Atom.Module.AzureKeyVault
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

| Parameter                          | CLI Arg                                   | Type   | Description                                                    |
|------------------------------------|-------------------------------------------|--------|----------------------------------------------------------------|
| `AzureKeyVaultAddress`             | `--azure-key-vault-address`               | Param  | URI of the Azure Key Vault                                     |
| `AzureKeyVaultTenantId`            | `--azure-key-vault-tenant-id`             | Param  | Microsoft Entra tenant ID                                      |
| `AzureKeyVaultClientId`            | `--azure-key-vault-client-id`             | Param  | Client ID of the app registration or managed identity          |
| `AzureKeyVaultClientSecret`        | `--azure-key-vault-client-secret`         | Secret | Client secret of the app registration (optional)               |
| `AzureKeyVaultServiceConnectionId` | `--azure-key-vault-service-connection-id` | Param  | Azure DevOps workload identity service connection ID           |
| `AzureKeyVaultAuthPort`            | `--azure-key-vault-auth-port`             | Param  | Local interactive login redirect port (default: 3421)          |
| `AzureKeyVaultTimeout`             | `--azure-key-vault-timeout`               | Param  | Per-secret timeout in seconds (default: 120, 0 disables)       |

These parameters can be provided via environment variables, `appsettings.json`, the command line or another secrets
provider (such as .NET user secrets). They are never looked up in the vault itself.

## Authentication

The vault is accessed with a Microsoft Entra token credential, chosen in this order:

1. **Client secret** — tenant ID, client ID and client secret are all set.
2. **GitHub Actions OIDC** — tenant ID and client ID are set and the job can request an ID token
   (`ACTIONS_ID_TOKEN_REQUEST_URL` is present). Add a federated credential for the repository to the app registration
   and grant the job `id-token: write`:

   ```csharp
   new(nameof(MyTarget))
   {
       Options = [new GitHubTokenPermissionsOption(new PermissionsEvent { IdTokens = PermissionsLevel.Write })],
   }
   ```

3. **Azure Pipelines workload identity federation** — tenant ID, client ID and service connection ID are set and
   `SYSTEM_OIDCREQUESTURI` and `SYSTEM_ACCESSTOKEN` are available. `SYSTEM_ACCESSTOKEN` is not exposed to scripts by
   default; map it into the target step:

   ```csharp
   new(nameof(MyTarget))
   {
       Options =
       [
           BuildOptions.Inject.EnvironmentVariable("SYSTEM_ACCESSTOKEN",
               Tex.Variable("System.AccessToken")),
       ],
   }
   ```

4. **`DefaultAzureCredential`** — environment variables, workload identity (for example AKS), managed identity, Azure
   CLI (including after `azure/login` or the `AzureCLI` task), Azure Developer CLI, and so on. The tenant ID and client
   ID are passed on when set. When the build is not running headless, an interactive browser login follows as a last
   resort.

Empty values and unexpanded Azure DevOps macros (such as `$(AZURE_KEY_VAULT_CLIENT_SECRET)` when the variable is not
defined) are treated as unset, so the injected-but-empty client secret does not prevent OIDC from being used.

If authentication fails or a request times out, the provider logs a warning and stops querying the vault for the rest
of the run, so other secrets providers and defaults are used instead.

## How It Works

When `IAzureKeyVault` is implemented, the module registers:

- An `ISecretsProvider` that queries the vault for secret values asynchronously. Secrets used by the targets being run
  are prefetched before execution with the build's cancellation token; each request is also bounded by
  `AzureKeyVaultTimeout`.
- An `IBuildOptionProvider` that contributes workflow options for injecting the vault connection parameters in CI.

## Value Injection Customisation

Control how each vault parameter is injected in workflows:

```csharp
AzureKeyVaultValueInjections AzureKeyVaultValueInjections => new(
    Address: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    TenantId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    ClientId: AzureKeyVaultValueInjectionType.EnvironmentVariable,
    ClientSecret: AzureKeyVaultValueInjectionType.Secret,
    ServiceConnectionId: AzureKeyVaultValueInjectionType.None);
```

`EnvironmentVariable` reads from workflow variables (`vars.AZURE_KEY_VAULT_ADDRESS` in GitHub Actions,
`$(AZURE_KEY_VAULT_ADDRESS)` in Azure DevOps); `Secret` reads from the platform's secret store. The values above are
the defaults. Set `ClientSecret` to `None` when you only use federated credentials, and `ServiceConnectionId` to
`EnvironmentVariable` for Azure Pipelines workload identity federation.

## Workflow Option

Enable vault integration at the build level:

```csharp
public override IReadOnlyList<IBuildOption> Options =>
[
    BuildOptions.AzureKeyVault.UseAzureKeyVault,
];
```

## See also

- [Secrets](../core-concepts/secrets.md): how secrets are declared and resolved.
- [Azure Storage](azure-storage.md): uses the same credential selection for artifact storage.
- <xref:Invex.Atom.Module.AzureKeyVault.IAzureKeyVault>
- <xref:Invex.Atom.Module.AzureKeyVault.AzureKeyVaultValueInjections>
