# Variables

Variables allow targets to share simple string values with downstream targets. Unlike parameters (which are inputs),
variables are **outputs** produced during execution.

## Producing a Variable

Declare and write a variable:

```csharp
Target SetupVersion => t => t
    .ProducesVariable("BuildVersion")
    .Executes(async cancellationToken =>
    {
        var version = "1.2.3";
        await WriteVariable("BuildVersion", version, cancellationToken);
    });
```

`WriteVariable` is available via the `IVariablesHelper` interface.

## Consuming a Variable

Declare that a target consumes a variable from another target:

```csharp
Target Pack => t => t
    .ConsumesVariable(nameof(SetupVersion), "BuildVersion")
    .Executes(async cancellationToken =>
    {
        var version = await ReadVariable(nameof(SetupVersion), "BuildVersion", cancellationToken);
        Logger.LogInformation("Packing version {Version}", version);
    });
```

## How Variables Work

- **Locally**, variables are stored in-memory and shared between targets in the same process.
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

