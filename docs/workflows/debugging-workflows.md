# Debugging Workflows

Atom includes a `DebugWorkflowType` that lets you simulate workflow generation locally without pushing to CI. Use it when a generated workflow does not look the way you expect.

## `DebugWorkflowType`

When `DebugWorkflowType` is added to your workflow's `Types`, Atom writes the resolved workflow model as JSON to
`.debug-workflows/<workflow>.json`, which is useful for inspecting exactly what Atom generates. Debug output is never
reported as outdated, so leaving it in place does not fail headless builds.

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

## Next steps

- [Modules Overview](../modules/overview.md): add platform and tool integrations.
- [Gen](../built-in-targets/generate-workflow-files.md): automatic generation and outdated-file detection.
- <xref:Invex.Atom.Workflows.DebugWorkflowType>
- <xref:Invex.Atom.Workflows.WorkflowOutdatedException>
