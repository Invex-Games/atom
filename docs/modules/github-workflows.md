# GitHub Workflows

Adds GitHub Actions support to Atom — workflow YAML generation, GitHub-specific build options, variable handling, and
access to GitHub environment variables. Add it when you want Atom to generate GitHub Actions workflows.

**Package:** `Invex.Atom.Module.GitHubWorkflows`

## Installation

```shell
dotnet add package Invex.Atom.Module.GitHubWorkflows
```

## Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IGitHubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(Compile))],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}
```

Running `dotnet run -- Gen` generates `.github/workflows/CI.yml`.

## Features

### Workflow Generation

The module provides a `WorkflowFileWriter` that generates GitHub Actions YAML files in `.github/workflows/`.

### `GitHub` Static Class

Access all GitHub Actions environment variables in a typed way:

```csharp
if (GitHub.IsGitHubActions)
{
    var sha = GitHub.Variables.Sha;
    var ref = GitHub.Variables.Ref;
}
```

### Variable Provider

`GitHubVariableProvider` writes variables to `$GITHUB_OUTPUT` and reads them from job outputs, enabling cross-job data
sharing.

### Report Writer

`GitHubSummaryOutcomeReportWriter` writes build report data to the GitHub Actions job summary (`$GITHUB_STEP_SUMMARY`).

### Runner Labels

Use `WorkflowLabels.GitHub.RunsOn.*` constants for runner selection:

```csharp
WorkflowLabels.GitHub.RunsOn.Ubuntu_Latest
WorkflowLabels.GitHub.RunsOn.Windows_Latest
WorkflowLabels.GitHub.RunsOn.MacOS_Latest
```

### GitHub-Specific Options

- `BuildOptions.GitHub.RunsOn.SetByMatrix` — set the runner from a matrix dimension
- `GitHubTokenPermissionsOption` — fine-grained `GITHUB_TOKEN` permissions
- `BuildOptions.GitHub.Concurrency.Set(...)` — limit concurrent workflow runs sharing a group
- GitHub-specific triggers (e.g. `release` events)

Configure concurrency on a `WorkflowDefinition` with a literal group or an interpolated template:

```csharp
Options =
[
    BuildOptions.GitHub.Concurrency.Set($"{Tex.GitHub.Workflow}-{Tex.GitHub.Ref}", true),
];
```

### Dependabot Configuration

Generate Dependabot configuration alongside your workflow files:

```csharp
WorkflowPresets.GitHub.Dependabot(new()
{
    Updates =
    [
        new()
        {
            Directory = "/",
            PackageEcosystem = WorkflowLabels.GitHub.Dependabot.NugetEcosystem,
            Schedule = new() { Interval = ScheduleInterval.Daily },
        },
    ],
})
```

## See also

- [Workflow Definitions](../workflows/workflow-definitions.md): a full GitHub Actions example.
- [Workflow Options](../workflows/workflow-options.md): runner, token, and condition options.
- [DevOps Workflows](devops-workflows.md): the Azure DevOps counterpart.
- <xref:Invex.Atom.Module.GitHubWorkflows.Helpers.IGitHubWorkflows>
- <xref:Invex.Atom.Module.GitHubWorkflows.GitHub>
- <xref:Invex.Atom.Module.GitHubWorkflows.Options.GitHubTrigger>
