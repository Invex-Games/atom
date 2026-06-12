# Debugging Workflows

Atom includes a `DebugWorkflowType` that lets you simulate workflow generation locally without pushing to CI.

## `DebugWorkflowType`

`DebugWorkflowType` is an `IWorkflowType` where `IsRunning` is always `true`. When added to your workflow's `Types`, it
triggers the `DebugWorkflowWriter` which writes the resolved workflow model as JSON to a `.debug-workflows` directory —
useful for inspecting exactly what Atom generates.

## Running Locally

When you execute `dotnet run -- Gen`, the platform-specific writers produce the actual YAML
files. You can compare these against your expectations by:

1. Running `Gen` and diffing the generated YAML.
2. Adding `DebugWorkflowType` temporarily to inspect the internal model.

## Checking for Outdated Workflows

The `WorkflowLifecycleHook` runs during `BeforeExecute`. In interactive (non-headless) mode it regenerates the workflow
files automatically; in headless mode (CI) it detects when the YAML on disk doesn't match what the current build
definition would generate and fails the build with a `WorkflowOutdatedException`. This helps catch cases where you've
changed targets or workflows but forgotten to regenerate.

## Tips

- Use `--verbose` to see detailed resolution logs.
- Inspect the generated YAML directly — it's meant to be human-readable.
- The workflow resolver will log warnings for missing variable/artifact declarations.
- All your targets execute the same code locally and on CI; only the variable/artifact transport differs.

## Next Steps

→ [Modules Overview](../modules/overview.md)

