# Module: Azure Storage

**Package:** `Invex.Atom.Module.AzureStorage`

Registers Azure Blob Storage as the artifact provider, enabling artifact upload and download via Azure Storage
containers.

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

| Parameter                              | CLI Arg                                    | Type   | Description                     |
|----------------------------------------|--------------------------------------------|--------|---------------------------------|
| `AzureArtifactStorageConnectionString` | `--azurestorage-artifact-connectionstring` | Secret | Azure Storage connection string |
| `AzureArtifactStorageContainer`        | `--azurestorage-artifact-container`        | Param  | Container name                  |

## How It Works

The module registers `AzureBlobArtifactProvider` as the singleton `IArtifactProvider`. All artifact operations (store,
retrieve, cleanup, list) go through Azure Blob Storage instead of the local file system.

This is particularly useful in CI/CD workflows where artifacts need to be shared across jobs running on different
machines.

