# Module: .NET

**Package:** `Invex.Atom.Module.Dotnet`

Provides helpers and targets for common .NET CLI operations — build, test, pack, publish, and NuGet push.

## Installation

```shell
dotnet add package Invex.Atom.Module.Dotnet
```

## Usage

The module exposes several interfaces with pre-built targets for .NET CLI commands. Implement the relevant interfaces on
your build class to gain access to the targets and their parameters.

## Features

- **CLI wrappers**: Strongly-typed wrappers around `dotnet build`, `dotnet test`, `dotnet pack`, `dotnet publish`, and
  `dotnet nuget push`.
- **Build helpers**: Utility methods for common .NET build patterns.
- **Framework labels**: `WorkflowLabels.Dotnet.Framework.*` constants for matrix dimensions (`Net_8_0`, `Net_9_0`,
  `Net_10_0`, etc.).

## Workflow Integration

Use the framework labels for matrix builds:

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

