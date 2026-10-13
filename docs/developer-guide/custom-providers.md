# Custom Providers

Atom uses a provider pattern for several extensibility points. Implement these interfaces to integrate with custom
backends. Implement one when the built-in providers and modules do not cover your secret store, artifact storage, or reporting target.

## `ISecretsProvider`

Retrieve secrets from a custom store:

```csharp
public class MySecretsProvider : ISecretsProvider
{
    public string? GetSecret(string key)
    {
        // Return the secret value, or null to delegate to the next provider.
        return MyVault.TryGet(key);
    }
}
```

Register it:

```csharp
builder.Services.AddSingleton<ISecretsProvider, MySecretsProvider>();
```

Multiple providers form a chain — return `null` to pass through.

## `IArtifactProvider`

Store and retrieve build artifacts in a custom location:

```csharp
public class MyArtifactProvider : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams => ["my-storage-url"];

    public Task StoreArtifacts(IEnumerable<string> artifactNames, string? buildId, string? buildSlice, CancellationToken ct)
    {
        // Upload artifacts
    }

    public Task RetrieveArtifacts(IEnumerable<string> artifactNames, string? buildId, string? buildSlice, CancellationToken ct)
    {
        // Download artifacts
    }

    public Task Cleanup(IEnumerable<string> runIdentifiers, CancellationToken ct)
    {
        // Remove old artifacts
    }

    public Task<IReadOnlyList<string>> GetStoredRunIdentifiers(string? artifactName, string? buildSlice, CancellationToken ct)
    {
        // List known runs
    }
}
```

Register as a singleton — only one `IArtifactProvider` is active at a time.

## `IVariableProvider`

Read and write workflow variables via a custom mechanism:

```csharp
public class MyVariableProvider : IVariableProvider
{
    public Task<bool> WriteVariable(string variableName, string variableValue, CancellationToken ct)
    {
        // Return true if handled, false to delegate
    }

    public Task<bool> ReadVariable(string jobName, string variableName, CancellationToken ct)
    {
        // Return true if found, false to delegate
    }
}
```

Multiple providers form a chain.

## `IBuildIdProvider` / `IBuildVersionProvider` / `IBuildTimestampProvider`

Replace how the build identity is determined:

```csharp
public class MyVersionProvider : IBuildVersionProvider
{
    // Version is a SemVer, not a string.
    public SemVer Version => SemVer.Parse(ReadVersionFromSomewhere());
}

public class MyBuildIdProvider : IBuildIdProvider
{
    public string BuildId => Guid.NewGuid().ToString();
}

public class MyTimestampProvider : IBuildTimestampProvider
{
    // Seconds since the Unix epoch (UTC).
    public long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
```

## `IPathProvider`

Add custom well-known paths:

```csharp
public class MyPathProvider : IPathProvider
{
    public RootedPath? GetPath(string key) => key switch
    {
        "MyCustomPath" => new RootedPath(fileSystem, "/custom/path"),
        _ => null,
    };
}
```

## `IOutcomeReportWriter`

Write build reports to a custom destination:

```csharp
public class MyReportWriter : IOutcomeReportWriter
{
    // Write report data to Slack, email, a database, etc.
}
```

## Next steps

- [Source Generators](source-generators.md): how Atom wires builds together.
- [Writing a Module](writing-a-module.md): package providers for reuse.
- <xref:Invex.Atom.Build.Secrets.ISecretsProvider>
- <xref:Invex.Atom.Build.Artifacts.IArtifactProvider>
- <xref:Invex.Atom.Build.Variables.IVariableProvider>
- <xref:Invex.Atom.Build.Reports.IOutcomeReportWriter>
