# Modules Overview

A **module** is a NuGet package that adds reusable functionality to an Atom build — targets, parameters, service
registrations, or workflow options.

## How Modules Work

Modules are typically distributed as interfaces. Your build definition implements the interface, and the source
generator + `[ConfigureHostBuilder]` attribute wire everything up automatically:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IGitVersion, IAzureKeyVault
{
    // All targets and parameters from IGitVersion and IAzureKeyVault
    // are automatically available.
}
```

## Adding a Module

1. Install the NuGet package:

   ```shell
   dotnet add package Invex.Atom.Module.GitVersion
   ```

2. Implement the module's interface on your build class:

   ```csharp
   internal partial class Build : BuildDefinition, IGitVersion { }
   ```

3. That's it. The module's services are registered, its targets are discoverable, and its parameters appear in help
   output.

## First-Party Modules

| Package                             | Interface               | Description                                        |
|-------------------------------------|-------------------------|----------------------------------------------------|
| `Invex.Atom.Module.Dotnet`          | *(various)*             | .NET CLI helpers (build, test, pack, publish)      |
| `Invex.Atom.Module.GithubWorkflows` | `IGithubWorkflows`      | GitHub Actions workflow writer and helpers         |
| `Invex.Atom.Module.DevopsWorkflows` | `IDevopsWorkflows`      | Azure DevOps Pipelines workflow writer and helpers |
| `Invex.Atom.Module.AzureKeyVault`   | `IAzureKeyVault`        | Azure Key Vault secrets provider                   |
| `Invex.Atom.Module.AzureStorage`    | `IAzureArtifactStorage` | Azure Blob Storage artifact provider               |
| `Invex.Atom.Module.GitVersion`      | `IGitVersion`           | GitVersion-based build ID and version providers    |

## Module Conventions

- Modules expose their functionality through **interfaces** that extend `IBuildAccessor`.
- Targets are defined as `Target` properties on the interface with default implementations.
- Parameters are declared with `[ParamDefinition]` or `[SecretDefinition]`.
- Service registration uses the `[ConfigureHostBuilder]` attribute with a static partial
  `ConfigureBuilderFrom{InterfaceName}` method.
- Build options are extended via the static `BuildOptions` class using extension-like patterns.

## Next Steps

→ Individual module
pages: [.NET](dotnet.md) · [GitHub Workflows](github-workflows.md) · [DevOps Workflows](devops-workflows.md) · [Azure Key Vault](azure-key-vault.md) · [Azure Storage](azure-storage.md) · [GitVersion](git-version.md)

