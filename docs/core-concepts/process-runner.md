# Process Runner

`IProcessRunner` provides a structured way to execute external processes with logging, output capture, and error
handling built in.

> [!NOTE]
> The process runner types (`IProcessRunner`, `ProcessRunOptions`, `ProcessRunResult`) ship in the standalone
> [`Invex.Process`](https://www.nuget.org/packages/Invex.Process) package, maintained in its own repository. You don't
> need to reference it directly — it is pulled in transitively by `Invex.Atom.Build` and surfaced through
> `IBuildAccessor.ProcessRunner`.

## Basic Usage

Access the process runner via `IBuildAccessor.ProcessRunner`:

```csharp
Target Build => t => t
    .Executes(async cancellationToken =>
    {
        await ProcessRunner.RunAsync(
            new ProcessRunOptions("dotnet", "build --configuration Release"),
            cancellationToken);
    });
```

## `ProcessRunOptions`

| Property               | Default       | Description                                              |
|------------------------|---------------|----------------------------------------------------------|
| `Name`                 | *(required)*  | Executable name or path (`"dotnet"`, `"git"`, etc.)      |
| `Args`                 | *(required)*  | Command-line arguments as a string or `string[]`         |
| `WorkingDirectory`     | `null`        | Working directory; inherits current if `null`            |
| `InvocationLogLevel`   | `Information` | Log level for the invocation line                        |
| `OutputLogLevel`       | `Debug`       | Log level for each stdout line                           |
| `ErrorLogLevel`        | `Warning`     | Log level for each stderr line                           |
| `AllowFailedResult`    | `false`       | If `false`, non-zero exit code throws                    |
| `EnvironmentVariables` | `{}`          | Extra env vars to inject; `null` value removes a var     |
| `TransformOutput`      | `null`        | Per-line transform for stdout; return `null` to suppress |
| `TransformError`       | `null`        | Per-line transform for stderr; return `null` to suppress |

### Array-Based Arguments

```csharp
new ProcessRunOptions("dotnet", ["build", "--configuration", configuration, verbosityFlag])
```

Empty or whitespace-only entries are automatically filtered out, so you can conditionally include arguments without
string concatenation.

## `ProcessRunResult`

Both `Run` and `RunAsync` return a `ProcessRunResult` containing:

- `ExitCode` — the process exit code
- `Output` — captured stdout lines
- `Error` — captured stderr lines

## Error Handling

By default, a non-zero exit code throws an exception that fails the enclosing target. Set `AllowFailedResult = true` to
handle failures yourself:

```csharp
var result = await ProcessRunner.RunAsync(
    new ProcessRunOptions("dotnet", "test") { AllowFailedResult = true },
    cancellationToken);

if (result.ExitCode != 0)
    Logger.LogWarning("Tests failed with exit code {Code}", result.ExitCode);
```

## Next Steps

→ [Build Info](build-info.md)

