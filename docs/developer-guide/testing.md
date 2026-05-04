# Testing

**Package:** `DecSm.Atom.TestUtils`

Atom provides test utilities for verifying your build definitions, targets, and modules in unit/integration tests.

## Installation

```shell
dotnet add package DecSm.Atom.TestUtils
```

## Usage

`DecSm.Atom.TestUtils` provides helpers for:

- Creating test build definitions
- Mocking services (file system, process runner, etc.)
- Verifying target execution order
- Testing parameter resolution
- Validating generated workflow models

## Testing a Target

Set up a test build with mocked services, execute a target, and assert the results:

```csharp
// Arrange - create a test host with your build definition
// Act - execute the target
// Assert - verify the expected behaviour
```

## Testing Workflow Generation

Verify that your workflow definitions produce the expected YAML by:

1. Instantiating the build definition in a test context.
2. Running the workflow resolver.
3. Comparing the output model against expected values.

## Tips

- Use `System.IO.Abstractions.TestingHelpers` (already a dependency) for in-memory file system testing.
- Mock `IProcessRunner` to avoid executing real processes in tests.
- Use snapshot testing to verify generated YAML doesn't change unexpectedly.

