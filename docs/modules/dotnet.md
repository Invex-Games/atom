# Module: .NET

**Package:** `Invex.Atom.Module.Dotnet`

Provides helpers and targets for common .NET CLI operations — build, test, pack, publish, and NuGet push.

## Installation

```shell
dotnet add package Invex.Atom.Module.Dotnet
```

## Usage

The module exposes several interfaces with pre-built helper methods for .NET CLI commands. Implement the relevant
interfaces on your build class (or your own target interfaces) to gain access to the helpers and their parameters:

| Interface                  | Purpose                                                                    |
|----------------------------|----------------------------------------------------------------------------|
| `IDotnetCli`               | Strongly-typed wrappers for the full `dotnet` CLI surface.                 |
| `IDotnetCliHelper`         | Base helper for invoking the .NET CLI.                                     |
| `IDotnetPackHelper`        | `DotnetPackAndStage` — packs a project and stages the `.nupkg` output.     |
| `IDotnetPublishHelper`     | Publishes a project and stages the output.                                 |
| `IDotnetTestHelper`        | Runs test projects and collects results/coverage (TRX models included).    |
| `IDotnetToolInstallHelper` | Installs .NET tools.                                                       |
| `INugetHelper`             | NuGet feed management and push operations.                                 |

## Features

- **CLI wrappers**: Strongly-typed wrappers around `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`,
  `dotnet nuget push`, and the rest of the `dotnet` CLI.
- **Build helpers**: Utility methods for common .NET build patterns (pack & stage, publish & stage, test with
  reporting, tool installation).
- **Version transformation**: `TransformProjectVersionScope` temporarily rewrites project versions from the build's
  version providers during packing.

## Workflow Integration

Framework labels for matrix builds are provided by the `Invex.Atom.Workflows` package
(`WorkflowLabels.Dotnet.Framework.*`):

```csharp
new MatrixDimension(nameof(ITestTargets.TestFramework))
{
    Values = [
        WorkflowLabels.Dotnet.Framework.Net_8_0,
        WorkflowLabels.Dotnet.Framework.Net_9_0,
    ],
}
```

Setup steps ensure the required .NET SDK versions are installed on CI runners:

```csharp
BuildOptions.Steps.SetupDotnet.Dotnet80X()
BuildOptions.Steps.SetupDotnet.Dotnet90X()
```

