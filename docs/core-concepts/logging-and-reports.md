# Logging & Reports

Atom provides structured logging via [Spectre.Console](https://spectreconsole.net/) and a report system for capturing
build summary data.

## Logging

Every target has access to a scoped `ILogger` via `IBuildAccessor.Logger`:

```csharp
Logger.LogInformation("Building {Project}", projectName);
Logger.LogWarning("Deprecated API detected");
Logger.LogError("Compilation failed");
```

### Console Output

Atom uses Spectre.Console for rich terminal rendering, including:

- Coloured log levels
- Build summary tables
- Target execution progress

### Verbose Mode

Pass `--verbose` (or `-v`) to increase log verbosity (shows `Debug`-level output).

### Headless Mode

Pass `--headless` (or `-hl`) for non-interactive CI environments where prompts and fancy rendering should be disabled.

### Log Masking

Values marked as secrets (via `[SecretDefinition]`) are automatically masked in all log output.

## Reports

The report system lets targets contribute structured data to the build summary.

### `IReportsHelper`

Targets implementing `IReportsHelper` can add report data:

```csharp
AddReportData(new TextReportData("Build completed successfully")
{
    Title = "Status",
});

AddReportData(new ListReportData(["Warning 1", "Warning 2"])
{
    Title = "Warnings",
});
```

### Report Writers

Report data is rendered by `IOutcomeReportWriter` implementations:

| Writer                             | Description                              |
|------------------------------------|------------------------------------------|
| `ConsoleOutcomeReportWriter`       | Writes to the console (always active)    |
| `GithubSummaryOutcomeReportWriter` | Writes to the GitHub Actions job summary |
| `DevopsSummaryOutcomeReportWriter` | Writes to the Azure DevOps build summary |

Platform-specific writers are registered by their respective modules.

## Next Steps

→ [File Transformations](file-transformations.md)

