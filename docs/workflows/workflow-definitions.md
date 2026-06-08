# Workflow Definitions

A `WorkflowDefinition` describes a single CI/CD pipeline — its name, triggers, targets, options, and which platforms it
applies to.

## Basic Structure

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("CI")
    {
        Triggers = [WorkflowTriggers.PushToMain, WorkflowTriggers.PullIntoMain],
        Targets =
        [
            new(nameof(SetupBuildInfo)),
            new(nameof(Compile)),
            new(nameof(Test)),
        ],
        Types = [WorkflowTypes.Github.Action],
    },
];
```

## `WorkflowDefinition` Properties

| Property   | Type                                      | Description                                    |
|------------|-------------------------------------------|------------------------------------------------|
| `Name`     | `string`                                  | The workflow name (used as the file name).     |
| `Triggers` | `IReadOnlyList<IWorkflowTrigger>`         | Events that start the workflow.                |
| `Targets`  | `IReadOnlyList<WorkflowTargetDefinition>` | The targets to include in the workflow.        |
| `Options`  | `IReadOnlyList<IBuildOption>`             | Workflow-level options applied to all targets. |
| `Types`    | `IReadOnlyList<IWorkflowType>`            | Which platforms to generate for.               |

## `WorkflowTargetDefinition`

Each entry in `Targets` is a `WorkflowTargetDefinition`:

```csharp
new(nameof(Test))
{
    MatrixDimensions =
    [
        new(nameof(IJobRunsOn.JobRunsOn))
        {
            Values = ["ubuntu-latest", "windows-latest"],
        },
    ],
    Options = [BuildOptions.Github.RunsOn.SetByMatrix],
}
```

| Property           | Description                                                         |
|--------------------|---------------------------------------------------------------------|
| `Name`             | The target name (must match a target in the build definition).      |
| `MatrixDimensions` | Run the target across multiple configurations (OS, framework, etc). |
| `Options`          | Per-target options (suppress artifacts, inject params, etc).        |

### Matrix Dimensions

A `MatrixDimension` defines a named axis with multiple values. The CI platform runs the target once per combination:

```csharp
new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
{
    Values = ["ubuntu-latest", "windows-latest", "macos-latest"],
}
```

Multiple dimensions create a full cross product.

## Workflow Types

The `Types` list determines which platforms receive generated files:

| Type                            | Platform                              |
|---------------------------------|---------------------------------------|
| `WorkflowTypes.Github.Action`   | GitHub Actions (`.github/workflows/`) |
| `WorkflowTypes.Devops.Pipeline` | Azure DevOps Pipelines                |

You can target multiple platforms from the same workflow definition.

## Workflow-Level Options

Options applied at the workflow level affect all targets in the workflow:

```csharp
new("Build")
{
    Options =
    [
        BuildOptions.Inject.Param(nameof(NugetDryRun), true),
        BuildOptions.Devops.VariableGroup.Atom,
    ],
    // ...
}
```

## Real-World Example

From the Atom project's own build:

```csharp
new("Validate")
{
    Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain],
    Targets =
    [
        new(nameof(ISetupBuildInfo.SetupBuildInfo)),
        new(nameof(IBuildTargets.PackProjects))
        {
            Options = [BuildOptions.Target.SuppressArtifactPublishing],
        },
        new(nameof(ITestTargets.TestProjects))
        {
            MatrixDimensions =
            [
                new(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = ["windows-latest", "ubuntu-latest", "macos-latest"],
                },
                new(nameof(ITestTargets.TestFramework))
                {
                    Values = ["net8.0", "net9.0", "net10.0"],
                },
            ],
            Options =
            [
                BuildOptions.Target.SuppressArtifactPublishing,
                BuildOptions.Github.RunsOn.SetByMatrix,
            ],
        },
    ],
    Types = [WorkflowTypes.Github.Action],
}
```

## Next Steps

→ [Triggers](triggers.md)

