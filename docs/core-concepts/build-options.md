# Build Options

Build options are configuration flags that modify the behaviour of the build or workflow at definition time. You use them to switch module features on and to tune individual workflow targets.

## `IBuildOption`

All build options implement the marker interface `IBuildOption`. Options can be applied at three levels:

1. **Build-wide** — via the `Options` property on your build definition
2. **Workflow-wide** — via `WorkflowDefinition.Options`
3. **Per-target** — via `WorkflowTargetDefinition.Options`

```csharp
public override IReadOnlyList<IBuildOption> Options =>
[
    BuildOptions.GitVersion.ProvideBuildId,
    BuildOptions.GitVersion.ProvideBuildVersion,
];
```

## `ToggleBuildOption`

A simple on/off option:

```csharp
BuildOptions.Target.SuppressArtifactPublishing
```

## Common Options

Options are accessed via the static `BuildOptions` class, which is extended by modules. Examples:

| Option                                           | Description                                 |
|--------------------------------------------------|---------------------------------------------|
| `BuildOptions.Target.SuppressArtifactPublishing` | Prevents artifact upload for a target       |
| `BuildOptions.Steps.SetupDotnet.Dotnet80X()`     | Adds a setup step to install .NET 8         |
| `BuildOptions.GitHub.RunsOn.SetByMatrix`         | Sets the runner from the matrix dimension   |
| `BuildOptions.Inject.Param(name, value)`         | Injects a parameter value at workflow level |
| `BuildOptions.Inject.Secret(name)`               | Injects a secret from the CI platform       |

## `IBuildOptionProvider`

Modules can contribute options dynamically by implementing `IBuildOptionProvider`. The framework collects all providers
and merges their options at build time.

## Next steps

- [Hosting](hosting.md): configure the build host.
- [Workflow Options](../workflows/workflow-options.md): options that shape generated workflows.
- <xref:Invex.Atom.Build.BuildOptions.IBuildOption>
- <xref:Invex.Atom.Build.BuildOptions.BuildOptions>
- <xref:Invex.Atom.Build.BuildOptions.ToggleBuildOption>
- <xref:Invex.Atom.Build.BuildOptions.IBuildOptionProvider>
