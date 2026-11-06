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
    public IReadOnlyList<IBuildOption> Options =>
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

## Choosing a configuration file

By default GitVersion finds its own configuration (usually `GitVersion.yml` in the repository root). Set the
`gitversion-config` parameter to a path relative to the repository root to use another file, for example one per
product in a monorepo:

```shell
atom SetupBuildInfo --gitversion-config src/AppA/GitVersion.yml
```

In a workflow, inject it per target. Use the property name, not the argument name:

```csharp
new(nameof(SetupBuildInfo))
{
    Options = [BuildOptions.Inject.Param(nameof(IGitVersion.GitVersionConfig), "src/AppA/GitVersion.yml")],
},
```

Atom resolves the path under the repository root and runs `dotnet gitversion /config "<absolute path>" /output json`.
If the file does not exist, resolving `BuildId` / `BuildVersion` throws an error that names the parameter and the
resolved path.

The build ID and version share one GitVersion run per configuration file. The output is cached in the build project's
`obj` directory (`.gitversioncache` for the default configuration, `.gitversioncache-<hash>` for each custom one) until
the commit or the configuration file's contents change. For the default configuration, Atom hashes the first of
`GitVersion.yml`, `GitVersion.yaml`, `.GitVersion.yml` and `.GitVersion.yaml` in the repository root.

### Monorepo products

In a build that implements `IProductsDefinition`, the current product's `GitVersionConfig` is the default for
`gitversion-config`, so `--product AppA` (or the `product` injection that `WorkflowDefinition.ForProduct` and
`WorkflowPresets.ProductValidate` add) selects AppA's configuration. An explicit `--gitversion-config` still wins.

`atom gen` also generates each product's configuration file from the root one, with the product's `tag-prefix` and
`ignore.paths`, and a headless run fails if one has drifted. See
[Per-product versions](../workflows/monorepos.md#per-product-versions).

## Prerequisites

Atom installs `GitVersion.Tool` as a global .NET tool when needed. It requires a 6.x release of at least 6.8.0, and
otherwise installs the latest 6.x release (`--version 6.*`), downgrading a newer major version if one is installed.
The build fails if no supported version can be installed.

If a local tool manifest (`dotnet-tools.json`) lists `GitVersion.Tool`, `dotnet gitversion` runs that version instead,
so Atom checks it and fails if it is not a 6.x release of at least 6.8.0. Atom does not change the manifest.
To install it yourself:

```shell
dotnet tool install --global GitVersion.Tool --version 6.*
```

## See also

- [Build Info](../core-concepts/build-info.md): the providers GitVersion replaces.
- [SetupBuildInfo](../built-in-targets/setup-build-info.md): publishes the version as a variable.
- [Monorepos](../workflows/monorepos.md): one GitVersion configuration and tag prefix per product.
- <xref:Invex.Atom.Module.GitVersion.IGitVersion>
