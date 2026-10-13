# Triggers

Triggers define when a workflow starts. Atom provides a fluent API via the `WorkflowTriggers` helper class. A definition with no triggers is allowed; for Azure DevOps it is written with `trigger: none`, so pushes do not start the pipeline and it can still be queued manually.

## Built-in Trigger Shortcuts

| Shortcut                        | Trigger Type            | Description                         |
|---------------------------------|-------------------------|-------------------------------------|
| `WorkflowTriggers.Manual`       | `ManualTrigger`         | Manual dispatch (workflow_dispatch) |
| `WorkflowTriggers.PushToMain`   | `GitPushTrigger`        | Push to `main` branch               |
| `WorkflowTriggers.PullIntoMain` | `GitPullRequestTrigger` | Pull request targeting `main`       |

## Git Push Trigger

```csharp
// Push to specific branches
WorkflowTriggers.PushTo("main", "release/**")

// Full control
WorkflowTriggers.Push(
    includedBranches: ["main"],
    excludedBranches: [],
    includedPaths: ["src/**"],
    excludedPaths: ["docs/**"],
    includedTags: ["v*"],
    excludedTags: [])
```

### `GitPushTrigger` Properties

| Property           | Description                            |
|--------------------|----------------------------------------|
| `IncludedBranches` | Branches that trigger the workflow     |
| `ExcludedBranches` | Branches to exclude                    |
| `IncludedPaths`    | File paths that trigger the workflow   |
| `ExcludedPaths`    | File paths to exclude                  |
| `IncludedTags`     | Tag patterns that trigger the workflow |
| `ExcludedTags`     | Tag patterns to exclude                |

## Git Pull Request Trigger

```csharp
// PR targeting specific branches
WorkflowTriggers.PullInto("main", "release/**")

// Full control
WorkflowTriggers.PullRequest(
    includedBranches: ["main"],
    excludedBranches: [],
    includedPaths: ["src/**"],
    excludedPaths: [],
    types: ["opened", "synchronize"])
```

## Manual Trigger

```csharp
// Simple manual dispatch
WorkflowTriggers.Manual

// With inputs (signature: name, description, required, [choices], [default])
WorkflowTriggers.ManualWithInputs(
    new ManualStringInput("version", "Version to deploy", required: true),
    new ManualBoolInput("dry-run", "Dry run mode", required: false),
    new ManualChoiceInput("environment", "Target environment", required: true, ["staging", "production"]))
```

> [!TIP]
> Use the `ForParam(...)` factory on each input type to build an input directly from a `ParamDefinition`, keeping the
> input name and description in sync with the parameter.

### Manual Input Types

| Type                | Description                     |
|---------------------|---------------------------------|
| `ManualStringInput` | Free-text string input          |
| `ManualBoolInput`   | Boolean toggle                  |
| `ManualChoiceInput` | Dropdown with predefined values |

## Platform-Specific Triggers

Platform modules can provide additional trigger types. For example, `Invex.Atom.Module.GithubWorkflows` adds
`GithubTrigger` for events like `release`:

```csharp
new GithubTrigger(new On.Release([On.Release.ReleaseType.released]))
```

## Combining Triggers

A workflow can have multiple triggers:

```csharp
Triggers =
[
    WorkflowTriggers.Manual,
    WorkflowTriggers.PushToMain,
    WorkflowTriggers.PullIntoMain,
],
```

## Next steps

- [Workflow Options](workflow-options.md): fine-tune targets in generated workflows.
- [GitHub Workflows](../modules/github-workflows.md): GitHub-specific triggers.
- <xref:Invex.Atom.Workflows.Definition.Triggers.WorkflowTriggers>
- <xref:Invex.Atom.Workflows.Definition.Triggers.GitPushTrigger>
- <xref:Invex.Atom.Workflows.Definition.Triggers.GitPullRequestTrigger>
- <xref:Invex.Atom.Workflows.Definition.Triggers.ManualTrigger>
