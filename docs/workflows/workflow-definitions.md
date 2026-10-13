# Workflow Definitions

A `WorkflowDefinition` describes a single CI/CD pipeline — its name, triggers, targets, options, and which platforms it
applies to. You need one for every pipeline you want Atom to generate from a workflow build.

## Basic Structure

```csharp
public override IReadOnlyList<WorkflowDefinition> Workflows =>
[
    new("CI")
    {
        Triggers = [WorkflowTriggers.PushToMain, WorkflowTriggers.PullIntoMain],
        Targets =
        [
            new(nameof(SetupBuildInfo)),
            new(nameof(Compile)),
            new(nameof(Test)),
        ],
        Types = [WorkflowTypes.Github.Action],
    },
];
```

## Example: Generating a GitHub Actions Workflow

This walkthrough turns the `HelloWorld` / `Goodbye` build from [Targets](../core-concepts/targets.md#example) into a
GitHub Actions workflow.

1. Reference the GitHub Workflows module in `_atom.csproj` (it brings in `Invex.Atom.Build` and `Invex.Atom.Workflows`
   transitively):

   ```xml
   <Project Sdk="Microsoft.NET.Sdk.Worker">

     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <RootNamespace>Atom</RootNamespace>
     </PropertyGroup>

     <ItemGroup>
       <PackageReference Include="Invex.Atom.Module.GithubWorkflows" Version="3.*" />
     </ItemGroup>

   </Project>
   ```

2. Extend `IWorkflowBuildDefinition` and `IGithubWorkflows`, and declare the workflow. The parameter is injected from a
   GitHub expression and the secret from the repository's secrets:

   ```csharp
   using Invex.Atom.Build.BuildOptions;
   using Invex.Atom.Build.Definition;
   using Invex.Atom.Build.Hosting;
   using Invex.Atom.Build.Params;
   using Invex.Atom.Module.GithubWorkflows.Extensions;
   using Invex.Atom.Module.GithubWorkflows.Helpers;
   using Invex.Atom.Workflows;
   using Invex.Atom.Workflows.Definition;
   using Invex.Atom.Workflows.Definition.Triggers;
   using Invex.Atom.Workflows.Options;
   using Invex.StructuredText.Expressions;

   namespace Atom;

   [BuildDefinition]
   [GenerateEntryPoint]
   internal interface IBuild : IWorkflowBuildDefinition, IGithubWorkflows
   {
       [ParamDefinition("my-name", "My name")]
       string? MyName => GetParam(() => MyName);

       [SecretDefinition("my-secret", "My secret")]
       string? MySecret => GetParam(() => MySecret);

       Target HelloWorld =>
           t => t
               .DescribedAs("Prints a hello world message to the console")
               .RequiresParam(nameof(MyName))
               .RequiresParam(nameof(MySecret))
               .Executes(() =>
                   Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.", MyName, MySecret));

       Target Goodbye =>
           t => t
               .DescribedAs("Prints a goodbye message to the console")
               .DependsOn(nameof(HelloWorld))
               .Executes(() => Logger.LogInformation("Goodbye!"));

       IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
       [
           new("Hello")
           {
               Triggers = [WorkflowTriggers.PushToMain],
               Targets =
               [
                   new(nameof(HelloWorld))
                   {
                       Options =
                       [
                           BuildOptions.Inject.Param(nameof(MyName), TextExpressions.Github.GithubRepositoryOwner),
                           BuildOptions.Inject.Secret(nameof(MySecret)),
                       ],
                   },
                   new(nameof(Goodbye)),
               ],
               Types = [WorkflowTypes.Github.Action],
           },
       ];
   }
   ```

3. Run the [`Gen`](../built-in-targets/generate-workflow-files.md) target with `atom gen`:

   ```text
   26-06-06 +10:00  Invex.Atom.Module.GithubWorkflows.GithubActions.GithubWorkflowFileWriter:
   01:38:57.388 INF Writing new workflow file:
                    L:\Repos\Invex-Games\atom\.github\workflows\Hello.yml

   26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
   01:38:57.472 INF Executing build

   Gen

   Generates workflow files


   Build Summary

     Gen    │ Succeeded │ <0.01s
   ```

   The generated `.github/workflows/Hello.yml` runs each target as its own job, in dependency order:

   ```yaml
   name: Hello

   on:
     push:
       branches: [ main ]

   permissions: { }

   jobs:

     HelloWorld:
       runs-on: ubuntu-latest
       steps:

         - name: Checkout
           uses: actions/checkout@v6
           with:
             fetch-depth: 0

         - name: HelloWorld
           id: HelloWorld
           run: dotnet run --project _atom/_atom.csproj -- HelloWorld --skip --headless
           env:
             my-secret: ${{ secrets.MY_SECRET }}
             my-name: ${{ github.repository_owner }}

     Goodbye:
       needs: [ HelloWorld ]
       runs-on: ubuntu-latest
       steps:

         - name: Checkout
           uses: actions/checkout@v6
           with:
             fetch-depth: 0

         - name: Goodbye
           id: Goodbye
           run: dotnet run --project _atom/_atom.csproj -- Goodbye --skip --headless
   ```

## `WorkflowDefinition` Properties

| Property   | Type                                      | Description                                    |
|------------|-------------------------------------------|------------------------------------------------|
| `Name`     | `string`                                  | The workflow name (used as the file name).     |
| `Triggers` | `IReadOnlyList<IWorkflowTrigger>`         | Events that start the workflow.                |
| `Targets`  | `IReadOnlyList<WorkflowTargetDefinition>` | The targets to include in the workflow.        |
| `Options`  | `IReadOnlyList<IBuildOption>`             | Workflow-level options applied to all targets. |
| `Types`    | `IReadOnlyList<IWorkflowType>`            | Which platforms to generate for.               |

## `WorkflowTargetDefinition`

Each entry in `Targets` is a `WorkflowTargetDefinition`:

```csharp
new(nameof(Test))
{
    MatrixDimensions =
    [
        new(nameof(IJobRunsOn.JobRunsOn))
        {
            Values = ["ubuntu-latest", "windows-latest"],
        },
    ],
    Options = [BuildOptions.Github.RunsOn.SetByMatrix],
}
```

| Property           | Description                                                         |
|--------------------|---------------------------------------------------------------------|
| `Name`             | The target name (must match a target in the build definition).      |
| `MatrixDimensions` | Run the target across multiple configurations (OS, framework, etc). |
| `Options`          | Per-target options (suppress artifacts, inject params, etc).        |

### Matrix Dimensions

A `MatrixDimension` defines a named axis with multiple values. The CI platform runs the target once per combination:

```csharp
new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
{
    Values = ["ubuntu-latest", "windows-latest", "macos-latest"],
}
```

Multiple dimensions create a full cross product.

## Workflow Types

The `Types` list determines which platforms receive generated files:

| Type                            | Platform                              |
|---------------------------------|---------------------------------------|
| `WorkflowTypes.Github.Action`   | GitHub Actions (`.github/workflows/`) |
| `WorkflowTypes.Devops.Pipeline` | Azure DevOps Pipelines                |

You can target multiple platforms from the same workflow definition.

## Workflow-Level Options

Options applied at the workflow level affect all targets in the workflow:

```csharp
new("Build")
{
    Options =
    [
        BuildOptions.Inject.Param(nameof(NugetDryRun), true),
        BuildOptions.Devops.VariableGroup.Atom,
    ],
    // ...
}
```

## Real-World Example

From the Atom project's own build:

```csharp
new("Validate")
{
    Triggers = [WorkflowTriggers.Manual, WorkflowTriggers.PullIntoMain],
    Targets =
    [
        new(nameof(ISetupBuildInfo.SetupBuildInfo)),
        new(nameof(IBuildTargets.PackProjects))
        {
            Options = [BuildOptions.Target.SuppressArtifactPublishing],
        },
        new(nameof(ITestTargets.TestProjects))
        {
            MatrixDimensions =
            [
                new(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = ["windows-latest", "ubuntu-latest", "macos-latest"],
                },
                new(nameof(ITestTargets.TestFramework))
                {
                    Values = ["net8.0", "net9.0", "net10.0"],
                },
            ],
            Options =
            [
                BuildOptions.Target.SuppressArtifactPublishing,
                BuildOptions.Github.RunsOn.SetByMatrix,
            ],
        },
    ],
    Types = [WorkflowTypes.Github.Action],
}
```

## Next steps

- [Triggers](triggers.md): when a workflow starts.
- [Workflow Options](workflow-options.md): per-workflow and per-target options.
- [Gen](../built-in-targets/generate-workflow-files.md): the target that writes the files.
- <xref:Invex.Atom.Workflows.Definition.WorkflowDefinition>
- <xref:Invex.Atom.Workflows.Definition.WorkflowTargetDefinition>
- <xref:Invex.Atom.Workflows.Definition.MatrixDimension>
