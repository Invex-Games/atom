# Secrets

Secrets are parameters that contain sensitive values (API keys, connection strings, passwords). Atom provides
first-class support for masking them in logs and resolving them from secure stores.

## Defining a Secret

Use `[SecretDefinition]` instead of `[ParamDefinition]`:

```csharp
[SecretDefinition("api-key", "The API key for the service")]
string? ApiKey => GetParam(() => ApiKey);
```

`[SecretDefinition]` inherits from `[ParamDefinition]` and sets `IsSecret = true`. When a parameter is marked as a
secret:

- Its value is **masked** in all log output.
- It can be resolved from registered `ISecretsProvider` implementations in addition to the standard sources.

## `ISecretsProvider`

Atom uses a chain-of-responsibility pattern for secret resolution. Implement `ISecretsProvider` to integrate with any
secret store:

```csharp
public interface ISecretsProvider
{
    string? GetSecret(string key);
}
```

Register your provider in the DI container. Multiple providers are queried in registration order — return `null` to
delegate to the next provider.

### Built-in Providers

| Provider                    | Package                           | Description                                       |
|-----------------------------|-----------------------------------|---------------------------------------------------|
| `DotnetUserSecretsProvider` | `DecSm.Atom.Build`                | Reads from .NET User Secrets (local development). |
| `AzureKeySecretsProvider`   | `DecSm.Atom.Module.AzureKeyVault` | Reads from Azure Key Vault.                       |

### Registering a Custom Provider

```csharp
public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
{
    base.ConfigureDefinitionHost(builder);
    builder.Services.AddSingleton<ISecretsProvider, MySecretsProvider>();
}
```

## .NET User Secrets

The built-in `DotnetUserSecretsProvider` integrates with `dotnet user-secrets`. This is the recommended approach for
local development — secrets stay out of source control and are resolved automatically.

## Next Steps

→ [Artifacts](artifacts.md)

