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
- `ProvideDevopsRunIdAsWorkflowId` — use the DevOps run ID as the Atom build ID

