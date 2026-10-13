# API Reference

This reference is generated from the XML documentation of every public type in the Atom packages; the tables below list
the main entry points per package, and the sidebar lists everything else.

## Invex.Atom.Build

| Type                                                                                 | Kind      | Description                                                                                    |
|--------------------------------------------------------------------------------------|-----------|------------------------------------------------------------------------------------------------|
| [`IBuildDefinition`](xref:Invex.Atom.Build.Definition.IBuildDefinition)              | Interface | The core structure of an Atom build; extend it with an interface build definition.              |
| [`BuildDefinition`](xref:Invex.Atom.Build.Definition.BuildDefinition)                | Class     | Abstract base class for partial-class build definitions.                                        |
| [`BuildDefinitionAttribute`](xref:Invex.Atom.Build.Definition.BuildDefinitionAttribute) | Attribute | Marks a build definition and triggers source generation of its targets and parameters.       |
| [`GenerateEntryPointAttribute`](xref:Invex.Atom.Build.Hosting.GenerateEntryPointAttribute) | Attribute | Generates the application's `Main` method.                                                 |
| [`Target`](xref:Invex.Atom.Build.Definition.Target)                                  | Delegate  | Applies configuration to a `TargetDefinition`; the type of every target property.               |
| [`TargetDefinition`](xref:Invex.Atom.Build.Definition.TargetDefinition)              | Class     | Fluent definition of a target's tasks, dependencies, parameters, artifacts, and variables.     |
| [`IBuildAccessor`](xref:Invex.Atom.Build.IBuildAccessor)                             | Interface | Access to the logger, file system, process runner, services, and parameters inside targets.    |
| [`ParamDefinitionAttribute`](xref:Invex.Atom.Build.Params.ParamDefinitionAttribute)  | Attribute | Declares a parameter with its command-line name, description, and resolution sources.           |
| [`SecretDefinitionAttribute`](xref:Invex.Atom.Build.Params.SecretDefinitionAttribute) | Attribute | Declares a secret parameter that is masked in logs and can be resolved from secret stores.    |
| [`ParamSource`](xref:Invex.Atom.Build.Params.ParamSource)                            | Enum      | The sources a parameter value can be resolved from.                                             |
| [`ISecretsProvider`](xref:Invex.Atom.Build.Secrets.ISecretsProvider)                 | Interface | Retrieves secrets from a secure store.                                                          |
| [`IDotnetUserSecrets`](xref:Invex.Atom.Build.Secrets.IDotnetUserSecrets)             | Interface | Enables sourcing secrets from the .NET user secrets store.                                      |
| [`IArtifactProvider`](xref:Invex.Atom.Build.Artifacts.IArtifactProvider)             | Interface | Stores and retrieves build artifacts.                                                           |
| [`IVariablesHelper`](xref:Invex.Atom.Build.Variables.IVariablesHelper)               | Interface | Helper methods for writing workflow variables from targets.                                     |
| [`IVariableProvider`](xref:Invex.Atom.Build.Variables.IVariableProvider)             | Interface | Reads and writes workflow variables to a specific storage backend.                              |
| [`IBuildInfo`](xref:Invex.Atom.Build.BuildInfo.IBuildInfo)                           | Interface | Build name, ID, version, timestamp, and slice.                                                  |
| [`ISetupBuildInfo`](xref:Invex.Atom.Build.ISetupBuildInfo)                           | Interface | Provides the `SetupBuildInfo` target.                                                           |
| [`IValidateBuild`](xref:Invex.Atom.Build.IValidateBuild)                             | Interface | Provides the `ValidateBuild` target.                                                            |
| [`IBuildOption`](xref:Invex.Atom.Build.BuildOptions.IBuildOption)                    | Interface | Marker interface for build options.                                                             |
| [`BuildOptions`](xref:Invex.Atom.Build.BuildOptions.BuildOptions)                    | Class     | Extension anchor for fluent build option factories.                                             |
| [`AtomHost`](xref:Invex.Atom.Build.Hosting.AtomHost)                                 | Class     | Creates and runs an Atom host application.                                                      |
| [`ConfigureHostBuilderAttribute`](xref:Invex.Atom.Build.Hosting.ConfigureHostBuilderAttribute) | Attribute | Marks a module interface as a source of host builder configuration.                 |
| [`IAtomLifecycleHook`](xref:Invex.Atom.Build.IAtomLifecycleHook)                     | Interface | Runs code before and after target execution.                                                    |
| [`IReportsHelper`](xref:Invex.Atom.Build.Reports.IReportsHelper)                     | Interface | Adds data to the build report.                                                                  |
| [`IOutcomeReportWriter`](xref:Invex.Atom.Build.Reports.IOutcomeReportWriter)         | Interface | Writes the build outcome report to a destination.                                               |
| [`StepFailedException`](xref:Invex.Atom.Build.Exceptions.StepFailedException)        | Class     | Thrown when a step within a target fails.                                                       |

