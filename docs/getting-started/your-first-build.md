# Your First Build

This guide walks you through creating a minimal Atom build, running it locally, and passing it a parameter. Start
here if you have not used Atom before.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Optionally, the `atom` global tool, which finds and runs your build from any directory below it:

  ```shell
  dotnet tool install --global Invex.Atom.Tool
  ```

> [!NOTE]
> The examples below use `atom <target>`. The .NET CLI works too: `dotnet run --project _atom -- <target>` for a
> project-based build, or `dotnet run Build.cs <target>` for a file-based build. See the [CLI reference](../reference/cli.md).

## Option 1: project-based build

Most repositories keep their build in a dedicated project, conventionally named `_atom`.

1. Create a .NET 10 project:

   ```shell
   dotnet new console -n _atom
   ```

2. Replace the contents of `_atom.csproj`. The Worker SDK provides the hosting and logging implicit usings that
   build definitions rely on:

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Worker">

     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <RootNamespace>Atom</RootNamespace>
     </PropertyGroup>

     <ItemGroup>
       <PackageReference Include="Invex.Atom.Build" Version="3.*" />
     </ItemGroup>

   </Project>
   ```

3. Replace `Program.cs` with `IBuild.cs`. `[GenerateEntryPoint]` generates the `Main` method for you:

   ```csharp
   using Invex.Atom.Build.Definition;
   using Invex.Atom.Build.Hosting;

   namespace Atom;

   [BuildDefinition]
   [GenerateEntryPoint]
   internal interface IBuild : IBuildDefinition
   {
       Target HelloWorld =>
           t => t
               .DescribedAs("Prints a hello world message to the console")
               .Executes(() => Logger.LogInformation("Hello, World!"));
   }
   ```

   The `using` lines are optional: `Invex.Atom.Build` adds its namespaces as global usings when referenced from NuGet.
   If you prefer to write the build as a class, for example to keep host hooks such as `OnConfigureHostBuilder` in the
   same type, use an `internal partial class Build : BuildDefinition` instead (see [Build Definitions](../core-concepts/build-definitions.md)).

4. Run the build:

   ```shell
   atom HelloWorld
   ```

   ```text
   26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
   01:16:47.552 INF Executing build

   HelloWorld

   Prints a hello world message to the console

   26-06-06 +10:00  HelloWorld | Atom.Build:
   01:16:47.616 INF Hello, World!


   Build Summary

     HelloWorld │ Succeeded │ <0.01s
   ```

## Option 2: file-based build

Atom can also run as a .NET file-based app. Create `Atom.cs` anywhere on disk:

```csharp
#:sdk Microsoft.NET.Sdk.Worker
#:package Invex.Atom.Build@3.*

using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;

namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

Run it:

```shell
dotnet run Atom.cs HelloWorld
```

The `#:package` directive pulls in `Invex.Atom.Build`, `[GenerateEntryPoint]` source-generates a `Main` method, and
the `HelloWorld` target is discovered and executed. Interface-based builds are the recommended pattern because they
compose naturally with module interfaces.

## Adding parameters

Parameters let you pass values into targets from the command line, configuration, or environment variables. Add a
parameter to `IBuild` and require it on the target:

```csharp
using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;
using Invex.Atom.Build.Params;

namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    [ParamDefinition("my-name", "My name")]
    string? MyName => GetParam(() => MyName);

    Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .RequiresParam(nameof(MyName))
            .Executes(() => Logger.LogInformation("Hello, World! I am {MyName}.", MyName));
}
```

> [!NOTE]
> `.RequiresParam(nameof(...))` ensures the parameter is provided before the target is executed.
> `.UsesParam(nameof(...))` uses the parameter if it is provided, but does not fail the build if it is not.

Run with a parameter:

```shell
atom HelloWorld --my-name Frodo
```

```text
26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
01:23:25.405 INF Executing build

HelloWorld

Prints a hello world message to the console

26-06-06 +10:00  HelloWorld | Atom.Build:
01:23:25.467 INF Hello, World! I am Frodo.


Build Summary

  HelloWorld │ Succeeded │ <0.01s
```

Or run interactively, and Atom prompts for any required parameters that have not been provided:

```shell
atom HelloWorld --interactive
```

Parameters can also be supplied via `appsettings.json`:

```json
{
  "Params": {
    "my-name": "Frodo"
  }
}
```

## Next steps

- [Secrets](../core-concepts/secrets.md): add a secret parameter that is masked in the output.
- [Targets](../core-concepts/targets.md): add a second target that depends on `HelloWorld`.
- [Base vs Workflow Build](base-vs-workflow-build.md): understand when you need workflow support.
- [Workflow Definitions](../workflows/workflow-definitions.md): generate a GitHub Actions workflow for this build.
