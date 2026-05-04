# Writing a Module

This guide covers how to create a reusable Atom module and publish it as a NuGet package.

## What Is a Module?

A module is a NuGet package that provides one or more of:

- **Targets** — reusable build steps
- **Parameters / Secrets** — configuration the module needs
- **Service registrations** — DI services (providers, helpers)
- **Build options** — workflow-level configuration

## Project Setup

1. Create a class library:

   ```shell
   dotnet new classlib -n DecSm.Atom.Module.MyModule
   ```

2. Add a reference to `DecSm.Atom.Build` (and `DecSm.Atom.Workflows` if your module contributes workflow features):

   ```xml
   <PackageReference Include="DecSm.Atom.Build" Version="2.*" />
   ```

3. Create a `.props` file (optional but recommended) to auto-import usings when consumers reference your package.

## Define the Module Interface

Modules expose their functionality through interfaces that extend `IBuildAccessor`:

```csharp
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IMyModule : IBuildAccessor
{
    [ParamDefinition("my-setting", "A setting for my module")]
    string? MySetting => GetParam(() => MySetting);

    Target MyTarget => t => t
        .DescribedAs("Does something useful")
        .RequiresParam(nameof(MySetting))
        .Executes(() =>
        {
            Logger.LogInformation("Setting: {Setting}", MySetting);
        });

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IMyService, MyService>();
}
```

### Key Points

- **`[ConfigureHostBuilder]`** + `static partial void ConfigureBuilder` — the source generator calls this method when a
  consumer implements the interface. This is how you register services.
- **`IBuildAccessor`** — gives the interface access to `Logger`, `AtomFileSystem`, `ProcessRunner`, `GetParam`, etc.
- **Default implementations** — both targets and parameters use default interface members, so consumers don't need to
  implement anything.

## Extend Build Options (Optional)

If your module needs workflow-level options, extend the static `BuildOptions` class:

```csharp
public static class BuildOptions
{
    public static class MyModule
    {
        public static IBuildOption UseSomething => new ToggleBuildOption("MyModule.UseSomething");
    }
}
```

## Package and Publish

1. Set the appropriate NuGet metadata in your `.csproj`.
2. Pack: `dotnet pack`
3. Publish to NuGet or a private feed.

## Consumer Usage

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IMyModule { }
```

The consumer just implements the interface — all targets, parameters, and services appear automatically.

## Conventions

- Name your package `DecSm.Atom.Module.<Name>` (or your own prefix).
- Prefix parameter CLI names to avoid collisions (e.g. `mymodule-setting`).
- Mark the interface with `[PublicAPI]` for API documentation.
- Provide XML doc comments on all public members.
- Include a `.props` file to auto-import common namespaces.

## Next Steps

→ [Custom Providers](custom-providers.md)

