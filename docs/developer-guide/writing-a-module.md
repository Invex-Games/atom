# Writing a Module

This guide covers how to create a reusable Atom module and publish it as a NuGet package. Write one when several builds need the same targets, parameters, or services.

## What Is a Module?

A module is a NuGet package that provides one or more of:

- **Targets** — reusable build steps
- **Parameters / Secrets** — configuration the module needs
- **Service registrations** — DI services (providers, helpers)
- **Build options** — workflow-level configuration

## Project Setup

1. Create a class library:

   ```shell
   dotnet new classlib -n Invex.Atom.Module.MyModule
   ```

2. Add a reference to `Invex.Atom.Build` (and `Invex.Atom.Workflows` if your module contributes workflow features):

   ```xml
   <PackageReference Include="Invex.Atom.Build" Version="3.*" />
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

    protected static partial void ConfigureHostBuilderFromIMyModule(IHostApplicationBuilder builder) =>
        builder.Services.TryAddSingleton<IMyService, MyService>();
}
```

### Key Points

- **`[ConfigureHostBuilder]`** + `static partial void ConfigureHostBuilderFrom{InterfaceName}` — the source generator
  declares this partial method (named after the interface, e.g. `ConfigureHostBuilderFromIMyModule`) and calls it when a
  consumer implements the interface. This is how you register services. The `AT0003` analyzer reports an error if the
  method body is missing. The hook is static and runs before any build definition is constructed.
- **`[ConfigureHost]`** + `static partial void ConfigureHostFrom{InterfaceName}(IHost host)` — the same pattern for
  configuring the built host; it runs when `UseAtom` is called. `AT0002` reports a missing body.
- **`IBuildAccessor`** — gives the interface access to `Logger`, `RootedFileSystem`, `ProcessRunner`, `GetParam`, etc.
- **Default implementations** — both targets and parameters use default interface members, so consumers don't need to
  implement anything.

## Extend Build Options (Optional)

If your module needs workflow-level options, define your own option type and expose it via the static `BuildOptions`
class. A simple on/off toggle can derive from `ToggleBuildOption`:

```csharp
// A toggle option (Enabled defaults to true).
public sealed record UseSomethingOption : ToggleBuildOption;

public static class BuildOptions
{
    public static class MyModule
    {
        public static UseSomethingOption UseSomething => new();
        public static UseSomethingOption DisableSomething => new() { Enabled = false };
    }
}
```

Check the toggle with the `IsEnabled` extension when generating workflows.

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

- Name your package `Invex.Atom.Module.<Name>` (or your own prefix).
- Prefix parameter CLI names to avoid collisions (e.g. `mymodule-setting`).
- Mark the interface with `[PublicAPI]` for API documentation.
- Provide XML doc comments on all public members.
- Include a `.props` file to auto-import common namespaces.

## Next steps

- [Custom Providers](custom-providers.md): extension points a module can implement.
- [Hosting](../core-concepts/hosting.md): `[ConfigureHostBuilder]` and `[ConfigureHost]`.
- <xref:Invex.Atom.Build.IBuildAccessor>
- <xref:Invex.Atom.Build.Hosting.ConfigureHostBuilderAttribute>
- <xref:Invex.Atom.Build.BuildOptions.ToggleBuildOption>
