# Hosting

Atom uses the standard .NET `IHost` / `HostApplicationBuilder` infrastructure under the hood. The `AtomHost` class
provides convenience methods for setting up and running the host.

## `AtomHost.Run<T>(args)`

The simplest way to start an Atom build:

```csharp
AtomHost.Run<Build>(args);
```

This is what `[GenerateEntryPoint]` generates for you in `Program.cs`.

## `AtomHost.CreateAtomBuilder<T>(args)`

For custom host configuration, use the builder pattern:

```csharp
var builder = AtomHost.CreateAtomBuilder<Build>(args);

// Add your own services
builder.Services.AddSingleton<IMyService, MyService>();

// Add configuration sources
builder.Configuration.AddJsonFile("custom.json", optional: true);

builder.Build().UseAtom().Run();
```

The builder automatically:

- Loads `appsettings.json` and `appsettings.{env}.json`
- Reads the `DOTNET_ENVIRONMENT` / `ASPNETCORE_ENVIRONMENT` variable
- Registers all core Atom services

## `[GenerateEntryPoint]`

Apply this attribute to your build definition class to have the source generator create the entry point:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition { /* ... */ }
```

If you need full control over `Main`, omit this attribute and write your own.

## `[ConfigureHostBuilder]` / `[ConfigureHost]`

Module interfaces can use `[ConfigureHostBuilder]` to have the source generator call a static partial method named
`ConfigureBuilderFrom{InterfaceName}` when the interface is implemented:

```csharp
[ConfigureHostBuilder]
public partial interface IGitVersion
{
    protected static partial void ConfigureBuilderFromIGitVersion(IHostApplicationBuilder builder) =>
        builder.Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
```

When your `Build` class implements `IGitVersion`, the generated code automatically calls
`ConfigureBuilderFromIGitVersion` during host setup. (`[ConfigureHost]` works the same way for configuring the built
`IHost`, with a method named `ConfigureHostFrom{InterfaceName}`.) The Atom analyzers (AT0002/AT0003) verify that the
required partial method is implemented.

## `ConfigureDefinitionHost`

Override this virtual method on your build class for one-off service registration:

```csharp
public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
{
    base.ConfigureDefinitionHost(builder);
    builder.Services.AddSingleton<IMyService, MyService>();
}
```

## Next Steps

→ [Lifecycle Hooks](lifecycle-hooks.md)

