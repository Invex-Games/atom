# Azure Storage

Registers Azure Blob Storage as the artifact provider, enabling artifact upload and download via Azure Storage
containers. Use it when artifacts must move between CI jobs through storage you control instead of the platform's
artifact mechanism.

**Package:** `Invex.Atom.Module.AzureStorage`

## Installation

```shell
dotnet add package Invex.Atom.Module.AzureStorage
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IAzureArtifactStorage
{
    // Artifact targets now use Azure Blob Storage.
}
```

`IAzureArtifactStorage` extends both `IStoreArtifact` and `IRetrieveArtifact`, so you get both upload and download
targets.

## Configuration Parameters

| Parameter                                 | CLI Arg                                         | Type   | Description                                              |
|-------------------------------------------|-------------------------------------------------|--------|----------------------------------------------------------|
| `AzureArtifactStorageContainer`           | `--azurestorage-artifact-container`             | Param  | Container name (required)                                |
| `AzureArtifactStorageConnectionString`    | `--azurestorage-artifact-connectionstring`      | Secret | Azure Storage connection string                          |
| `AzureArtifactStorageAccountUrl`          | `--azurestorage-artifact-account-url`           | Param  | Blob service URL, e.g. `https://myaccount.blob.core.windows.net` |
| `AzureArtifactStorageTenantId`            | `--azurestorage-artifact-tenant-id`             | Param  | Microsoft Entra tenant ID                                |
| `AzureArtifactStorageClientId`            | `--azurestorage-artifact-client-id`             | Param  | Client ID of the app registration or managed identity    |
| `AzureArtifactStorageClientSecret`        | `--azurestorage-artifact-client-secret`         | Secret | Client secret of the app registration (optional)         |
| `AzureArtifactStorageServiceConnectionId` | `--azurestorage-artifact-service-connection-id` | Param  | Azure DevOps workload identity service connection ID     |

Set either the connection string or the account URL. The connection string takes precedence when both are set.

## Authentication

With an account URL, the provider authenticates with a Microsoft Entra token credential, chosen in the same order as
the [Azure Key Vault](azure-key-vault.md#authentication) module:

1. Client secret (tenant ID, client ID and client secret).
2. GitHub Actions OIDC (tenant ID and client ID, with the `id-token: write` permission on the job).
3. Azure Pipelines workload identity federation (tenant ID, client ID and service connection ID, with
   `SYSTEM_ACCESSTOKEN` mapped into the step).
4. `DefaultAzureCredential` (managed identity, workload identity, Azure CLI, ...), followed by an interactive browser
   login when not running headless.

The identity needs a data-plane role on the container, such as `Storage Blob Data Contributor`.

The connection parameters are declared as used (optional) parameters of the artifact targets, so secrets among them
are prefetched and can be injected into generated workflows with `BuildOptions.Inject.Secret(...)` or
`BuildOptions.Inject.SecretFromWorkflowEnvironment(...)`.

## How It Works

The module registers an Azure Blob Storage `IArtifactProvider` as a singleton. All artifact operations (store,
retrieve, cleanup, list) go through Azure Blob Storage instead of the local file system.

This is particularly useful in CI/CD workflows where artifacts need to be shared across jobs running on different
machines.

## See also

- [Artifacts](../core-concepts/artifacts.md): producing and consuming artifacts and enabling a custom provider.
- <xref:Invex.Atom.Module.AzureStorage.IAzureArtifactStorage>
