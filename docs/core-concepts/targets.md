# Targets

A **target** is the fundamental unit of work in an Atom build. Each target has a name, an optional description,
dependencies on other targets, and one or more tasks to execute.

## Defining a Target

Targets are declared as properties of type `Target` (a delegate alias for `Func<TargetDefinition, TargetDefinition>`):

```csharp
private Target Compile => t => t
    .DescribedAs("Compiles the solution")
    .Executes(() =>
    {
        // build logic here
    });
```

The property name becomes the target name on the command line: `dotnet run -- Compile`.

## Fluent API

`TargetDefinition` exposes a fluent API for configuring every aspect of a target:

### Description & Visibility

```csharp
t => t
    .DescribedAs("Human-readable description for help output")
    .IsHidden()          // hide from default help; still executable
    .WithAlias("c")      // short alias for the CLI
```

### Execution

Targets can execute synchronous actions, async tasks, or async tasks with cancellation:

```csharp
// Synchronous
t => t.Executes(() => Console.WriteLine("done"))

// Async
t => t.Executes(async () => await DoWorkAsync())

// Async with CancellationToken
t => t.Executes(async cancellationToken => await DoWorkAsync(cancellationToken))
```

You can call `.Executes()` multiple times — tasks run in order.

### Dependencies

```csharp
t => t
    .DependsOn(Restore)               // inferred from property expression
    .DependsOn(nameof(Compile))        // by name
    .DependsOn(Compile, Test)          // multiple at once
```

Dependencies are resolved transitively. If `Pack` depends on `Compile` and `Compile` depends on `Restore`, running
`Pack` executes `Restore → Compile → Pack`.

### Parameters

```csharp
t => t
    .RequiresParam(nameof(MyName))     // build fails if not provided
    .UsesParam(nameof(Verbosity))      // optional; documented but not enforced
```

### Artifacts

```csharp
t => t
    .ProducesArtifact("packages")
    .ConsumesArtifact(nameof(Pack), "packages")
```

### Variables

```csharp
t => t
    .ProducesVariable("BuildVersion")
    .ConsumesVariable(nameof(SetupBuildInfo), "BuildVersion")
```

## Extending Targets

A target can extend another target defined in an interface, inheriting its tasks, dependencies, parameters, artifacts,
and variables:

```csharp
Target MyCompile => t => t
    .Extends<IDotnetTargets>(x => x.DotnetBuild)
    .Executes(() => Logger.LogInformation("Extra step after build"));
```

By default the extending target's tasks run **before** the base target's tasks. Pass `runExtensionAfter: true` to
reverse the order:

```csharp
.Extends<IDotnetTargets>(x => x.DotnetBuild, runExtensionAfter: true)
```

## Accessing Services

Inside a target body you have access to several built-in services via `IBuildAccessor`:

| Property         | Type               | Description                                   |
|------------------|--------------------|-----------------------------------------------|
| `Logger`         | `ILogger`          | Structured logger scoped to the current type. |
| `RootedFileSystem` | `IRootedFileSystem`  | File system abstraction with path resolution. |
| `ProcessRunner`  | `IProcessRunner`   | Execute external processes.                   |
| `Services`       | `IServiceProvider` | Full DI container.                            |

Use `GetService<T>()` or `GetServices<T>()` for any other registered service.

## Running Targets

```shell
# Single target
dotnet run -- Compile

# Multiple targets
dotnet run -- Compile Test

# Skip dependencies
dotnet run -- Pack --skip
```

## Next Steps

→ [Parameters](parameters.md)

