# Testing

**Package:** `Invex.Atom.TestUtils`

Atom provides test utilities for verifying your build definitions, targets, and modules in unit/integration tests.

## Installation

```shell
dotnet add package Invex.Atom.TestUtils
```

## Usage

`Invex.Atom.TestUtils` provides helpers for:

- Creating test build definitions
- Mocking services (file system, process runner, etc.)
- Verifying target execution order
- Testing parameter resolution
- Validating generated workflow models

## Testing a Build Model

`CreateTestHost<T>` creates an Atom host with test providers and an in-memory file system. This
example verifies that a target was discovered:

```csharp
using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;
using Invex.Atom.Build.Model;
using Invex.Atom.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

[BuildDefinition]
internal sealed partial class TestBuild : BuildDefinition
{
    public Target SayHello => t => t
        .Executes(() => Logger.LogInformation("Hello, World!"));
}

public sealed class BuildTests
{
    [Test]
    public void Build_discovers_target()
    {
        using var host = TestUtils.CreateTestHost<TestBuild>();

        var model = host.Services.GetRequiredService<BuildModel>();

        model.Targets.ShouldContainKey(nameof(TestBuild.SayHello));
    }
}
```

For target execution tests, retrieve the registered `BuildExecutor`, provide the desired
`CommandLineArgs`, and assert on the resulting target state or captured `TestConsole` output.

## Testing Workflow Generation

Verify that your workflow definitions produce the expected YAML by:

1. Instantiating the build definition in a test context.
2. Running the workflow resolver.
3. Comparing the output model against expected values.

## Tips

- Use `System.IO.Abstractions.TestingHelpers` (already a dependency) for in-memory file system testing.
- Mock `IProcessRunner` to avoid executing real processes in tests.
- Use snapshot testing to verify generated YAML doesn't change unexpectedly.
