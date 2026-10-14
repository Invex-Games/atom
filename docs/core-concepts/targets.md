# Targets

A **target** is the fundamental unit of work in an Atom build. Each target has a name, an optional description,
dependencies on other targets, and one or more tasks to execute. Every build definition is made of targets, so read
this page before the rest of the core concepts.

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
    .AsHidden()          // hide from default help; still executable
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
    .DependsOn(Restore)                // name inferred from the argument expression
    .DependsOn(nameof(Compile))        // by name
```

> [!NOTE]
> When passing a target directly, the name is inferred from the argument expression, so pass the target property
> itself (e.g. `DependsOn(Restore)`) rather than a local variable. Call `DependsOn` once per dependency.

Dependencies are resolved transitively. If `Pack` depends on `Compile` and `Compile` depends on `Restore`, running
`Pack` executes `Restore → Compile → Pack`.

#### Example

This build adds a `Goodbye` target that depends on the `HelloWorld` target from
[Secrets](secrets.md#example):

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    [ParamDefinition("my-name", "My name")]
    string? MyName => GetParam(() => MyName);

    [SecretDefinition("my-secret", "My secret")]
    string? MySecret => GetParam(() => MySecret);

    Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .RequiresParam(nameof(MyName))
            .RequiresParam(nameof(MySecret))
            .Executes(() =>
                Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.", MyName, MySecret));

    Target Goodbye =>
        t => t
            .DescribedAs("Prints a goodbye message to the console")
            .DependsOn(nameof(HelloWorld))
            .Executes(() => Logger.LogInformation("Goodbye!"));
}
```

Running `atom Goodbye --my-name Frodo --my-secret TheOneRing` executes both targets in the correct order, and the
parameters only need to be provided once:

```text
26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
01:30:12.175 INF Executing build

HelloWorld

Prints a hello world message to the console

26-06-06 +10:00  HelloWorld | Atom.Build:
01:30:12.235 INF Hello, World! I am Frodo and my secret is *****.

Goodbye

Prints a goodbye message to the console

26-06-06 +10:00  Goodbye | Atom.Build:
01:30:12.240 INF Goodbye!


Build Summary

  HelloWorld │ Succeeded │ <0.01s
  Goodbye    │ Succeeded │ <0.01s
```

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

By default the base target's tasks run **before** the extending target's tasks. Pass `runExtensionAfter: true` to
reverse the order (your tasks first, then the base target's):

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

## Next steps

- [Parameters](parameters.md): pass values into targets.
- [Artifacts](artifacts.md) and [Variables](variables.md): share outputs between targets.
- <xref:Invex.Atom.Build.Definition.TargetDefinition>
- <xref:Invex.Atom.Build.Definition.Target>
- <xref:Invex.Atom.Build.IBuildAccessor>
