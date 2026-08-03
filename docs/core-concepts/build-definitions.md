# Build Definitions

A **build definition** is the central entry point for an Atom build. It declares which targets exist, what parameters
are available, and how the build host is configured. For consumer builds, the recommended approach is an interface
that extends `IBuildDefinition`; use a partial class when the build needs class-specific host configuration.

## Recommended: interface-based builds

Define the build as an interface annotated with `[BuildDefinition]` and `[GenerateEntryPoint]`:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target SayHello => t => t
        .DescribedAs("Prints a greeting")
        .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

This pattern is recommended because the build can compose targets, parameters, and host
configuration from module interfaces without putting all implementation in one type. The generated
entry point discovers the interface members and runs the build.

## Alternative: partial class builds

Use a partial class when the build itself needs to override virtual members such as
`ConfigureDefinitionHost`:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    Target SayHello => t => t
        .DescribedAs("Prints a greeting")
        .Executes(() => Logger.LogInformation("Hello, World!"));
}
```

The class must be `partial` so source generators can augment it, and it must derive from
`BuildDefinition` or `WorkflowBuildDefinition`. It can also implement module interfaces in the
same way as an interface-based build.

## What the Source Generator Does

When you apply `[BuildDefinition]`, the Atom source generator automatically:

- Discovers all `Target` properties (including those from implemented interfaces) and populates the `TargetDefinitions`
  dictionary.
- Discovers all `[ParamDefinition]` / `[SecretDefinition]` properties and populates the `ParamDefinitions` dictionary.
- Generates the `AccessParam` method so the framework can inspect parameter values.

This means you never need to manually register targets or parameters — just declare them and the generator wires
everything up.

## `[GenerateEntryPoint]`

Adding this attribute causes the source generator to emit a `Program.cs` with a `Main` method:

```csharp
// Auto-generated
AtomHost.Run<Build>(args);
```

If you need custom host configuration, omit `[GenerateEntryPoint]` and write your own entry point:

```csharp
var builder = AtomHost.CreateAtomBuilder<Build>(args);
// customise builder.Services, builder.Configuration, etc.
builder.Build().UseAtom().Run();
```

## Composing module interfaces

Targets and parameters are typically defined in **interfaces** so they can be shared across builds or published in
module packages:

```csharp
public interface IMyTargets : IBuildAccessor
{
    [ParamDefinition("greeting", "The greeting message")]
    string Greeting => GetParam(() => Greeting, "Hello");

    Target SayGreeting => t => t
        .DescribedAs("Says a greeting")
        .Executes(() => Logger.LogInformation(Greeting));
}

[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition, IMyTargets { }
```

The source generator discovers `SayGreeting` and `Greeting` from `IMyTargets` and includes them in the build model.

## `[GenerateSolutionModel]`

When applied alongside `[BuildDefinition]`, this attribute generates a typed model of your solution's projects, giving
you compile-time access to project paths.

## `ConfigureDefinitionHost`

Override `ConfigureDefinitionHost` on your build class to register additional services before the build runs:

```csharp
public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
{
    base.ConfigureDefinitionHost(builder);
    builder.Services.AddSingleton<IMyService, MyService>();
}
```

## Next Steps

→ [Targets](targets.md)
