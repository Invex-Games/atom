# Hosting

Atom uses the standard .NET `IHost` / `HostApplicationBuilder` infrastructure under the hood. The `AtomHost` class
provides convenience methods for setting up and running the host. Read this when you need custom services, configuration sources, or your own entry point.

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
`ConfigureHostBuilderFrom{InterfaceName}` when the interface is implemented:

```csharp
[ConfigureHostBuilder]
public partial interface IGitVersion
{
    protected static partial void ConfigureHostBuilderFromIGitVersion(IHostApplicationBuilder builder) =>
        builder.Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
```

When your `Build` class implements `IGitVersion`, the generated code automatically calls
`ConfigureHostBuilderFromIGitVersion` during host setup. (`[ConfigureHost]` works the same way for configuring the built
`IHost`, with a method named `ConfigureHostFrom{InterfaceName}`.) The Atom analyzers (AT0002/AT0003) verify that the
required partial method is implemented.

## `OnConfigureHostBuilder` / `OnConfigureHost`

Every generated build class implements <xref:Invex.Atom.Build.Hosting.IConfigureHost> with two static methods.
`ConfigureHostBuilder` runs when `AddAtom` configures the builder and `ConfigureHost` runs when `UseAtom` is called on
the built host. Each calls the module hooks above, then an optional static partial method you can implement for one-off
configuration:

```csharp
[BuildDefinition]
internal partial class Build : BuildDefinition, IGitVersion
{
    static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IMyService, MyService>();

    static partial void OnConfigureHost(IHost host) =>
        host.Services.GetRequiredService<IMyService>().Initialize();
}
```

Both methods are optional; leave out the ones you do not need. They are static because they run before the build
definition is constructed, so they cannot use instance members such as `GetParam`; resolve services from
`host.Services` instead.

For an interface-based build (`[BuildDefinition] internal interface IBuild`), the generator emits
`internal sealed partial class Build` in the same namespace, named after the interface without its leading `I`. Either
implement the hooks in a `partial class Build`, or make the build interface itself a host hook source:

```csharp
// Option 1: extend the generated class
internal sealed partial class Build
{
    static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IMyService, MyService>();
}

// Option 2: mark the build interface (which must then be partial)
[BuildDefinition]
[GenerateEntryPoint]
[ConfigureHostBuilder]
internal partial interface IBuild : IBuildDefinition
{
    protected static partial void ConfigureHostBuilderFromIBuild(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IMyService, MyService>();
}
```

If you call `AddAtom` yourself, also call `UseAtom()` on the built host so that `ConfigureHost` runs. `AtomHost.Run`
and `[GenerateEntryPoint]` do this for you.

## Next steps

- [Lifecycle Hooks](lifecycle-hooks.md): run code before and after target execution.
- [Writing a Module](../developer-guide/writing-a-module.md): register services from a module interface.
- <xref:Invex.Atom.Build.Hosting.AtomHost>
- <xref:Invex.Atom.Build.Hosting.GenerateEntryPointAttribute>
- <xref:Invex.Atom.Build.Hosting.ConfigureHostBuilderAttribute>
- <xref:Invex.Atom.Build.Hosting.ConfigureHostAttribute>
- <xref:Invex.Atom.Build.Hosting.IConfigureHost>
