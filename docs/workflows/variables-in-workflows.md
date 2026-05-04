# Variables in Workflows

When running locally, variables are shared in-memory between targets. In CI/CD workflows, targets may run in separate
jobs on different machines. Atom bridges this gap by mapping variables to the platform's native output/variable
mechanism.

## How It Works

1. A target declares `.ProducesVariable("BuildVersion")` and writes it with `WriteVariable`.
2. A downstream target declares `.ConsumesVariable(nameof(SetupBuildInfo), "BuildVersion")`.
3. **Locally**: the variable is stored in-memory and read directly.
4. **In CI**: the `WorkflowResolver` detects the dependency, creates a job boundary if needed, and the platform module
   maps the variable to:
    - **GitHub Actions**: job outputs (`${{ needs.job.outputs.var }}`)
    - **Azure DevOps**: pipeline variables

## Platform Variable Providers

Each platform module registers an `IVariableProvider`:

| Module                              | Provider                 | Mechanism                                          |
|-------------------------------------|--------------------------|----------------------------------------------------|
| `DecSm.Atom.Module.GithubWorkflows` | `GithubVariableProvider` | Writes to `$GITHUB_OUTPUT`, reads from job outputs |
| `DecSm.Atom.Module.DevopsWorkflows` | `DevopsVariableProvider` | Uses Azure DevOps pipeline variables               |

The built-in `AtomVariableProvider` handles local execution.

## Example

```csharp
Target SetupVersion => t => t
    .ProducesVariable("BuildVersion")
    .Executes(async ct =>
    {
        await WriteVariable("BuildVersion", "1.2.3", ct);
    });

Target Pack => t => t
    .ConsumesVariable(nameof(SetupVersion), "BuildVersion")
    .DependsOn(SetupVersion)
    .Executes(async ct =>
    {
        var version = await ReadVariable(nameof(SetupVersion), "BuildVersion", ct);
        // use version
    });
```

In the generated YAML, `SetupVersion` and `Pack` will be in separate jobs with the variable passed as a job output.

## Next Steps

→ [Debugging Workflows](debugging-workflows.md)

