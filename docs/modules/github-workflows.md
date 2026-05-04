# Module: GitHub Workflows

**Package:** `DecSm.Atom.Module.GithubWorkflows`

Adds GitHub Actions support to Atom — workflow YAML generation, GitHub-specific build options, variable handling, and
access to GitHub environment variables.

## Installation

```shell
dotnet add package DecSm.Atom.Module.GithubWorkflows
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(Compile))],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}
```

Running `dotnet run -- Gen` generates `.github/workflows/CI.yml`.

## Features

### Workflow Generation

The module provides a `WorkflowFileWriter` that generates GitHub Actions YAML files in `.github/workflows/`.

### `Github` Static Class

Access all GitHub Actions environment variables in a typed way:

```csharp
if (Github.IsGithubActions)
{
    var sha = Github.Variables.Sha;
    var ref = Github.Variables.Ref;
}
```

### Variable Provider

`GithubVariableProvider` writes variables to `$GITHUB_OUTPUT` and reads them from job outputs, enabling cross-job data
sharing.

### Report Writer

`GithubSummaryOutcomeReportWriter` writes build report data to the GitHub Actions job summary (`$GITHUB_STEP_SUMMARY`).

### Runner Labels

Use `WorkflowLabels.Github.RunsOn.*` constants for runner selection:

```csharp
WorkflowLabels.Github.RunsOn.Ubuntu_Latest
WorkflowLabels.Github.RunsOn.Windows_Latest
WorkflowLabels.Github.RunsOn.MacOs_Latest
```

### GitHub-Specific Options

- `BuildOptions.Github.RunsOn.SetByMatrix` — set the runner from a matrix dimension
- `GithubTokenPermissionsOption` — fine-grained `GITHUB_TOKEN` permissions
- GitHub-specific triggers (e.g. `release` events)

### Dependabot Configuration

Generate Dependabot configuration alongside your workflow files:

```csharp
WorkflowPresets.Github.Dependabot(new()
{
    Updates =
    [
        new()
        {
            Directory = "/",
            PackageEcosystem = WorkflowLabels.Github.Dependabot.NugetEcosystem,
            Schedule = new() { Interval = ScheduleInterval.Daily },
        },
    ],
})
```

