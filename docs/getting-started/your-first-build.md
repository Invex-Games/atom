# Your First Build

This guide walks you through creating a minimal Atom build and running it locally.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) or later

## Option 1 — Single-File Build (Simplest)

Create a file called `Build.cs` anywhere on disk:

```csharp
#:package DecSm.Atom@2.*

[BuildDefinition]
[GenerateEntryPoint]
partial class Build : BuildDefinition
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

That's it. The `#:package` directive pulls in the Atom NuGet package automatically, `[GenerateEntryPoint]`
source-generates a `Main` method, and the `SayHello` target is discovered and executed.

## Option 2 — Project-Based Build

For larger builds you'll typically use a dedicated project.

1. Create a new console project:

   ```shell
   dotnet new console -n _atom
   ```

2. Add the Atom package:

   ```shell
   cd _atom
   dotnet add package DecSm.Atom
   ```

3. Replace `Program.cs` with a build definition (or use `[GenerateEntryPoint]` to have the entry point generated for
   you). Here's the minimal version with `[GenerateEntryPoint]`:

   ```csharp
   using DecSm.Atom.Build.Definition;
   using DecSm.Atom.Build.Hosting;
   
   namespace Atom;
   
   [BuildDefinition]
   [GenerateEntryPoint]
   internal partial class Build : BuildDefinition
   {
       private Target HelloWorld => t => t
           .DescribedAs("Prints a hello world message")
           .Executes(() =>
           {
               Logger.LogInformation("Hello, World!");
           });
   }
   ```

4. Run the build:

   ```shell
   dotnet run -- HelloWorld
   ```

### Expected Output

```
25-12-16 +10:00  DecSm.Atom.Build.BuildExecutor:
22:46:01.754 INF Executing build

SayHello

25-12-16 +10:00  SayHello | Build:
22:46:01.790 INF Hello, World!

Build Summary

  SayHello │ Succeeded │ <0.01s
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

