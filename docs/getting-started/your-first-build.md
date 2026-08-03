# Your First Build

This guide walks you through creating a minimal Atom build and running it locally.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Option 1 — Single-File Build (Simplest)

Create a file called `Build.cs` anywhere on disk:

```csharp
#:package Invex.Atom.Build@3.*

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target SayHello => t => t
        .DescribedAs("Prints a hello world message")
        .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

Run it:

```shell
dotnet run Build.cs SayHello
```

That's it. The `#:package` directive pulls in `Invex.Atom.Build`, `[GenerateEntryPoint]` source-generates
a `Main` method, and the `SayHello` target is discovered and executed. Interface-based builds are
the recommended pattern because they compose naturally with module interfaces.

## Option 2 — Project-Based Build

For larger builds you'll typically use a dedicated project.

1. Create a new console project:

   ```shell
   dotnet new console -n _atom
   ```

2. Add the Atom package:

   ```shell
   cd _atom
   dotnet add package Invex.Atom.Build
   ```

3. Replace `Program.cs` with the recommended interface-based build definition. `[GenerateEntryPoint]`
   generates the entry point for you:

   ```csharp
   namespace Atom;
   
   [BuildDefinition]
   [GenerateEntryPoint]
   internal interface IBuild : IBuildDefinition
   {
       Target HelloWorld => t => t
           .DescribedAs("Prints a hello world message")
           .Executes(() =>
           {
               Logger.LogInformation("Hello, World!");
           });
   }
   ```

   If the build needs to override class members such as `ConfigureDefinitionHost`, use an
   `internal partial class Build : BuildDefinition` instead.

4. Run the build:

   ```shell
   dotnet run -- HelloWorld
   ```

### Expected Output

```
25-12-16 +10:00  Invex.Atom.Build.BuildExecutor:
22:46:01.754 INF Executing build

HelloWorld

25-12-16 +10:00  HelloWorld | Build:
22:46:01.790 INF Hello, World!

Build Summary

  HelloWorld │ Succeeded │ <0.01s
```

## Adding Parameters

Parameters let you pass values into targets from the command line, configuration, or environment variables.

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    [ParamDefinition("my-name", "Name to greet")]
    private string? MyName => GetParam(() => MyName);

    private Target Hello => t => t
        .DescribedAs("Prints a greeting")
        .RequiresParam(nameof(MyName))
        .Executes(() => Logger.LogInformation("Hello, {Name}!", MyName));
}
```

Run with a parameter:

```shell
dotnet run -- Hello --my-name World
```

Or interactively:

```shell
dotnet run -- Hello --interactive
```

Atom will prompt you for any required parameters that haven't been provided.

Parameters can also be supplied via `appsettings.json`:

```json
{
  "Params": {
    "my-name": "World"
  }
}
```

## Next Steps

→ [Base vs Workflow Build](base-vs-workflow-build.md) — understand when you need workflow support
