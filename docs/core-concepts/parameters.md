# Parameters

Parameters allow you to pass configuration values into your build from multiple sources.

## Defining a Parameter

Use `[ParamDefinition]` on a property and `GetParam()` to resolve its value:

```csharp
[ParamDefinition("my-param", "Description shown in help")]
string? MyParam => GetParam(() => MyParam);
```

The first argument is the **command-line name** (kebab-case by convention). The property name is used internally for
`RequiresParam`/`UsesParam`.

### With a Default Value

```csharp
[ParamDefinition("configuration", "Build configuration")]
string Configuration => GetParam(() => Configuration, "Release");
```

### With a Custom Converter

```csharp
[ParamDefinition("retry-count", "Number of retries")]
int RetryCount => GetParam(() => RetryCount, 3, s => int.Parse(s!));
```

## Resolution Order

When `GetParam` is called, Atom resolves the value from the following sources (first match wins):

1. **Cache** — a previously resolved value for this build run
2. **Command-line arguments** — `--my-param value`
3. **Environment variables** — `MY_PARAM=value`
4. **Configuration** — `appsettings.json` under the `Params` section
5. **Secrets** — registered `ISecretsProvider` implementations

You can restrict which sources are checked using the `sources` parameter:

```csharp
[ParamDefinition("api-url", "API endpoint", sources: ParamSource.Configuration | ParamSource.EnvironmentVariables)]
string? ApiUrl => GetParam(() => ApiUrl);
```

### `ParamSource` Flags

| Flag                   | Source                                        |
|------------------------|-----------------------------------------------|
| `Cache`                | Previously resolved value                     |
| `CommandLineArgs`      | `--param-name value`                          |
| `EnvironmentVariables` | Environment variable matching the param name  |
| `Configuration`        | `appsettings.json` / `appsettings.{env}.json` |
| `Secrets`              | Registered `ISecretsProvider` implementations |
| `All`                  | All of the above (default)                    |

## Requiring Parameters

Mark a parameter as required for a target so the build fails early if it's missing:

```csharp
Target Deploy => t => t
    .RequiresParam(nameof(ApiKey))
    .Executes(() => { /* ... */ });
```

## Interactive Prompting

When running with `--interactive` (or `-i`), Atom will prompt the user for any required parameter that hasn't been
provided:

```shell
dotnet run -- Deploy --interactive
```

## Chained Parameters

A parameter can depend on other parameters:

```csharp
[ParamDefinition("full-url", "Complete URL", chainedParams: ["base-url", "path"])]
string? FullUrl => GetParam(() => FullUrl);
```

## Configuration File

Parameters can be set in `appsettings.json`:

```json
{
  "Params": {
    "configuration": "Release",
    "my-param": "some-value"
  }
}
```

Environment-specific overrides work via `appsettings.{DOTNET_ENVIRONMENT}.json`.

## Next Steps

→ [Secrets](secrets.md)

