# SetupBuildInfo

The `SetupBuildInfo` target initialises the build's identity — name, ID, version, and timestamp — and makes them
available as workflow variables. Include it in workflows whose later targets need the build version or ID.

**Interface:** `ISetupBuildInfo` · **Package:** `Invex.Atom.Build`

## What It Does

1. Reads `BuildName`, `BuildId`, `BuildVersion`, and `BuildTimestamp` from the
   registered [build info providers](../core-concepts/build-info.md).
2. Writes each as a workflow variable: `BuildName`, `BuildId`, `BuildVersion`, `BuildTimestamp`.
3. Adds a "Run Information" section to the build report.
4. Logs the values.

## Produced Variables

| Variable         | Description                    |
|------------------|--------------------------------|
| `BuildName`      | The logical name of the build  |
| `BuildId`        | Unique identifier for this run |
| `BuildVersion`   | Semantic version string        |
| `BuildTimestamp` | Timestamp as a string          |

## Usage

Include `SetupBuildInfo` as a dependency of your first workflow target:

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("CI")
    {
        Targets =
        [
            new(nameof(ISetupBuildInfo.SetupBuildInfo)),
            new(nameof(Compile)),
            new(nameof(Test)),
        ],
        // ...
    },
];
```

## Visibility

This target is marked as **hidden** — it doesn't appear in default help output but is still executable.

## See also

- [Build Info](../core-concepts/build-info.md): the providers behind each value.
- [Variables](../core-concepts/variables.md): consuming the produced variables.
- <xref:Invex.Atom.Build.ISetupBuildInfo>
