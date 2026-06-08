# Module: GitVersion

**Package:** `Invex.Atom.Module.GitVersion`

Integrates [GitVersion](https://gitversion.net/) to provide automatic build ID and version numbers based on your Git
history.

## Installation

```shell
dotnet add package Invex.Atom.Module.GitVersion
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IGitVersion
{
    // BuildId and BuildVersion are now sourced from GitVersion.
}
```

## What It Does

When `IGitVersion` is implemented, the module registers:

- `GitVersionBuildIdProvider` as `IBuildIdProvider`
- `GitVersionBuildVersionProvider` as `IBuildVersionProvider`

These replace the default providers, so `BuildId` and `BuildVersion` in your `IBuildInfo` are automatically populated
from GitVersion output.

## Workflow Options

Enable at the build level:

```csharp
public override IReadOnlyList<IBuildOption> Options =>
[
    BuildOptions.GitVersion.ProvideBuildId,
    BuildOptions.GitVersion.ProvideBuildVersion,
];
```

## Prerequisites

GitVersion must be available as a .NET tool. Install it globally or as a local tool:

```shell
dotnet tool install --global GitVersion.Tool
```

Configure it via `GitVersion.yml` in your repository root.

