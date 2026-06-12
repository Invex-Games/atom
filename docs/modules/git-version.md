# Module: GitVersion

**Package:** `Invex.Atom.Module.GitVersion`

Integrates [GitVersion](https://gitversion.net/) to provide automatic build ID and version numbers based on your Git
history.

## Installation

```shell
dotnet add package Invex.Atom.Module.GitVersion
```

## Usage

Implement `IGitVersion` **and** enable the corresponding build options:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IGitVersion
{
    public override IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId,
        BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    // BuildId and BuildVersion are now sourced from GitVersion.
}
```

## What It Does

When `IGitVersion` is implemented, the module registers:

- `GitVersionBuildIdProvider` as `IBuildIdProvider`
- `GitVersionBuildVersionProvider` as `IBuildVersionProvider`

These replace the default providers, so `BuildId` and `BuildVersion` in your `IBuildInfo` are automatically populated
from GitVersion output.

> [!IMPORTANT]
> The providers only activate when the corresponding flag is enabled in `Options`. If `IGitVersion` is implemented but
> a flag is missing, resolving `BuildId` / `BuildVersion` throws an error telling you to enable the flag. This lets you
> opt in to GitVersion for the version, the ID, or both.

## Prerequisites

GitVersion must be available as a .NET tool. Install it globally or as a local tool:

```shell
dotnet tool install --global GitVersion.Tool
```

Configure it via `GitVersion.yml` in your repository root.

