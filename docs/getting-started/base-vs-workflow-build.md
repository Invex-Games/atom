# Base Build vs Workflow Build

Atom supports interface and partial-class build definitions. Prefer the interface pathway for consumer
builds; choose the partial-class alternative when you need class-specific host configuration. In
either pathway, choose the base contract according to whether you need to generate CI/CD files.

## Basic builds

Use `IBuildDefinition` when you only need to run builds locally (or you manage your CI YAML by hand):

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    Target Compile => t => t
        .DescribedAs("Compiles the solution")
        .Executes(() => { /* ... */ });
}
```

The partial-class equivalent is:

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

Both forms provide:

- Target discovery and execution with dependency resolution
- Parameter and secret management
- Artifact and variable support
- Process runner, file system, logging, reports
- All core concepts documented in this guide

## Workflow builds

Use `IWorkflowBuildDefinition` when the build also defines CI/CD workflows. Add a platform module
interface such as `IGithubWorkflows` or `IDevopsWorkflows` to register the corresponding writer:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IWorkflowBuildDefinition, IGithubWorkflows
{
    Target Compile => t => t
        .DescribedAs("Compiles the solution")
        .Executes(() => { /* ... */ });

    Target Test => t => t
        .DescribedAs("Runs tests")
        .DependsOn(nameof(Compile))
        .Executes(() => { /* ... */ });

    IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
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

The partial-class equivalent derives from `WorkflowBuildDefinition` instead:
`internal partial class Build : WorkflowBuildDefinition, IGithubWorkflows`.

Running `dotnet run -- Gen` writes a GitHub Actions YAML file that calls the build with the correct targets.
`IGithubWorkflows` comes from `Invex.Atom.Module.GithubWorkflows`.

### What workflow builds add

| Feature              | Description                                                                                               |
|----------------------|-----------------------------------------------------------------------------------------------------------|
| `Workflows` property | Declare named workflows with triggers, targets, and platform types.                                       |
| `Gen` target         | Generates YAML for each workflow.                                                                         |
| Workflow triggers    | Push, pull request, manual (with inputs).                                                                 |
| Matrix dimensions    | Run a target across multiple OS or framework combinations.                                                |
| Workflow options     | Checkout steps, deployment environments, run conditions, etc.                                             |
| Platform modules     | `Invex.Atom.Module.GithubWorkflows` / `Invex.Atom.Module.DevopsWorkflows` add platform-specific features. |

### When to upgrade

You can start with `IBuildDefinition` and later change it to `IWorkflowBuildDefinition`; existing
targets, parameters, and module interfaces can remain unchanged while you add the `Workflows`
property and `Gen` target.

## Next Steps

→ [Build Definitions](../core-concepts/build-definitions.md) — deep dive into the `[BuildDefinition]` attribute and
source generators
