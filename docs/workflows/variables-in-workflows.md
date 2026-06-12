# Variables in Workflows

When running locally, variables are shared in-memory between targets. In CI/CD workflows, targets may run in separate
jobs on different machines. Atom bridges this gap by mapping variables to the platform's native output/variable
mechanism.

## How It Works

1. A target declares `.ProducesVariable(nameof(BuildVersion))` and writes it with `WriteVariable`.
2. A downstream target declares `.ConsumesVariable(nameof(SetupVersion), nameof(BuildVersion))`.
3. **Locally**: the variable is persisted to a job-scoped JSON file and loaded back into the consuming target.
4. **In CI**: the `WorkflowResolver` detects the dependency, creates a job boundary if needed, and the platform module
   maps the variable to:
    - **GitHub Actions**: job outputs (`${{ needs.job.outputs.var }}`)
    - **Azure DevOps**: pipeline variables

## Platform Variable Providers

Each platform module registers an `IVariableProvider`:

| Module                              | Provider                 | Mechanism                                          |
|-------------------------------------|--------------------------|----------------------------------------------------|
| `Invex.Atom.Module.GithubWorkflows` | `GithubVariableProvider` | Writes to `$GITHUB_OUTPUT`, reads from job outputs |
| `Invex.Atom.Module.DevopsWorkflows` | `DevopsVariableProvider` | Uses Azure DevOps pipeline variables               |

The built-in `AtomVariableProvider` handles local execution.

## Example

A variable is always backed by a `[ParamDefinition]` property of the same name. The producing target writes the value;
consuming targets read it back through that parameter (the executor loads consumed variables automatically before the
target runs):

```csharp
[ParamDefinition("build-version", "The computed build version")]
string? BuildVersion => GetParam(() => BuildVersion);

Target SetupVersion => t => t
    .ProducesVariable(nameof(BuildVersion))
    .Executes(async ct =>
    {
        await WriteVariable(nameof(BuildVersion), "1.2.3", ct);
    });

Target Pack => t => t
    .DependsOn(SetupVersion)
    .ConsumesVariable(nameof(SetupVersion), nameof(BuildVersion))
    .Executes(() =>
    {
        // BuildVersion resolves to the value written by SetupVersion
        Logger.LogInformation("Packing version {Version}", BuildVersion);
    });
```

In the generated YAML, `SetupVersion` and `Pack` will be in separate jobs with the variable passed as a job output.

## Next Steps

→ [Debugging Workflows](debugging-workflows.md)