## Invex.Atom.Workflows

| Type                                                                                              | Kind      | Description                                                                  |
|---------------------------------------------------------------------------------------------------|-----------|------------------------------------------------------------------------------|
| [`IWorkflowBuildDefinition`](xref:Invex.Atom.Workflows.IWorkflowBuildDefinition)                  | Interface | Extends `IBuildDefinition` with workflow definitions and the `Gen` target.   |
| [`WorkflowBuildDefinition`](xref:Invex.Atom.Workflows.WorkflowBuildDefinition)                    | Class     | Abstract base class for partial-class workflow build definitions.            |
| [`WorkflowDefinition`](xref:Invex.Atom.Workflows.Definition.WorkflowDefinition)                   | Record    | A workflow's name, triggers, options, targets, and types.                    |
| [`WorkflowTargetDefinition`](xref:Invex.Atom.Workflows.Definition.WorkflowTargetDefinition)       | Record    | A target within a workflow, with matrix dimensions and options.              |
| [`MatrixDimension`](xref:Invex.Atom.Workflows.Definition.MatrixDimension)                         | Record    | A named build-matrix axis and its values.                                    |
| [`WorkflowTriggers`](xref:Invex.Atom.Workflows.Definition.Triggers.WorkflowTriggers)              | Class     | Factories and presets for push, pull request, and manual triggers.           |
| [`IGen`](xref:Invex.Atom.Workflows.IGen)                                                          | Interface | Provides the `Gen` target that writes workflow files.                        |
| [`IJobRunsOn`](xref:Invex.Atom.Workflows.IJobRunsOn)                                              | Interface | The runner or agent a workflow job executes on.                              |
| [`WorkflowBuildOptionsExtensions`](xref:Invex.Atom.Workflows.Options.WorkflowBuildOptionsExtensions) | Class  | Adds `BuildOptions.Inject`, `Artifacts`, `Deploy`, `Target`, and `Steps`.    |
| [`DebugWorkflowType`](xref:Invex.Atom.Workflows.DebugWorkflowType)                                | Record    | Writes the resolved workflow model as JSON for inspection.                   |
| [`WorkflowOutdatedException`](xref:Invex.Atom.Workflows.WorkflowOutdatedException)                | Class     | Thrown in headless mode when workflow files are out of date.                 |

## Invex.Atom.Module.Dotnet

