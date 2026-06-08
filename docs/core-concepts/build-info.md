# Build Info

Atom provides a pluggable system for determining the build's identity: its **name**, **ID**, **version**, and *
*timestamp**.

## `IBuildInfo`

The `IBuildInfo` interface exposes:

| Property         | Type             | Description                                    |
|------------------|------------------|------------------------------------------------|
| `BuildName`      | `string`         | Logical name of the build (e.g. `"MyProject"`) |
| `BuildId`        | `string`         | Unique identifier for this build run           |
| `BuildVersion`   | `string`         | Semantic version string                        |
| `BuildTimestamp` | `DateTimeOffset` | When the build started                         |

## Providers

Each piece of build info is resolved by a dedicated provider interface:

| Interface                 | Default                         | Description                     |
|---------------------------|---------------------------------|---------------------------------|
| `IBuildIdProvider`        | `DefaultBuildIdProvider`        | Returns a GUID-based ID         |
| `IBuildVersionProvider`   | `DefaultBuildVersionProvider`   | Returns `"0.0.0"`               |
| `IBuildTimestampProvider` | `DefaultBuildTimestampProvider` | Returns `DateTimeOffset.UtcNow` |

### Replacing a Provider

Register your own implementation in the DI container. For example, the **GitVersion** module replaces both the ID and
version providers:

```csharp
builder.Services
    .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
    .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
```

## `SetupBuildInfo` Target

The built-in `ISetupBuildInfo` interface provides a `SetupBuildInfo` target that:

1. Reads the build name, ID, version, and timestamp from the providers.
2. Writes each as a [workflow variable](variables.md) (`BuildName`, `BuildId`, `BuildVersion`, `BuildTimestamp`).
3. Adds a "Run Information" section to the build report.
4. Logs the values.

Most builds include `SetupBuildInfo` as a dependency of their first real target.

See [SetupBuildInfo](../built-in-targets/setup-build-info.md) for details.

## Next Steps

→ [Build Options](build-options.md)

