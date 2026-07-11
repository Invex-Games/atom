# Atom

[![Validate](https://github.com/Invex-Games/atom/actions/workflows/Validate.yml/badge.svg)](https://github.com/Invex-Games/atom/actions/workflows/Validate.yml)
[![Build](https://github.com/Invex-Games/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/Invex-Games/atom/actions/workflows/Build.yml)
[![Dependabot Updates](https://github.com/Invex-Games/atom/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/Invex-Games/atom/actions/workflows/dependabot/dependabot-updates)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Why Atom?

### Zero Context Switching

Write build logic in C# alongside your application code.

### Intellisense & Debugging

Step through your build process using your IDE.

### CI/CD Agnostic

Define logic once; Atom generates the YAML for GitHub and Azure DevOps.

### Modular

Pull in capabilities via NuGet packages (GitVersion, Azure KeyVault, etc.).

### Source Generators

Reduces boilerplate by automatically discovering targets and parameters.

## Examples

### Hello World

> [!NOTE]
>
> It is recommended to use the atom dotnet tool to invoke atom projects:
>
> `dotnet tool install -g Invex.Atom.Tool`
>
> ` atom ...`
>
> However, the dotnet cli can also be used directly:
>
> `dotnet run -- ...`

1. Create a .NET 10 project

   ```
   dotnet new console -n _atom
   ```

2. Update the `_atom.csproj` file:

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Worker">
    
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <RootNamespace>Atom</RootNamespace>
      </PropertyGroup>
    
      <ItemGroup>
        <PackageReference Include="Invex.Atom.Build" Version="3.*" />
      </ItemGroup>
    
    </Project>
    ```

3. Replace the `Program.cs` file with `IBuild.cs`:

    ```csharp
    using Invex.Atom.Build.Definition;
    using Invex.Atom.Build.Hosting;
    
    namespace Atom;
    
    [BuildDefinition]
    [GenerateEntryPoint]
    internal interface IBuild : IBuildDefinition
    {
        Target HelloWorld =>
            t => t
            .DescribedAs("Prints a hello world message to the console")
            .Executes(() => Logger.LogInformation("Hello, World!"));
    }
    ```

4. Execute `atom HelloWorld`

    ```
    26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
    01:16:47.552 INF Executing build
    
    HelloWorld
    
    Prints a hello world message to the console
    
    26-06-06 +10:00  HelloWorld | Atom.Build:      
    01:16:47.616 INF Hello, World!
    
    
    Build Summary
    
    HelloWorld │ Succeeded │ <0.01s
   ```

### Adding Params

1. Add a parameter to the `IBuild` interface:

   > [!NOTE]
   >
   > `.RequiresParam(nameof(...))` is used to ensure the parameter is provided before the target is executed.
   >
   > `.UsesParam(nameof(...))` is used to use the parameter if it is provided, but not fail the build if it is not.

    ```csharp
    using Invex.Atom.Build.Definition;
    using Invex.Atom.Build.Hosting;
    using Invex.Atom.Build.Params;
    
    namespace Atom;
    
    [BuildDefinition]
    [GenerateEntryPoint]
    internal interface IBuild : IBuildDefinition
    {
        [ParamDefinition("my-name", "My name")]
        string? MyName => GetParam(() => MyName);
    
        Target HelloWorld =>
            t => t
                .DescribedAs("Prints a hello world message to the console")
                .RequiresParam(nameof(MyName))
                .Executes(() => Logger.LogInformation("Hello, World! I am {MyName}.", MyName));
    }
    ```

2. Execute `atom HelloWorld --my-name Frodo

    ```
    26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
    01:23:25.405 INF Executing build

    HelloWorld
    
    Prints a hello world message to the console
    
    26-06-06 +10:00  HelloWorld | Atom.Build:
    01:23:25.467 INF Hello, World! I am Frodo.

    
    Build Summary

      HelloWorld │ Succeeded │ <0.01s
    ```

### Adding Secrets

1. Add a secret parameter to the `IBuild` interface:

    ```csharp
    using Invex.Atom.Build.Definition;
    using Invex.Atom.Build.Hosting;
    using Invex.Atom.Build.Params;
    
    namespace Atom;
    
    [BuildDefinition]
    [GenerateEntryPoint]
    internal interface IBuild : IBuildDefinition
    {
        [ParamDefinition("my-name", "My name")]
        string? MyName => GetParam(() => MyName);
    
        [SecretDefinition("my-secret", "My secret")]
        string? MySecret => GetParam(() => MySecret);
    
        Target HelloWorld =>
            t => t
                .DescribedAs("Prints a hello world message to the console")
                .RequiresParam(nameof(MyName))
                .RequiresParam(nameof(MySecret))
                .Executes(() =>
                    Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.",
                        MyName,
                        MySecret));
    }
    ```

2. Execute `atom HelloWorld --my-name Frodo --my-secret TheOneRing`

   > [!NOTE]
   > The secret is masked in the output.

    ```
    26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
    01:27:08.482 INF Executing build                
                                                    
    HelloWorld
    
    Prints a hello world message to the console
    
    26-06-06 +10:00  HelloWorld | Atom.Build:                        
    01:27:08.544 INF Hello, World! I am Frodo and my secret is *****.
                                                                     
    
    Build Summary
                                         
      HelloWorld │ Succeeded │ <0.01s
    ```

### Adding Target Dependencies

1. Update the `IBuild` interface:

    ```csharp
    using Invex.Atom.Build.Definition;
    using Invex.Atom.Build.Hosting;
    using Invex.Atom.Build.Params;
    
    namespace Atom;
    
    [BuildDefinition]
    [GenerateEntryPoint]
    internal interface IBuild : IBuildDefinition
    {
        [ParamDefinition("my-name", "My name")]
        string? MyName => GetParam(() => MyName);
    
        [SecretDefinition("my-secret", "My secret")]
        string? MySecret => GetParam(() => MySecret);
    
        Target HelloWorld =>
            t => t
                .DescribedAs("Prints a hello world message to the console")
                .RequiresParam(nameof(MyName))
                .RequiresParam(nameof(MySecret))
                .Executes(() =>
                    Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.", MyName, MySecret));
    
        Target Goodbye =>
            t => t
                .DescribedAs("Prints a goodbye message to the console")
                .DependsOn(nameof(HelloWorld))
                .Executes(() => Logger.LogInformation("Goodbye!"));
    }
    ```

2. Execute `atom Goodbye --my-name Frodo --my-secret TheOneRing`

   > [!NOTE]
   > The `Goodbye` target depends on the `HelloWorld` target, so both will be executed in the correct order, and the
   parameters only need to be provided once.

    ```
    26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
    01:30:12.175 INF Executing build                
                                                    
    HelloWorld
    
    Prints a hello world message to the console
    
    26-06-06 +10:00  HelloWorld | Atom.Build:                        
    01:30:12.235 INF Hello, World! I am Frodo and my secret is *****.
                                                                     
    Goodbye
    
    Prints a goodbye message to the console
    
    26-06-06 +10:00  Goodbye | Atom.Build:
    01:30:12.240 INF Goodbye!             
                                          
    
    Build Summary
                                         
      HelloWorld │ Succeeded │ <0.01s    
      Goodbye    │ Succeeded │ <0.01s
    ```

### Adding Workflow Generation

1. Update the `_atom.csproj` file:

    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Worker">
    
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <RootNamespace>Atom</RootNamespace>
      </PropertyGroup>
    
      <ItemGroup>
        <PackageReference Include="Invex.Atom.Module.GithubWorkflows" Version="3.*" />
      </ItemGroup>
    
    </Project>
    ```
2. Update the `IBuild` interface:

    ```csharp
    using Invex.Atom.Build.BuildOptions;
    using Invex.Atom.Build.Definition;
    using Invex.Atom.Build.Hosting;
    using Invex.Atom.Build.Params;
    using Invex.Atom.Module.GithubWorkflows.Extensions;
    using Invex.Atom.Module.GithubWorkflows.Helpers;
    using Invex.Atom.Workflows;
    using Invex.Atom.Workflows.Definition;
    using Invex.Atom.Workflows.Definition.Triggers;
    using Invex.Atom.Workflows.Options;
    using Invex.StructuredText.Expressions;
    
    namespace Atom;
    
    [BuildDefinition]
    [GenerateEntryPoint]
    internal interface IBuild : IWorkflowBuildDefinition, IGithubWorkflows
    {
    [ParamDefinition("my-name", "My name")]
    string? MyName => GetParam(() => MyName);
    
        [SecretDefinition("my-secret", "My secret")]
        string? MySecret => GetParam(() => MySecret);
    
        Target HelloWorld =>
            t => t
                .DescribedAs("Prints a hello world message to the console")
                .RequiresParam(nameof(MyName))
                .RequiresParam(nameof(MySecret))
                .Executes(() =>
                    Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.", MyName, MySecret));
    
        Target Goodbye =>
            t => t
                .DescribedAs("Prints a goodbye message to the console")
                .DependsOn(nameof(HelloWorld))
                .Executes(() => Logger.LogInformation("Goodbye!"));
    
        IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
        [
            new("Hello")
            {
                Triggers = [WorkflowTriggers.PushToMain],
                Targets =
                [
                    new(nameof(HelloWorld))
                    {
                        Options =
                        [
                            BuildOptions.Inject.Param(nameof(MyName), TextExpressions.Github.GithubRepositoryOwner),
                            BuildOptions.Inject.Secret(nameof(MySecret)),
                        ],
                    },
                    new(nameof(Goodbye)),
                ],
                Types = [WorkflowTypes.Github.Action],
            },
        ];
    }
    ```

3. Execute `atom gen`

    ```
    26-06-06 +10:00  Invex.Atom.Module.GithubWorkflows.GithubActions.GithubWorkflowFileWriter:                                               
    01:38:57.388 INF Writing new workflow file:                                  
                     L:\Repos\Invex-Games\atom\.github\workflows\Hello.yml             
                                                                                 
    
    26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
    01:38:57.472 INF Executing build                
                                                    
    Gen
    
    Generates workflow files
    
    
    Build Summary
                                     
      Gen    │ Succeeded │ <0.01s
    ```

   `.github/workflows/Hello.yml`

    ```yaml
    name: Hello
    
    on:
      push:
        branches: [ main ]
    
    permissions: { }
    
    jobs:
    
      HelloWorld:
        runs-on: ubuntu-latest
        steps:
    
          - name: Checkout
            uses: actions/checkout@v6
            with:
              fetch-depth: 0
    
          - name: HelloWorld
            id: HelloWorld
            run: dotnet run --project _atom/_atom.csproj -- HelloWorld --skip --headless
            env:
              my-secret: ${{ secrets.MY_SECRET }}
              my-name: ${{ github.repository_owner }}
    
      Goodbye:
        needs: [ HelloWorld ]
        runs-on: ubuntu-latest
        steps:
    
          - name: Checkout
            uses: actions/checkout@v6
            with:
              fetch-depth: 0
    
          - name: Goodbye
            id: Goodbye
            run: dotnet run --project _atom/_atom.csproj -- Goodbye --skip --headless
    ```

### File-based Apps

Atom can also be used as a file-based app:

`Atom.cs`

```csharp
#:sdk Microsoft.NET.Sdk.Worker
#:package Invex.Atom.Build@3.*

using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;

namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .Executes(() => Logger.LogInformation("Hello, World!"));
}
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
- [File System](docs/core-concepts/file-system.md) — `IRootedFileSystem`, `RootedPath`, and path providers
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
- [.NET](docs/modules/dotnet.md) — `Invex.Atom.Module.Dotnet`
- [GitHub Workflows](docs/modules/github-workflows.md) — `Invex.Atom.Module.GithubWorkflows`
- [DevOps Workflows](docs/modules/devops-workflows.md) — Azure Pipelines generation, pools, PR controls, and concurrency
- [Azure Key Vault](docs/modules/azure-key-vault.md) — `Invex.Atom.Module.AzureKeyVault`
- [Azure Storage](docs/modules/azure-storage.md) — `Invex.Atom.Module.AzureStorage`
- [GitVersion](docs/modules/git-version.md) — `Invex.Atom.Module.GitVersion`

### Built-in Targets

- [SetupBuildInfo](docs/built-in-targets/setup-build-info.md) — Initialises build ID, version, and timestamp
- [ValidateBuild](docs/built-in-targets/validate-build.md) — Checks the build for common issues
- [GenerateWorkflowFiles](docs/built-in-targets/generate-workflow-files.md) — Generates CI/CD YAML files

### Developer Guide

- [Writing a Module](docs/developer-guide/writing-a-module.md) — Creating a reusable Atom module NuGet package
- [Custom Providers](docs/developer-guide/custom-providers.md) — Implementing `IArtifactProvider`, `ISecretsProvider`,
  and more
- [Source Generators](docs/developer-guide/source-generators.md) — How the Atom analysers and source generators work
- [Testing](docs/developer-guide/testing.md) — Using `Invex.Atom.TestUtils`
- [Releasing Atom](docs/developer-guide/releasing-atom.md) — Protected release process and rollback

### Reference

- [CLI](docs/reference/cli.md) — Command-line arguments and the Atom global tool
- [API Reference](api/index.md) — Auto-generated API documentation (via DocFX)

## AI Disclaimer

The Atom libraries are human-made, however GitHub Copilot completions have been used on a superficial level.

Generative AI was also used to assist in writing tests and documentation.

## License

Atom is released under the [MIT License](LICENSE.txt).