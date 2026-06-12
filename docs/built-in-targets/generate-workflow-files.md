# Gen (Generate Workflow Files)

**Interface:** `IGen`  
**Package:** `Invex.Atom.Workflows`

The `Gen` target generates CI/CD configuration files from your workflow definitions.

## What It Does

1. Resolves each `WorkflowDefinition` in your build's `Workflows` property.
2. Analyses the target dependency graph, artifact dependencies, and variable dependencies.
3. Maps targets into jobs and creates a platform-neutral `WorkflowModel`.
4. Invokes each registered `IWorkflowWriter` (one per platform type) to render and write the YAML files.

## Generated File Locations

| Platform       | Output Directory     |
|----------------|----------------------|
| GitHub Actions | `.github/workflows/` |
| Azure DevOps   | `.devops/workflows/` |

## Usage

```shell
dotnet run --project _atom Gen
# or, with the atom global tool
atom Gen
```

## Requirements

Your build must:

1. Inherit from `WorkflowBuildDefinition` (or implement `IWorkflowBuildDefinition`).
2. Override the `Workflows` property with at least one definition.
3. Inherit a platform module interface (`IGithubWorkflows`, `IDevopsWorkflows`, or both) so the corresponding
   workflow writer is registered.

## Automatic Generation & Outdated File Detection

You usually don't need to run `Gen` explicitly during local development: when the build runs in **interactive
(non-headless) mode**, the `WorkflowLifecycleHook` regenerates workflow files automatically before any targets execute.

When the build runs in **headless mode** (the `--headless` flag, typical in CI), files are *not* regenerated. Instead,
if the files on disk are out of date with the current definitions, the build fails early with a
`WorkflowOutdatedException`, prompting you to run the `Gen` target locally and commit the regenerated files.
