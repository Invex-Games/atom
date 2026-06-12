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

For a class decorated with `[BuildDefinition]`, the generator emits:

- `TargetDefinitions` — a dictionary mapping target names to `Target` delegates, collected from the class and all
  implemented interfaces.
- `ParamDefinitions` — a dictionary mapping parameter names to `ParamDefinition` records, collected from
  `[ParamDefinition]` / `[SecretDefinition]` attributes.
- `AccessParam` — a method that can read any declared parameter by name.

### `[GenerateEntryPoint]`

Emits a `Program` class with:

```csharp
AtomHost.Run<Build>(args);
```

### `[ConfigureHostBuilder]`

For interfaces marked with `[ConfigureHostBuilder]`, the generator declares a `ConfigureBuilderFrom{InterfaceName}`
partial method and ensures that when a `[BuildDefinition]` class implements the interface, that method is called during
host setup.

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

## Next Steps

→ [Testing](testing.md)

