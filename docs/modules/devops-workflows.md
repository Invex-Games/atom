# DevOps Workflows

Adds Azure DevOps Pipelines support to Atom — pipeline YAML generation, DevOps-specific options, and access to Azure
DevOps environment variables. Add it when you want Atom to generate Azure Pipelines YAML.

**Package:** `Invex.Atom.Module.DevOpsWorkflows`

## Installation

```shell
dotnet add package Invex.Atom.Module.DevOpsWorkflows
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IDevOpsWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Build")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(Compile))],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}
```

## Features

### Pipeline Generation

Generates Azure DevOps Pipeline YAML files.

### `DevOps` Static Class

Access Azure DevOps environment variables:

```csharp
if (DevOps.IsDevOpsPipelines)
{
    var buildId = DevOps.Variables.BuildBuildId;
    var branch = DevOps.Variables.BuildSourceBranch;
}
```

### Variable Provider

`DevOpsVariableProvider` maps Atom variables to Azure DevOps pipeline variables.

### Report Writer

`DevOpsSummaryOutcomeReportWriter` writes build report data to the Azure DevOps build summary.

### Pool Labels

```csharp
WorkflowLabels.DevOps.Pool.Ubuntu_Latest
WorkflowLabels.DevOps.Pool.Windows_Latest
WorkflowLabels.DevOps.Pool.MacOS_Latest
```

### DevOps-Specific Options

- `BuildOptions.DevOps.DevOpsPool.SetByMatrix` — set the agent pool from a matrix dimension
- `BuildOptions.DevOps.VariableGroup.*` — reference Azure DevOps variable groups
- `BuildOptions.DevOps.Concurrency.RunLatest` — run only the latest pipeline waiting on an exclusive lock
- `BuildOptions.DevOps.Concurrency.Sequential` — run every pipeline waiting on an exclusive lock in sequence
- `BuildOptions.DevOps.PullRequest.AutoCancel` — cancel an in-progress PR run when a new commit is pushed
- `BuildOptions.DevOps.PullRequest.KeepRunning` — allow in-progress PR runs to finish after new commits
- `ProvideDevOpsRunIdAsWorkflowId` — use the DevOps run ID as the Atom build ID

Configure the lock behavior on a `WorkflowDefinition`:

```csharp
Options =
[
    BuildOptions.DevOps.Concurrency.Sequential,
];
```

Azure Pipelines applies `lockBehavior` to resources protected by an
[exclusive lock check](https://learn.microsoft.com/azure/devops/pipelines/process/approvals#exclusive-lock).

Configure pull request auto-cancellation alongside a pull request trigger:

```csharp
new("Validate")
{
    Triggers = [WorkflowTriggers.PullInto("main", "develop")],
    Options = [BuildOptions.DevOps.PullRequest.AutoCancel],
}
```

## See also

- [Workflow Definitions](../workflows/workflow-definitions.md): declaring workflows.
- [GitHub Workflows](github-workflows.md): the GitHub Actions counterpart.
- <xref:Invex.Atom.Module.DevOpsWorkflows.IDevOpsWorkflows>
- <xref:Invex.Atom.Module.DevOpsWorkflows.DevOps>
- <xref:Invex.Atom.Module.DevOpsWorkflows.Options.DevOpsConcurrencyOption>
- <xref:Invex.Atom.Module.DevOpsWorkflows.Options.DevOpsPullRequestOption>
