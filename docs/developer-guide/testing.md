# Testing

Atom's own test suite uses a set of internal test helpers for verifying build definitions, targets, and modules in unit/integration tests without running a real build. This page describes those helpers and the patterns they support, so you can follow the same approach in your own tests.

**Project:** `tests/Invex.Atom.TestUtils` (internal to this repository)

## Availability

`Invex.Atom.TestUtils` is not published as a NuGet package. Projects in this repository reference it with a
`ProjectReference`:

```xml
<ProjectReference Include="..\Invex.Atom.TestUtils\Invex.Atom.TestUtils.csproj" />
```

Outside this repository, use its source as a reference for building your own test helpers; its API is not
documented or supported as a public contract and may change without notice.

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

## See also

- [Writing a Module](writing-a-module.md): the module patterns you will be testing.
- [Custom Providers](custom-providers.md): providers you may want to fake.
- <xref:Invex.Atom.Build.Model.BuildModel>
- <xref:Invex.Atom.Build.Args.CommandLineArgs>
