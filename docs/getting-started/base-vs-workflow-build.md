# Base Build vs Workflow Build

Atom offers two base classes for your build definition. Which one you choose depends on whether you need to generate
CI/CD pipeline files.

## `BuildDefinition` — The Base Build

Use `BuildDefinition` when you only need to run builds locally (or you manage your CI YAML by hand).

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    private Target Compile => t => t
        .DescribedAs("Compiles the solution")
        .Executes(() => { /* ... */ });
}
```

`BuildDefinition` gives you:

- Target discovery and execution with dependency resolution
- Parameter and secret management
- Artifact and variable support
- Process runner, file system, logging, reports
- All core concepts documented in this guide

## `WorkflowBuildDefinition` — Adding CI/CD Generation

`WorkflowBuildDefinition` extends `BuildDefinition` with the ability to define **workflows** — descriptions of how your
targets map to CI/CD jobs — and generate the corresponding YAML files.

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition
{
    private Target Compile => t => t
        .DescribedAs("Compiles the solution")
        .Executes(() => { /* ... */ });

    private Target Test => t => t
        .DescribedAs("Runs tests")
        .DependsOn(Compile)
        .Executes(() => { /* ... */ });

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("CI")
        {
            Triggers = [WorkflowTriggers.PushToMain, WorkflowTriggers.PullIntoMain],
            Targets =
            [
                new(nameof(Compile)),
                new(nameof(Test)),
            ],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}
```

Running `dotnet run -- GenerateWorkflowFiles` (or `dotnet run -- Gen`) writes a GitHub Actions YAML file that calls your
build with the correct targets.

### What `WorkflowBuildDefinition` adds

| Feature                        | Description                                                                                               |
|--------------------------------|-----------------------------------------------------------------------------------------------------------|
| `Workflows` property           | Declare named workflows with triggers, targets, and platform types.                                       |
| `GenerateWorkflowFiles` target | Generates YAML for each workflow.                                                                         |
| Workflow triggers              | Push, pull request, manual (with inputs).                                                                 |
| Matrix dimensions              | Run a target across multiple OS or framework combinations.                                                |
| Workflow options               | Checkout steps, deployment environments, run conditions, etc.                                             |
| Platform modules               | `DecSm.Atom.Module.GithubWorkflows` / `DecSm.Atom.Module.DevopsWorkflows` add platform-specific features. |

### When to upgrade

You can always start with `BuildDefinition` and switch to `WorkflowBuildDefinition` later — the change is additive. Your
existing targets, parameters, and modules continue to work unchanged; you just gain the `Workflows` property and the
`GenerateWorkflowFiles` target.

## Next Steps

→ [Build Definitions](../core-concepts/build-definitions.md) — deep dive into the `[BuildDefinition]` attribute and
source generators

