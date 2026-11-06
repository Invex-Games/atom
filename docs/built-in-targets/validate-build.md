# ValidateBuild

The `ValidateBuild` target checks the Atom build configuration for common issues. Run it to catch configuration mistakes, such as undocumented targets, before they reach CI.

**Interface:** `IValidateBuild` · **Package:** `Invex.Atom.Build`

## What It Does

1. Inspects all targets in the build model.
2. Reports **warnings** for targets without descriptions.
3. Reports **errors** for critical configuration problems: a target that consumes an artifact or variable its
   producer does not produce, two parameters with the same argument name, and, in a build that implements
   `IProductsDefinition`, invalid [monorepo products](../workflows/monorepos.md#declare-the-products) (a duplicate or
   invalid name, no paths, an unsupported path glob, a folder named without a trailing `/`, an unknown or cyclic
   dependency, a tag prefix that equals or starts another product's, a GitVersion configuration shared by two
   products, or a product GitVersion configuration that is the root `GitVersion.yml`).
4. Adds warning/error lists to the build report.
5. Throws `StepFailedException` if any errors are found.

## Usage

Run directly:

```shell
dotnet run -- ValidateBuild
```

Or include as a dependency:

```csharp
Target MyTarget => t => t
    .DependsOn(nameof(IValidateBuild.ValidateBuild))
    .Executes(() => { /* ... */ });
```

## Output

Warnings appear in the build report. Example:

```text
Warnings:
  - Target 'MyTarget' has no description.
```

## See also

- [Targets](../core-concepts/targets.md): describing targets with `DescribedAs`.
- <xref:Invex.Atom.Build.IValidateBuild>
- <xref:Invex.Atom.Build.Exceptions.StepFailedException>