| Type                                                                                         | Kind      | Description                                                       |
|----------------------------------------------------------------------------------------------|-----------|-------------------------------------------------------------------|
| [`IDotnetCli`](xref:Invex.Atom.Module.Dotnet.Cli.IDotnetCli)                                 | Interface | Strongly-typed methods for `dotnet` CLI commands.                 |
| [`IDotnetCliHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.IDotnetCliHelper)                 | Interface | Makes the `IDotnetCli` service available to a build.              |
| [`IDotnetPackHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.IDotnetPackHelper)               | Interface | Packs projects and stages the `.nupkg` output.                    |
| [`IDotnetPublishHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.IDotnetPublishHelper)         | Interface | Publishes projects and stages the output.                         |
| [`IDotnetTestHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.IDotnetTestHelper)               | Interface | Runs test projects and reports results and coverage.              |
| [`IDotnetToolInstallHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.IDotnetToolInstallHelper) | Interface | Installs .NET tools.                                              |
| [`INugetHelper`](xref:Invex.Atom.Module.Dotnet.Helpers.INugetHelper)                         | Interface | NuGet feed management and push operations.                        |
| [`TransformProjectVersionScope`](xref:Invex.Atom.Module.Dotnet.Util.TransformProjectVersionScope) | Class | Creates scopes that temporarily set a project file's version.  |

## Invex.Atom.Module.GithubWorkflows

| Type                                                                                                   | Kind      | Description                                                   |
|--------------------------------------------------------------------------------------------------------|-----------|---------------------------------------------------------------|
| [`IGithubWorkflows`](xref:Invex.Atom.Module.GithubWorkflows.Helpers.IGithubWorkflows)                  | Interface | Integrates GitHub Actions workflows into a build.             |
| [`IGithubHelper`](xref:Invex.Atom.Module.GithubWorkflows.Helpers.IGithubHelper)                        | Interface | Common GitHub parameters such as the GitHub token.            |
| [`Github`](xref:Invex.Atom.Module.GithubWorkflows.Github)                                              | Class     | Detects GitHub Actions and exposes its environment variables. |
| [`GithubTrigger`](xref:Invex.Atom.Module.GithubWorkflows.Options.GithubTrigger)                        | Record    | A GitHub-specific workflow trigger, such as `release`.        |
| [`GithubTokenPermissionsOption`](xref:Invex.Atom.Module.GithubWorkflows.Options.GithubTokenPermissionsOption) | Record | Sets fine-grained `GITHUB_TOKEN` permissions.          |
| [`GithubConcurrencyOption`](xref:Invex.Atom.Module.GithubWorkflows.Options.GithubConcurrencyOption)    | Record    | Configures concurrency for a GitHub Actions workflow.         |

## Invex.Atom.Module.DevopsWorkflows

| Type                                                                                                         | Kind      | Description                                                       |
|--------------------------------------------------------------------------------------------------------------|-----------|-------------------------------------------------------------------|
| [`IDevopsWorkflows`](xref:Invex.Atom.Module.DevopsWorkflows.IDevopsWorkflows)                                | Interface | Integrates Azure DevOps Pipelines into a build.                   |
| [`Devops`](xref:Invex.Atom.Module.DevopsWorkflows.Devops)                                                    | Class     | Detects Azure Pipelines and exposes its environment variables.    |
| [`ProvideDevopsRunIdAsWorkflowId`](xref:Invex.Atom.Module.DevopsWorkflows.ProvideDevopsRunIdAsWorkflowId)    | Record    | Uses the Azure DevOps run ID as the workflow ID.                  |
| [`DevopsConcurrencyOption`](xref:Invex.Atom.Module.DevopsWorkflows.Workflows.Options.DevopsConcurrencyOption) | Record   | Configures how runs waiting on an exclusive lock are handled.     |
| [`DevopsPullRequestOption`](xref:Invex.Atom.Module.DevopsWorkflows.Workflows.Options.DevopsPullRequestOption) | Record   | Configures pull request auto-cancellation.                        |

## Invex.Atom.Module.AzureKeyVault

| Type                                                                                                | Kind      | Description                                                 |
|-----------------------------------------------------------------------------------------------------|-----------|-------------------------------------------------------------|
| [`IAzureKeyVault`](xref:Invex.Atom.Module.AzureKeyVault.IAzureKeyVault)                             | Interface | Integrates Azure Key Vault for secrets.                     |
| [`AzureKeySecretsProvider`](xref:Invex.Atom.Module.AzureKeyVault.AzureKeySecretsProvider)           | Class     | `ISecretsProvider` that reads secrets from Azure Key Vault. |
| [`AzureKeyVaultValueInjections`](xref:Invex.Atom.Module.AzureKeyVault.AzureKeyVaultValueInjections) | Record    | How each vault parameter is injected into workflows.        |
| [`UseAzureKeyVault`](xref:Invex.Atom.Module.AzureKeyVault.Options.UseAzureKeyVault)                 | Record    | Build option that enables Azure Key Vault.                  |

## Invex.Atom.Module.AzureStorage

| Type                                                                                          | Kind      | Description                                                       |
|-----------------------------------------------------------------------------------------------|-----------|-------------------------------------------------------------------|
| [`IAzureArtifactStorage`](xref:Invex.Atom.Module.AzureStorage.IAzureArtifactStorage)          | Interface | Configures Azure Blob Storage as the artifact store.              |
| [`AzureBlobArtifactProvider`](xref:Invex.Atom.Module.AzureStorage.AzureBlobArtifactProvider)  | Class     | `IArtifactProvider` that stores artifacts in Azure Blob Storage.  |

## Invex.Atom.Module.GitVersion

| Type                                                              | Kind      | Description                                                 |
|-------------------------------------------------------------------|-----------|-------------------------------------------------------------|
| [`IGitVersion`](xref:Invex.Atom.Module.GitVersion.IGitVersion)    | Interface | Enables GitVersion-based build identification and versioning. |

## Typical flow

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IWorkflowBuildDefinition, IGithubWorkflows
{
    [ParamDefinition("my-name", "My name")]
    string? MyName => GetParam(() => MyName);

    Target HelloWorld => t => t
        .RequiresParam(nameof(MyName))
        .Executes(() => Logger.LogInformation("Hello, {MyName}!", MyName));

    IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
    [
        new("CI")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(HelloWorld))],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}
```

For concepts and walkthroughs, start with the [Introduction](../docs/getting-started/introduction.md).
