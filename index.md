# Atom

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Start here

- [Introduction](docs/getting-started/introduction.md): when to use Atom, key concepts, and design principles.
- [Your First Build](docs/getting-started/your-first-build.md): create and run a minimal build.
- [Targets](docs/core-concepts/targets.md): define units of work, dependencies, parameters, and artifacts.
- [Workflow Definitions](docs/workflows/workflow-definitions.md): generate GitHub Actions or Azure DevOps workflows.
- [API Reference](api/index.md): key types per package, linked into the generated reference.

## Packages

| Package                                                                                               | What it does                                                         |
|-------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------|
| [Invex.Atom.Build](https://www.nuget.org/packages/Invex.Atom.Build)                                   | Core framework: build definitions, targets, parameters, hosting.    |
| [Invex.Atom.Workflows](https://www.nuget.org/packages/Invex.Atom.Workflows)                           | Workflow definitions, triggers, options, and the `Gen` target.       |
| [Invex.Atom.Module.Dotnet](https://www.nuget.org/packages/Invex.Atom.Module.Dotnet)                   | .NET CLI wrappers and build, test, pack, publish, and NuGet helpers. |
| [Invex.Atom.Module.GithubWorkflows](https://www.nuget.org/packages/Invex.Atom.Module.GithubWorkflows) | GitHub Actions workflow and Dependabot generation.                   |
| [Invex.Atom.Module.DevopsWorkflows](https://www.nuget.org/packages/Invex.Atom.Module.DevopsWorkflows) | Azure DevOps Pipelines workflow generation.                          |
| [Invex.Atom.Module.AzureKeyVault](https://www.nuget.org/packages/Invex.Atom.Module.AzureKeyVault)     | Azure Key Vault secrets provider.                                    |
| [Invex.Atom.Module.AzureStorage](https://www.nuget.org/packages/Invex.Atom.Module.AzureStorage)       | Azure Blob Storage artifact provider.                                |
| [Invex.Atom.Module.GitVersion](https://www.nuget.org/packages/Invex.Atom.Module.GitVersion)           | GitVersion-based build ID and version.                               |
| [Invex.Atom.Tool](https://www.nuget.org/packages/Invex.Atom.Tool)                                     | The `atom` .NET global tool for running builds from any directory.   |
