# Atom

[![Validate](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml)
[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)
[![Dependabot Updates](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Why Atom?

* **Zero Context Switching**: Write build logic in C# alongside your application code.
* **Intellisense & Debugging**: Step through your build process using your IDE.
* **CI/CD Agnostic**: Define logic once; Atom generates the YAML for GitHub and Azure DevOps.
* **Modular**: Pull in capabilities via NuGet packages (GitVersion, Azure KeyVault, etc.).
* **Source Generators**: Reduces boilerplate by automatically discovering targets and parameters.

## Basic Example

1. Create a new file `Build.cs`

   ```csharp
   #:package DecSm.Atom@2.*
   
   [BuildDefinition]
   [GenerateEntryPoint]
   partial class Build : BuildDefinition
   {
       Target SayHello => t => t
           .Executes(() => Logger.LogInformation("Hello, World!"));
   }
   ```

2. Execute `dotnet run Build.cs SayHello`

   ```
   25-12-16 +10:00  DecSm.Atom.Build.BuildExecutor:
   22:46:01.754 INF Executing build
   
   SayHello
   
   25-12-16 +10:00  SayHello | Build:
   22:46:01.790 INF Hello, World!    

   Build Summary
   
     SayHello │ Succeeded │ <0.01s
   ```

## Documentation

### Getting Started

- [Introduction](docs/getting-started/introduction.md) — What Atom is and why you'd use it
- [Your First Build](docs/getting-started/your-first-build.md) — Create a minimal build and run it
- [Base vs Workflow Build](docs/getting-started/base-vs-workflow-build.md) — `BuildDefinition` vs
  `WorkflowBuildDefinition`

### Core Concepts

- [Build Definitions](docs/core-concepts/build-definitions.md) — The `[BuildDefinition]` attribute and source generators
- [Targets](docs/core-concepts/targets.md) — Defining targets with the fluent `TargetDefinition` API
- [Parameters](docs/core-concepts/parameters.md) — Declaring, requiring, and resolving parameters
- [Secrets](docs/core-concepts/secrets.md) — Secure parameter handling with `ISecretsProvider`
- [Artifacts](docs/core-concepts/artifacts.md) — Producing and consuming build artifacts
- [Variables](docs/core-concepts/variables.md) — Sharing data between targets with workflow variables
- [File System](docs/core-concepts/file-system.md) — `IAtomFileSystem`, `RootedPath`, and path providers
- [Process Runner](docs/core-concepts/process-runner.md) — Executing external processes
- [Build Info](docs/core-concepts/build-info.md) — Build ID, version, and timestamp providers
- [Build Options](docs/core-concepts/build-options.md) — Configuring build behaviour with `IBuildOption`
- [Hosting](docs/core-concepts/hosting.md) — `AtomHost`, `[GenerateEntryPoint]`, and host configuration
- [Lifecycle Hooks](docs/core-concepts/lifecycle-hooks.md) — `IAtomLifecycleHook` for pre/post-build logic
- [Logging & Reports](docs/core-concepts/logging-and-reports.md) — Spectre console output and report data
- [File Transformations](docs/core-concepts/file-transformations.md) — Temporary, reversible file edits with
  `TransformFileScope`

### Workflows

- [Overview](docs/workflows/overview.md) — What workflows add on top of a base build
- [Workflow Definitions](docs/workflows/workflow-definitions.md) — `WorkflowDefinition`, targets, and types
- [Triggers](docs/workflows/triggers.md) — Push, pull request, and manual triggers
- [Workflow Options](docs/workflows/workflow-options.md) — Checkout, deployment, conditions, and more
- [Variables in Workflows](docs/workflows/variables-in-workflows.md) — Cross-job data sharing
- [Debugging Workflows](docs/workflows/debugging-workflows.md) — Local workflow simulation

### Modules

- [Overview](docs/modules/overview.md) — What a module is and how to add one
- [.NET](docs/modules/dotnet.md) — `DecSm.Atom.Module.Dotnet`
- [GitHub Workflows](docs/modules/github-workflows.md) — `DecSm.Atom.Module.GithubWorkflows`
- [DevOps Workflows](docs/modules/devops-workflows.md) — `DecSm.Atom.Module.DevopsWorkflows`
- [Azure Key Vault](docs/modules/azure-key-vault.md) — `DecSm.Atom.Module.AzureKeyVault`
- [Azure Storage](docs/modules/azure-storage.md) — `DecSm.Atom.Module.AzureStorage`
- [GitVersion](docs/modules/git-version.md) — `DecSm.Atom.Module.GitVersion`

### Built-in Targets

- [SetupBuildInfo](docs/built-in-targets/setup-build-info.md) — Initialises build ID, version, and timestamp
- [ValidateBuild](docs/built-in-targets/validate-build.md) — Checks the build for common issues
- [GenerateWorkflowFiles](docs/built-in-targets/generate-workflow-files.md) — Generates CI/CD YAML files

### Developer Guide

- [Writing a Module](docs/developer-guide/writing-a-module.md) — Creating a reusable Atom module NuGet package
- [Custom Providers](docs/developer-guide/custom-providers.md) — Implementing `IArtifactProvider`, `ISecretsProvider`,
  and more
- [Source Generators](docs/developer-guide/source-generators.md) — How the Atom analysers and source generators work
- [Testing](docs/developer-guide/testing.md) — Using `DecSm.Atom.TestUtils`

### Reference

- [CLI](docs/reference/cli.md) — Command-line arguments and the Atom global tool
- [API Reference](api/index.md) — Auto-generated API documentation (via DocFX)

## AI Disclaimer

The Atom libraries are human-made, however GitHub Copilot completions have been used on a superficial level.

Generative AI was also used to assist in writing tests and documentation.

## License

Atom is released under the [MIT License](LICENSE.txt).