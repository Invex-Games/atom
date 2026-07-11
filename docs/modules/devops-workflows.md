# Module: DevOps Workflows

**Package:** `Invex.Atom.Module.DevopsWorkflows`

Adds Azure DevOps Pipelines support to Atom — pipeline YAML generation, DevOps-specific options, and access to Azure
DevOps environment variables.

## Installation

```shell
dotnet add package Invex.Atom.Module.DevopsWorkflows
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IDevopsWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Build")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(Compile))],
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}
```

## Features

### Pipeline Generation

Generates Azure DevOps Pipeline YAML files.

### `Devops` Static Class

Access Azure DevOps environment variables:

```csharp
if (Devops.IsDevopsPipelines)
{
    var buildId = Devops.Variables.BuildBuildId;
    var branch = Devops.Variables.BuildSourceBranch;
}
```

### Variable Provider

`DevopsVariableProvider` maps Atom variables to Azure DevOps pipeline variables.

### Report Writer

`DevopsSummaryOutcomeReportWriter` writes build report data to the Azure DevOps build summary.

### Pool Labels

```csharp
WorkflowLabels.Devops.Pool.Ubuntu_Latest
WorkflowLabels.Devops.Pool.Windows_Latest
WorkflowLabels.Devops.Pool.MacOs_Latest
```

### DevOps-Specific Options

- `BuildOptions.Devops.DevopsPool.SetByMatrix` — set the agent pool from a matrix dimension
- `BuildOptions.Devops.VariableGroup.*` — reference Azure DevOps variable groups
- `BuildOptions.Devops.Concurrency.RunLatest` — run only the latest pipeline waiting on an exclusive lock
- `BuildOptions.Devops.Concurrency.Sequential` — run every pipeline waiting on an exclusive lock in sequence
- `BuildOptions.Devops.PullRequest.AutoCancel` — cancel an in-progress PR run when a new commit is pushed
- `BuildOptions.Devops.PullRequest.KeepRunning` — allow in-progress PR runs to finish after new commits
- `ProvideDevopsRunIdAsWorkflowId` — use the DevOps run ID as the Atom build ID

Configure the lock behavior on a `WorkflowDefinition`:

```csharp
Options =
[
    BuildOptions.Devops.Concurrency.Sequential,
];
```

Azure Pipelines applies `lockBehavior` to resources protected by an
[exclusive lock check](https://learn.microsoft.com/azure/devops/pipelines/process/approvals#exclusive-lock).

Configure pull request auto-cancellation alongside a pull request trigger:

```csharp
new("Validate")
{
    Triggers = [WorkflowTriggers.PullInto("main", "develop")],
    Options = [BuildOptions.Devops.PullRequest.AutoCancel],
}
```
