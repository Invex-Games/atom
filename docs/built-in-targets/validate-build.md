# ValidateBuild

**Interface:** `IValidateBuild`  
**Package:** `Invex.Atom.Build`

The `ValidateBuild` target checks the Atom build configuration for common issues.

## What It Does

1. Inspects all targets in the build model.
2. Reports **warnings** for targets without descriptions.
3. Reports **errors** for critical configuration problems (extensible).
4. Adds warning/error lists to the build report.
5. Throws `StepFailedException` if any errors are found.

## Usage

Run directly:

```shell
dotnet run -- ValidateBuild
```

Or include as a dependency:

```csharp
Target MyTarget => t => t
    .DependsOn(nameof(IValidateBuild.ValidateBuild))
    .Executes(() => { /* ... */ });
```

## Output

Warnings appear in the build report. Example:

```
Warnings:
  - Target 'MyTarget' has no description.
```

