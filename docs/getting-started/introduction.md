# Introduction

Atom is a build automation framework for .NET that lets you define your entire build pipeline in C#. Instead of
maintaining separate YAML or script files, you write strongly-typed build logic alongside your application code, gaining
full IDE support — IntelliSense, refactoring, and step-through debugging.

## Key Concepts

| Concept              | Description                                                                                                            |
|----------------------|------------------------------------------------------------------------------------------------------------------------|
| **Build Definition** | A C# class that declares your targets, parameters, and build configuration.                                            |
| **Target**           | A named unit of work (compile, test, pack, deploy, etc.) with optional dependencies on other targets.                  |
| **Parameter**        | A value that can be supplied via the command line, environment variable, `appsettings.json`, or a secrets provider.    |
| **Module**           | A NuGet package that adds reusable targets, parameters, or service registrations to your build.                        |
| **Workflow**         | An optional layer that maps targets to CI/CD jobs and generates platform-specific YAML (GitHub Actions, Azure DevOps). |

## Package Overview

Atom is split into several NuGet packages so you only pull in what you need:

| Package                      | Purpose                                                                    |
|------------------------------|----------------------------------------------------------------------------|
| `DecSm.Atom.Build`           | Core framework — build definitions, targets, parameters, hosting.          |
| `DecSm.Atom.Workflows`       | Workflow definitions, triggers, and YAML generation.                       |
| `DecSm.Atom.FileSystem`      | `IAtomFileSystem`, `RootedPath`, path providers, and file transformations. |
| `DecSm.Atom.Process`         | `IProcessRunner` for executing external tools.                             |
| `DecSm.Atom.SemanticVersion` | Semantic versioning utilities.                                             |
| `DecSm.Atom.Module.*`        | First-party modules (Dotnet, GitVersion, AzureKeyVault, etc.).             |
| `DecSm.Atom.Tool`            | The `atom` .NET global tool for running builds from the command line.      |

## How It Works

1. You create a C# project (or a single `.cs` file) that references the Atom packages.
2. You define a class decorated with `[BuildDefinition]` that inherits from `BuildDefinition` (or
   `WorkflowBuildDefinition` if you need CI/CD generation).
3. Inside that class you declare **targets** — lambda-based definitions that describe what to execute, their
   dependencies, required parameters, and produced artifacts.
4. You run the build with `dotnet run -- <TargetName>` (or via the `atom` global tool).
5. If you use `WorkflowBuildDefinition`, running the `GenerateWorkflowFiles` target emits platform-specific YAML that
   invokes your same build on CI.

## Next Steps

→ [Your First Build](your-first-build.md)

