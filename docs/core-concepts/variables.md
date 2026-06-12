# Variables

Variables allow targets to share simple string values with downstream targets. Unlike parameters (which are inputs),
variables are **outputs** produced during execution.

Every variable is backed by a **parameter**: the variable name must match a property declared with `[ParamDefinition]`.
Producing a target writes the value; consuming targets read it back through the same parameter.

## Producing a Variable

Declare a backing parameter, then declare and write the variable:

```csharp
[ParamDefinition("build-version", "The computed build version")]
string? BuildVersion => GetParam(() => BuildVersion);

Target SetupVersion => t => t
    .ProducesVariable(nameof(BuildVersion))
    .Executes(async cancellationToken =>
    {
        await WriteVariable(nameof(BuildVersion), "1.2.3", cancellationToken);
    });
```

`WriteVariable` is available via the `IVariablesHelper` interface.

## Consuming a Variable

Declare that a target consumes a variable from another target, then read the value through the backing parameter:

```csharp
Target Pack => t => t
    .ConsumesVariable(nameof(SetupVersion), nameof(BuildVersion))
    .Executes(() =>
    {
        // BuildVersion now resolves to the value written by SetupVersion
        Logger.LogInformation("Packing version {Version}", BuildVersion);
    });
```

Before the target runs, the `BuildExecutor` automatically loads each consumed variable into the execution context. If a
consumed variable has no value, the target fails with a clear error message.

## How Variables Work

- **Locally**, variables are persisted in a job-scoped JSON file in the Atom temp directory and loaded into the
  environment before consuming targets run.
- **In workflows**, variables are mapped to the platform's native mechanism (e.g. GitHub Actions job outputs, Azure
  DevOps pipeline variables) so they can cross job boundaries.

## `IVariableProvider`

The framework uses `IVariableProvider` implementations to read and write variables. Multiple providers form a chain of
responsibility:

```csharp
public interface IVariableProvider
{
    Task<bool> WriteVariable(string variableName, string variableValue, CancellationToken ct);
    Task<bool> ReadVariable(string jobName, string variableName, CancellationToken ct);
}
```

The built-in `AtomVariableProvider` handles local execution. Platform modules (GitHub, DevOps) register their own
providers for CI environments.

## Next Steps

→ [File System](file-system.md)

