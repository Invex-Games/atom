# Atom

[![Build](https://github.com/Invex-Games/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/Invex-Games/atom/actions/workflows/Build.yml)
[![NuGet](https://img.shields.io/nuget/v/Invex.Atom.Build.svg)](https://www.nuget.org/packages/Invex.Atom.Build)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/Invex-Games/atom/blob/main/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Packages

| Package                                                                                                 | What it does                                                          |
|---------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------|
| [Invex.Atom.Build](https://www.nuget.org/packages/Invex.Atom.Build)                                     | Core framework: build definitions, targets, parameters, hosting.     |
| [Invex.Atom.Workflows](https://www.nuget.org/packages/Invex.Atom.Workflows)                             | Workflow definitions, triggers, options, and the `Gen` target.        |
| [Invex.Atom.Module.Dotnet](https://www.nuget.org/packages/Invex.Atom.Module.Dotnet)                     | .NET CLI wrappers and build, test, pack, publish, and NuGet helpers.  |
| [Invex.Atom.Module.GitHubWorkflows](https://www.nuget.org/packages/Invex.Atom.Module.GitHubWorkflows)   | GitHub Actions workflow and Dependabot generation.                    |
| [Invex.Atom.Module.DevOpsWorkflows](https://www.nuget.org/packages/Invex.Atom.Module.DevOpsWorkflows)   | Azure DevOps Pipelines workflow generation.                           |
| [Invex.Atom.Module.AzureKeyVault](https://www.nuget.org/packages/Invex.Atom.Module.AzureKeyVault)       | Azure Key Vault secrets provider.                                     |
| [Invex.Atom.Module.AzureStorage](https://www.nuget.org/packages/Invex.Atom.Module.AzureStorage)         | Azure Blob Storage artifact provider.                                 |
| [Invex.Atom.Module.GitVersion](https://www.nuget.org/packages/Invex.Atom.Module.GitVersion)             | GitVersion-based build ID and version.                                |
| [Invex.Atom.Tool](https://www.nuget.org/packages/Invex.Atom.Tool)                                       | The `atom` .NET global tool for running builds from any directory.    |

## Features

- **Zero context switching.** Write build logic in C# alongside your application code.
- **IntelliSense and debugging.** Step through your build process using your IDE.
- **CI/CD agnostic.** Define logic once; Atom generates the YAML for GitHub Actions and Azure DevOps.
- **Modular.** Pull in capabilities via NuGet packages (GitVersion, Azure Key Vault, and more).
- **Source generators.** Targets and parameters are discovered automatically, so there is no registration boilerplate.

## Installation

Atom build projects require the .NET 10 SDK. The libraries target `net10.0`.

Add the core package to your build project (commonly `_atom/_atom.csproj`):

```shell
dotnet add package Invex.Atom.Build
```

Install the CLI once if you want to invoke builds from any directory:

```shell
dotnet tool install --global Invex.Atom.Tool
```

## Quick start

1. Create a .NET 10 project:

   ```shell
   dotnet new console -n _atom
   ```

2. Replace the contents of `_atom.csproj`:

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

3. Replace `Program.cs` with `IBuild.cs`:

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

4. Run the target with `atom HelloWorld` (or `dotnet run --project _atom -- HelloWorld`):

   ```text
   26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
   01:16:47.552 INF Executing build

   HelloWorld

   Prints a hello world message to the console

   26-06-06 +10:00  HelloWorld | Atom.Build:
   01:16:47.616 INF Hello, World!

   Build Summary

     HelloWorld │ Succeeded │ <0.01s
   ```

## Usage

### Parameters and secrets

Declare a parameter with `[ParamDefinition]` (or a secret with `[SecretDefinition]`, which is masked in logs) and
require it on a target:

```csharp
[ParamDefinition("my-name", "My name")]
string? MyName => GetParam(() => MyName);

[SecretDefinition("my-secret", "My secret")]
string? MySecret => GetParam(() => MySecret);

Target HelloWorld =>
    t => t
        .RequiresParam(nameof(MyName))
        .RequiresParam(nameof(MySecret))
        .Executes(() => Logger.LogInformation("I am {MyName} and my secret is {MySecret}.", MyName, MySecret));
```

Run it with `atom HelloWorld --my-name Frodo --my-secret TheOneRing`. See
[Parameters](https://github.com/Invex-Games/atom/blob/main/docs/core-concepts/parameters.md) and
[Secrets](https://github.com/Invex-Games/atom/blob/main/docs/core-concepts/secrets.md).

### Target dependencies

Targets run in dependency order, and parameters only need to be provided once:

```csharp
Target Goodbye =>
    t => t
        .DescribedAs("Prints a goodbye message to the console")
        .DependsOn(nameof(HelloWorld))
        .Executes(() => Logger.LogInformation("Goodbye!"));
```

`atom Goodbye --my-name Frodo --my-secret TheOneRing` runs `HelloWorld` and then `Goodbye`. See
[Targets](https://github.com/Invex-Games/atom/blob/main/docs/core-concepts/targets.md).

### Workflow generation

Reference `Invex.Atom.Module.GitHubWorkflows`, extend `IWorkflowBuildDefinition`, and declare a workflow:

```csharp
using Invex.StructuredText.Expressions;

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IWorkflowBuildDefinition, IGitHubWorkflows
{
    // targets and parameters as above

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
                        BuildOptions.Inject.Param(nameof(MyName), TextExpressions.GitHub.GitHubRepositoryOwner),
                        BuildOptions.Inject.Secret(nameof(MySecret)),
                    ],
                },
                new(nameof(Goodbye)),
            ],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}
```

`atom Gen` writes `.github/workflows/Hello.yml`, with one job per target and the parameter and secret injected from
GitHub. See [Workflow Definitions](https://github.com/Invex-Games/atom/blob/main/docs/workflows/workflow-definitions.md).

### File-based apps

A build can also be a single `Atom.cs` file run with `dotnet run Atom.cs HelloWorld`:

```csharp
#:sdk Microsoft.NET.Sdk.Worker
#:package Invex.Atom.Build@3.*

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target HelloWorld => t => t.Executes(() => Logger.LogInformation("Hello, World!"));
}
```

See [Your First Build](https://github.com/Invex-Games/atom/blob/main/docs/getting-started/your-first-build.md).

## Documentation

Full documentation: [https://invex-games.github.io/atom/](https://invex-games.github.io/atom/)

| Guide                                                                                                             | Contents                                                                                                  |
|-------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------|
| [Introduction](https://github.com/Invex-Games/atom/blob/main/docs/getting-started/introduction.md)                | What Atom is, when to use it, and how the packages fit together.                                          |
| [Your First Build](https://github.com/Invex-Games/atom/blob/main/docs/getting-started/your-first-build.md)        | Create a project-based or file-based build and run it.                                                    |
| [Base vs Workflow Build](https://github.com/Invex-Games/atom/blob/main/docs/getting-started/base-vs-workflow-build.md) | `IBuildDefinition` vs `IWorkflowBuildDefinition`.                                                     |
| [Core Concepts](https://github.com/Invex-Games/atom/blob/main/docs/core-concepts/build-definitions.md)            | Build definitions, targets, parameters, secrets, artifacts, variables, file system, hosting, and more.    |
| [Workflows](https://github.com/Invex-Games/atom/blob/main/docs/workflows/overview.md)                             | Workflow definitions, triggers, options, variables across jobs, and debugging.                            |
| [Modules](https://github.com/Invex-Games/atom/blob/main/docs/modules/overview.md)                                 | .NET, GitHub Workflows, DevOps Workflows, Azure Key Vault, Azure Storage, and GitVersion.                 |
| [Built-in Targets](https://github.com/Invex-Games/atom/blob/main/docs/built-in-targets/setup-build-info.md)       | `SetupBuildInfo`, `ValidateBuild`, and `Gen`.                                                              |
| [Developer Guide](https://github.com/Invex-Games/atom/blob/main/docs/developer-guide/writing-a-module.md)         | Writing modules, custom providers, source generators, and testing.                                        |
| [CLI Reference](https://github.com/Invex-Games/atom/blob/main/docs/reference/cli.md)                              | Command-line options and the `atom` global tool.                                                          |
| [API Reference](https://github.com/Invex-Games/atom/blob/main/api/index.md)                                       | Key types per package, linked into the generated reference.                                               |

## Development

The solution is `Invex.Atom.slnx` and requires the .NET 10 SDK:

```shell
dotnet build Invex.Atom.slnx
dotnet test Invex.Atom.slnx
```

Atom builds itself with the build definition in `_atom/`. Build or preview the DocFX documentation with:

```shell
atom BuildDocs
atom ServeDocs
```

`dotnet run --project _atom -- <target>` is equivalent to `atom <target>`.

## AI Disclaimer

The Atom libraries are human-made, however GitHub Copilot completions have been used on a superficial level.

Generative AI was also used to assist in writing tests and documentation.

## License

Atom is released under the [MIT License](https://github.com/Invex-Games/atom/blob/main/LICENSE.txt).
