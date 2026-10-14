# Source Generators

Atom uses Roslyn source generators and analysers to reduce boilerplate. This page explains what they do and how they
affect your build definition.

## Packages

| Package                             | Role                                                                                                                                                   |
|-------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Invex.Atom.Build.SourceGenerators` | Generates code for `[BuildDefinition]`, `[GenerateEntryPoint]`, `[ConfigureHostBuilder]`, `[GenerateSolutionModel]`, and `[GenerateInterfaceMembers]`. |
| `Invex.Atom.Build.Analyzers`        | Reports diagnostics for common mistakes (e.g. forgetting `partial`, missing attributes).                                                               |

Both are automatically referenced when you add `Invex.Atom.Build`.

## What Gets Generated

### `[BuildDefinition]`

For a build definition decorated with `[BuildDefinition]` (either an interface extending
`IBuildDefinition` or a partial class deriving from `BuildDefinition`), the generator emits:

- `TargetDefinitions` — a dictionary mapping target names to `Target` delegates, collected from the build definition and
  all implemented interfaces.
- `ParamDefinitions` — a dictionary mapping parameter names to `ParamDefinition` records, collected from
  `[ParamDefinition]` / `[SecretDefinition]` attributes.
- `AccessParam` — a method that can read any declared parameter by name.
- An implementation of `IConfigureHost`: static `ConfigureHostBuilder` and `ConfigureHost` methods that register the
  build's module interfaces in the service collection, call every `ConfigureHostBuilderFrom{InterfaceName}` /
  `ConfigureHostFrom{InterfaceName}` hook of the implemented interfaces, and then call the optional
  `static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder)` and
  `static partial void OnConfigureHost(IHost host)` methods, which you can implement on the build class.
  `AtomHost` calls these without constructing the build definition.

For an interface build such as `IBuild`, the class is emitted as `internal sealed partial class Build` in the same
namespace (the interface name without its leading `I`), so you can add members such as `OnConfigureHostBuilder` to it
in your own `partial class Build`.

### `[GenerateEntryPoint]`

Emits a `Program` class with:

```csharp
AtomHost.Run<Build>(args);
```

### `[ConfigureHostBuilder]` / `[ConfigureHost]`

For interfaces marked with `[ConfigureHostBuilder]`, the generator declares a
`protected static partial void ConfigureHostBuilderFrom{InterfaceName}(IHostApplicationBuilder builder)` method and
ensures that when a `[BuildDefinition]` implements the interface, that method is called from the generated
`ConfigureHostBuilder` during host setup. `[ConfigureHost]` likewise declares
`protected static partial void ConfigureHostFrom{InterfaceName}(IHost host)`, called from the generated `ConfigureHost`
when `UseAtom` runs. The analyzers AT0003 and AT0002 report a missing implementation. The build interface of an
interface build can carry these attributes too.

### `[GenerateSolutionModel]`

Scans the solution file and emits a typed model with project paths.

### `[GenerateInterfaceMembers]`

Generates boilerplate interface members (e.g. forwarding properties) so module authors don't have to write them
manually.

## Analyser Diagnostics

The analyser package reports warnings and errors such as:

- Build definition class is not `partial`
- `[BuildDefinition]` is missing on a class that inherits `BuildDefinition`
- Target properties that don't follow the expected pattern

## Debugging Generators

To inspect the generated code:

1. In your `.csproj`, add:

   ```xml
   <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
   ```

2. Build the project. Generated files appear under `obj/Debug/net*/generated/`.

## Next steps

- [Testing](testing.md): test build definitions and modules.
- [Build Definitions](../core-concepts/build-definitions.md): the attributes from a consumer's point of view.
- <xref:Invex.Atom.Build.Definition.BuildDefinitionAttribute>
- <xref:Invex.Atom.Build.Hosting.GenerateEntryPointAttribute>
- <xref:Invex.Atom.Build.Definition.GenerateSolutionModelAttribute>
- <xref:Invex.Atom.Build.Definition.GenerateInterfaceMembersAttribute>
