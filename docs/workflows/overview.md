# Workflows Overview

Workflows are an optional layer on top of the base build system. They let you define how your targets map to CI/CD jobs
and automatically generate the platform-specific YAML files for GitHub Actions and Azure DevOps Pipelines.
Read this before the rest of the workflow pages.

## When Do You Need Workflows?

Use workflows when you want Atom to **generate** your CI/CD configuration. If you only run builds locally or maintain
your YAML by hand, stick with `IBuildDefinition`.

## Enabling Workflows

1. Extend `IWorkflowBuildDefinition` instead of `IBuildDefinition`:

   ```csharp
   [BuildDefinition]
   [GenerateEntryPoint]
   internal interface IBuild : IWorkflowBuildDefinition
   {
       // ...
   }
   ```

   The alternative partial-class form derives from `WorkflowBuildDefinition`.

2. Define the `Workflows` property to declare your pipelines. Interface builds explicitly
   implement `IWorkflowBuildDefinition.Workflows`; partial classes override it.

3. Add a platform module (`Invex.Atom.Module.GitHubWorkflows` or `Invex.Atom.Module.DevOpsWorkflows`) so Atom knows
   which YAML format to emit.

4. Run `dotnet run -- Gen` to write the files.

## What workflow builds add

Both `IWorkflowBuildDefinition` and `WorkflowBuildDefinition` provide:

| Feature                 | Description                                                                    |
|-------------------------|--------------------------------------------------------------------------------|
| `Workflows` property    | An `IReadOnlyList<WorkflowDefinition>` describing each CI/CD pipeline.         |
| `IGen`                  | The `Gen` target that writes YAML.                                             |
| `WorkflowGenerator`     | Resolves the build model into a platform-neutral workflow model.               |
| `WorkflowResolver`      | Analyses target dependencies, artifacts, and variables to build the job graph. |
| `WorkflowLifecycleHook` | A lifecycle hook that regenerates files locally and fails the build in headless mode when generated files are outdated. |

## How Generation Works

1. The `WorkflowResolver` examines your `Workflows` definitions and your target dependency graph.
2. It groups targets into **jobs**, splitting on artifact/variable boundaries (since those require separate CI jobs).
3. The `WorkflowGenerator` maps each job to a `WorkflowModel`.
4. A platform-specific `WorkflowFileWriter` (e.g. for GitHub Actions or Azure DevOps) renders the model to YAML and
   writes it to disk.

## Next steps

- [Workflow Definitions](workflow-definitions.md): declare workflows, targets, and platforms.
- [Base vs Workflow Build](../getting-started/base-vs-workflow-build.md): choose the right base contract.
- <xref:Invex.Atom.Workflows.IWorkflowBuildDefinition>
- <xref:Invex.Atom.Workflows.WorkflowBuildDefinition>
