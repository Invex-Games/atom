# GitVersion

Integrates [GitVersion](https://gitversion.net/) to provide automatic build ID and version numbers based on your Git
history. Use it when versions should come from tags and branches instead of `Directory.Build.props`.

**Package:** `Invex.Atom.Module.GitVersion`

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

The providers install and run a pinned version of the `GitVersion.Tool` global .NET tool (currently `7.0.0`), so the
build doesn't break when a new GitVersion major version changes its command-line arguments or output. If a different
version is installed globally, it is replaced with the pinned version the first time GitVersion is needed.

Configure it via `GitVersion.yml` in your repository root, using the configuration format of the pinned GitVersion
version.

> [!IMPORTANT]
> GitVersion 7 nests settings under `calculation:` and `output:` and rejects the flat GitVersion 6 layout. It also
> renames the pre-release variables: `PreReleaseTag` is now `PreReleaseLabel`, and the old `PreReleaseLabel` is now
> `PreReleaseLabelName`. Migrate a GitVersion 6 configuration with `dotnet gitversion config migrate --in-place`, then
> update any format strings that still use the old names, such as `{PreReleaseTagWithDash}` (now
> `{PreReleaseLabelWithDash}`). The migration drops comments from the file.

## See also

- [Build Info](../core-concepts/build-info.md): the providers GitVersion replaces.
- [SetupBuildInfo](../built-in-targets/setup-build-info.md): publishes the version as a variable.
- <xref:Invex.Atom.Module.GitVersion.IGitVersion>
