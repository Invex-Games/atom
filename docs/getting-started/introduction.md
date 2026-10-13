# Introduction

Atom is a build automation framework for .NET that lets you define your entire build pipeline in C#. Instead of
maintaining separate YAML or script files, you write strongly-typed build logic alongside your application code, gaining
full IDE support — IntelliSense, refactoring, and step-through debugging. This page explains when Atom fits, the
concepts you will meet, and how the packages fit together.

## When to Use Atom

Atom is a good fit when:

- Your repository is already .NET and you would rather write build steps in C# than in shell scripts or YAML.
- The same build has to run on a developer machine and on CI, and you want to debug it locally.
- You maintain GitHub Actions or Azure DevOps pipelines by hand and want them generated from one definition instead.
- Several repositories share build logic that you want to package and version as NuGet modules.

It is probably not worth it when:

- The build is a single `dotnet build` / `dotnet test` call that a few lines of YAML already cover.
- The repository has no .NET SDK available, since an Atom build is itself a .NET 10 project or file.
- You depend on CI platform features that Atom's workflow model does not express; you can still use a base build
  locally and keep hand-written YAML.

## Key Concepts

| Concept              | Description                                                                                                            |
|----------------------|------------------------------------------------------------------------------------------------------------------------|
| **Build Definition** | A C# interface (recommended) or partial class that declares your targets, parameters, and build configuration.     |
| **Target**           | A named unit of work (compile, test, pack, deploy, etc.) with optional dependencies on other targets.                  |
| **Parameter**        | A value that can be supplied via the command line, environment variable, `appsettings.json`, or a secrets provider.    |
| **Module**           | A NuGet package that adds reusable targets, parameters, or service registrations to your build.                        |
| **Workflow**         | An optional layer that maps targets to CI/CD jobs and generates platform-specific YAML (GitHub Actions, Azure DevOps). |

## Package Overview

Atom is split into several NuGet packages so you only pull in what you need:

| Package                | Purpose                                                               |
|------------------------|-----------------------------------------------------------------------|
| `Invex.Atom.Build`     | Core framework — build definitions, targets, parameters, hosting.     |
| `Invex.Atom.Workflows` | Workflow definitions, triggers, and YAML generation.                  |
| `Invex.Atom.Module.*`  | First-party modules (Dotnet, GitVersion, AzureKeyVault, etc.).        |
| `Invex.Atom.Tool`      | The `atom` .NET global tool for running builds from the command line. |

### Foundational Libraries

Several lower-level building blocks live in their own repositories and are published as standalone packages. You don't
normally reference these directly — they are pulled in transitively by `Invex.Atom.Build` — but their types surface
through Atom (for example via `IBuildAccessor.RootedFileSystem` and `IBuildAccessor.ProcessRunner`):

| Package                 | Purpose                                                                      |
|-------------------------|------------------------------------------------------------------------------|
| `Invex.FileSystem`      | `IRootedFileSystem`, `RootedPath`, path providers, and file transformations. |
| `Invex.Process`         | `IProcessRunner` for executing external tools.                               |
| `Invex.SemanticVersion` | Semantic versioning utilities.                                               |
| `Invex.StructuredText`  | Structured text / YAML writing used by the workflow generators.              |

## How It Works

1. You create a C# project (or a single `.cs` file) that references the Atom packages.
2. You define an interface decorated with `[BuildDefinition]` that extends `IBuildDefinition` (or
   `IWorkflowBuildDefinition` if you need CI/CD generation). A partial class deriving from
   `BuildDefinition` or `WorkflowBuildDefinition` is the alternative when class-specific overrides are needed.
3. Inside that build definition you declare **targets** — lambda-based definitions that describe what to execute, their
   dependencies, required parameters, and produced artifacts.
4. You run the build with `dotnet run -- <TargetName>` (or via the `atom` global tool).
5. If you use `WorkflowBuildDefinition`, running the `Gen` target emits platform-specific YAML that
   invokes your same build on CI.

## Design Principles

- **Builds are code.** Targets, parameters, and workflows are C# members, so they are type-checked, refactorable, and
  debuggable.
- **Same code everywhere.** A target executes the same code locally and on CI; only the transport of variables and
  artifacts between jobs differs.
- **Composition over inheritance.** Build definitions and modules are interfaces that a build implements, and source
  generators discover their targets and parameters.
- **Explicit data flow.** Targets declare the parameters, artifacts, and variables they use, so Atom can validate them
  and split workflows into jobs correctly.
- **Generated files stay in sync.** Workflow files are regenerated locally and the build fails in headless mode when they
  are out of date.

## Next steps

- [Your First Build](your-first-build.md): create and run a minimal build.
- [Base vs Workflow Build](base-vs-workflow-build.md): choose between local-only builds and workflow generation.
- [Build Definitions](../core-concepts/build-definitions.md): how build definitions and source generators work.
