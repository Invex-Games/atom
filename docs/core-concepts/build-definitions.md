# Build Definitions

A **build definition** is the central class of an Atom build. It declares which targets exist, what parameters are
available, and how the build host is configured.

## The `[BuildDefinition]` Attribute

Every build definition class must be:

1. Decorated with `[BuildDefinition]`
2. Marked `partial` (so source generators can augment it)
3. Derived from `BuildDefinition` (or `WorkflowBuildDefinition`)

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    // targets, parameters, etc.
}
```

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

## Composing with Interfaces

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

