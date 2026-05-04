# Workflow Options

Workflow options fine-tune how targets behave when running as CI/CD jobs. They can be applied at the workflow level (
affecting all targets) or per-target.

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
BuildOptions.Inject.Secret(nameof(IGithubHelper.GithubToken))
```

### Runner Selection

Set the runner from a matrix dimension:

```csharp
BuildOptions.Github.RunsOn.SetByMatrix
BuildOptions.Devops.DevopsPool.SetByMatrix
```

### Setup Steps

Add pre-job steps like installing a specific .NET SDK:

```csharp
BuildOptions.Steps.SetupDotnet.Dotnet80X()
BuildOptions.Steps.SetupDotnet.Dotnet90X()
BuildOptions.Steps.SetupDotnet.Dotnet100X()
```

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
    TextExpressions.Github.GithubActor.EqualToString("dependabot[bot]"))
```

### GitHub Token Permissions

Set fine-grained permissions for the `GITHUB_TOKEN`:

```csharp
new GithubTokenPermissionsOption(new Permissions.Exact(new()
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
        BuildOptions.Github.RunsOn.SetByMatrix,
    ],
}
```

## Custom Options

Implement `IBuildOption` to create your own:

```csharp
public sealed record MyCustomOption(string Value) : IBuildOption;
```

Platform modules can then inspect the options during YAML generation.

## Next Steps

→ [Variables in Workflows](variables-in-workflows.md)

