# Workflow Options

Workflow options fine-tune how targets behave when running as CI/CD jobs. They can be applied at the workflow level
(affecting all targets) or per-target. Reach for them when the default generated job is not quite what you need.

## Common Options

### Suppress Artifact Publishing

Prevents a target from uploading artifacts in CI (useful for validation workflows):

```csharp
BuildOptions.Target.SuppressArtifactPublishing
```

### Inject Parameters and Secrets

Inject a parameter value at workflow level:

```csharp
BuildOptions.Inject.Param(nameof(NugetDryRun), true)
```

Inject a secret from the CI platform's secret store:

```csharp
BuildOptions.Inject.Secret(nameof(IGitHubHelper.GitHubToken))
```

### Runner Selection

Set the runner from a matrix dimension:

```csharp
BuildOptions.GitHub.RunsOn.SetByMatrix
BuildOptions.DevOps.DevOpsPool.SetByMatrix
```

### Setup Steps

Add pre-job steps like installing a specific .NET SDK:

```csharp
BuildOptions.Steps.SetupDotnet.Dotnet80X()
BuildOptions.Steps.SetupDotnet.Dotnet90X()
BuildOptions.Steps.SetupDotnet.Dotnet100X()
BuildOptions.Steps.SetupDotnet.Dotnet110X()
```

For a preview or release-candidate SDK, pass an exact version, or a version pattern with a quality:

```csharp
BuildOptions.Steps.SetupDotnet.From("11.0.100-rc.2.26504.105")
BuildOptions.Steps.SetupDotnet.From("11.0.x", SetupDotnetStep.DotnetQuality.Preview)
```

Each setup step installs one SDK, so add one per SDK the job needs. For example, a .NET 11 SDK step alone doesn't
provide the .NET 10 runtime that `net10.0` tests run on.

### Checkout Configuration

Configure the checkout step for the workflow.

### Deploy to Environment

Target a specific deployment environment (with approvals, gates, etc.):

```csharp
new DeployToEnvironment("production")
```

### Conditional Execution

Run a target only when a condition is met:

```csharp
BuildOptions.Target.RunIfWorkflowCondition(
    TextExpressions.GitHub.GitHubActor.EqualToString("dependabot[bot]"))
```

### GitHub Token Permissions

Set fine-grained permissions for the `GITHUB_TOKEN`:

```csharp
new GitHubTokenPermissionsOption(new Permissions.Exact(new()
{
    Contents = PermissionsLevel.Write,
    PullRequests = PermissionsLevel.Write,
}))
```

## Applying Options

### Workflow-Level

```csharp
new("Build")
{
    Options =
    [
        BuildOptions.Inject.Param(nameof(NugetDryRun), true),
    ],
    // ...
}
```

### Per-Target

```csharp
new(nameof(PackTool))
{
    Options =
    [
        BuildOptions.Target.SuppressArtifactPublishing,
        BuildOptions.GitHub.RunsOn.SetByMatrix,
    ],
}
```

## Custom Options

Implement `IBuildOption` to create your own:

```csharp
public sealed record MyCustomOption(string Value) : IBuildOption;
```

Platform modules can then inspect the options during YAML generation.

## Next steps

- [Variables in Workflows](variables-in-workflows.md): pass values between jobs.
- [Build Options](../core-concepts/build-options.md): how options are applied and merged.
- <xref:Invex.Atom.Workflows.Options.WorkflowBuildOptionsExtensions>
- <xref:Invex.Atom.Workflows.Options.DeployToEnvironment>
- <xref:Invex.Atom.Workflows.Options.TargetCondition>
- <xref:Invex.Atom.Module.GitHubWorkflows.Options.GitHubTokenPermissionsOption>
