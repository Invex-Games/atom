# GenerateWorkflowFiles

**Interface:** `IGenerateWorkflowFiles`  
**Package:** `Invex.Atom.Workflows`

The `GenerateWorkflowFiles` target generates CI/CD configuration files from your workflow definitions.

## Alias

`Gen` — you can run this target with `dotnet run -- Gen`.

## What It Does

1. Resolves each `WorkflowDefinition` in your build's `Workflows` property.
2. Analyses the target dependency graph, artifact dependencies, and variable dependencies.
3. Maps targets into jobs and creates a platform-neutral `WorkflowModel`.
4. Invokes each registered `WorkflowFileWriter` (one per platform type) to render and write the YAML files.

## Generated File Locations

| Platform       | Output Directory             |
|----------------|------------------------------|
| GitHub Actions | `.github/workflows/`         |
| Azure DevOps   | Project root (pipeline YAML) |

## Usage

```shell
dotnet run -- GenerateWorkflowFiles
# or
dotnet run -- Gen
```

## Requirements

Your build must:

1. Inherit from `WorkflowBuildDefinition` (not just `BuildDefinition`).
2. Override the `Workflows` property with at least one definition.
3. Implement a platform module (`IGithubWorkflows`, `IDevopsWorkflows`, or both).

## Outdated File Detection

The `WorkflowLifecycleHook` checks on every build whether the files on disk are up to date with the current definitions.
If not, it warns or fails the build, prompting you to re-run `Gen`.

