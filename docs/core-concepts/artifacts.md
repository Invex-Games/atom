# Artifacts

Artifacts are files or directories produced by one target and consumed by another. Atom tracks artifact dependencies to
ensure correct execution order, especially in workflow scenarios where different targets may run on different machines.

## Producing Artifacts

Declare that a target produces an artifact:

```csharp
Target Pack => t => t
    .DescribedAs("Packs NuGet packages")
    .ProducesArtifact("packages")
    .Executes(async () =>
    {
        // write files to the artifact directory
    });
```

You can produce multiple artifacts:

```csharp
.ProducesArtifacts(["packages", "binaries"])
```

## Consuming Artifacts

Declare that a target consumes an artifact from another target:

```csharp
Target Deploy => t => t
    .ConsumesArtifact(nameof(Pack), "packages")
    .Executes(async () =>
    {
        // read files from the artifact directory
    });
```

Multiple artifacts from the same or different targets:

```csharp
.ConsumesArtifacts(nameof(Pack), ["packages", "binaries"])
```

## Build Slices

Artifacts can be scoped to a **build slice** — a named variation within a matrix build (e.g. a specific OS or
framework):

```csharp
.ProducesArtifact("tool", buildSlice: "windows-x64")
.ConsumesArtifact(nameof(PackTool), "tool", buildSlice: "windows-x64")
```

You can also consume an artifact across all slices:

```csharp
.ConsumesArtifact(nameof(PackTool), "tool", buildSlices: ["windows-x64", "linux-x64"])
```

## `IArtifactProvider`

When targets run on **separate CI jobs**, artifacts must be transported between machines. By default, workflows use
the CI platform's native artifact mechanism (e.g. `actions/upload-artifact` on GitHub Actions). Alternatively, you can
register an `IArtifactProvider` and enable it with the `UseCustomArtifactProvider` workflow option
(`BuildOptions.Artifacts.UseCustomProvider`), which routes artifacts through the hidden `StoreArtifact` /
`RetrieveArtifact` targets instead:

| Provider                    | Package                          | Storage            |
|-----------------------------|----------------------------------|--------------------|
| `AzureBlobArtifactProvider` | `Invex.Atom.Module.AzureStorage` | Azure Blob Storage |

Locally, artifacts are simply directories under the Atom publish/artifacts paths — no provider is required.

### Provider API

```csharp
public interface IArtifactProvider
{
    Task StoreArtifacts(IEnumerable<string> artifactNames, ...);
    Task RetrieveArtifacts(IEnumerable<string> artifactNames, ...);
    Task Cleanup(IEnumerable<string> runIdentifiers, ...);
    Task<IReadOnlyList<string>> GetStoredRunIdentifiers(...);
}
```

## Next Steps

→ [Variables](variables.md)

