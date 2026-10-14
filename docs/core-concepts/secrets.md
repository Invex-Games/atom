# Secrets

Secrets are parameters that contain sensitive values (API keys, connection strings, passwords). Atom provides
first-class support for masking them in logs and resolving them from secure stores. Use a secret instead of a plain
parameter whenever the value must never appear in build output.

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

## Example

This build extends the parameter example from [Your First Build](../getting-started/your-first-build.md) with a
secret:

```csharp
using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;
using Invex.Atom.Build.Params;

namespace Atom;

[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition
{
    [ParamDefinition("my-name", "My name")]
    string? MyName => GetParam(() => MyName);

    [SecretDefinition("my-secret", "My secret")]
    string? MySecret => GetParam(() => MySecret);

    Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .RequiresParam(nameof(MyName))
            .RequiresParam(nameof(MySecret))
            .Executes(() =>
                Logger.LogInformation("Hello, World! I am {MyName} and my secret is {MySecret}.",
                    MyName,
                    MySecret));
}
```

Run it with `atom HelloWorld --my-name Frodo --my-secret TheOneRing`. The secret is masked in the output:

```text
26-06-06 +10:00  Invex.Atom.Build.BuildExecutor:
01:27:08.482 INF Executing build

HelloWorld

Prints a hello world message to the console

26-06-06 +10:00  HelloWorld | Atom.Build:
01:27:08.544 INF Hello, World! I am Frodo and my secret is *****.


Build Summary

  HelloWorld │ Succeeded │ <0.01s
```

> [!WARNING]
> Passing a real secret on the command line can expose it through shell history and process diagnostics. Use a
> secrets provider, .NET user secrets, or CI secret injection for real credentials.

## `ISecretsProvider`

Atom uses a chain-of-responsibility pattern for secret resolution. Implement `ISecretsProvider` to integrate with any
secret store:

```csharp
public interface ISecretsProvider
{
    ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken);

    int Priority => 0;
}
```

Register your provider in the DI container. Multiple providers are queried in descending `Priority` order (ties
preserve registration order) — return `null` to delegate to the next provider.

### Prefetching and synchronous access

Parameter properties such as `GetParam(() => MySecret)` are synchronous, but secret stores are usually remote. Before
any target runs, Atom collects the secret parameters used by every target that will run and resolves them
asynchronously with the build's cancellation token (cancelled when the host stops). Secrets that are already available
from the command line, environment variables or configuration are not looked up in providers. Later reads use the
prefetched values, so they do not block on the network.

A secret that was not prefetched (for example, one read by name inside another provider, or one used by a target that
is not part of the run) is still resolved on demand: Atom blocks on `GetSecretAsync` with `CancellationToken.None`.
Atom is a console application without a `SynchronizationContext`, so this cannot deadlock, but providers should apply
their own timeouts to network calls. Code that reads secrets dynamically can call
`IParamService.PrefetchSecretsAsync` first to avoid blocking.

### Built-in Providers

Implement one of these interfaces on your build definition to register its provider:

| Interface            | Package                           | Description                                                     |
|----------------------|-----------------------------------|-----------------------------------------------------------------|
| `IDotnetUserSecrets` | `Invex.Atom.Build`                | Reads from .NET User Secrets (local development).               |
| `IAzureKeyVault`     | `Invex.Atom.Module.AzureKeyVault` | Reads from Azure Key Vault (client secret, OIDC or Entra login). |

### Registering a Custom Provider

```csharp
// In your partial build class
static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder) =>
    builder.Services.AddSingleton<ISecretsProvider, MySecretsProvider>();
```

A module interface can register a provider the same way from a `[ConfigureHostBuilder]` hook; see
[Hosting](hosting.md).

## .NET User Secrets

The built-in `DotnetUserSecretsProvider` integrates with `dotnet user-secrets`. This is the recommended approach for
local development — secrets stay out of source control and are resolved automatically. Implement `IDotnetUserSecrets` on
your build definition to register it, and give the build project a `UserSecretsId`:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IBuildDefinition, IDotnetUserSecrets
{
    [SecretDefinition("my-secret", "My secret")]
    string? MySecret => GetParam(() => MySecret);
}
```

## Next steps

- [Artifacts](artifacts.md): pass files between targets.
- [Azure Key Vault](../modules/azure-key-vault.md): resolve secrets from a vault.
- [Custom Providers](../developer-guide/custom-providers.md): implement your own `ISecretsProvider`.
- <xref:Invex.Atom.Build.Params.SecretDefinitionAttribute>
- <xref:Invex.Atom.Build.Secrets.ISecretsProvider>
- <xref:Invex.Atom.Build.Secrets.IDotnetUserSecrets>
