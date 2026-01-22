# Atom

[![Validate](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Validate.yml)
[![Build](https://github.com/DecSmith42/atom/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/Build.yml)
[![Dependabot Updates](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/dependabot/dependabot-updates)
[![CodeQL](https://github.com/DecSmith42/atom/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/DecSmith42/atom/actions/workflows/github-code-scanning/codeql)

Atom is an opinionated, type-safe build automation framework for .NET. It enables you to define your build logic in C#,
debug it like standard code, and automatically generate CI/CD configuration files for GitHub Actions and Azure DevOps.

## Why Atom?

* **Zero Context Switching**: Write build logic in C# alongside your application code.
* **Intellisense & Debugging**: Step through your build process using your IDE.
* **CI/CD Agnostic**: Define logic once; Atom generates the YAML for GitHub and Azure DevOps.
* **Modular**: Pull in capabilities via NuGet packages (GitVersion, Azure KeyVault, etc.).
* **Source Generators**: Reduces boilerplate by automatically discovering targets and parameters.

## Basic Example

1. Create a new file `Build.cs`

   ```csharp
   #:package DecSm.Atom@2.*
   
   [BuildDefinition]
   [GenerateEntryPoint]
   partial class Build : BuildDefinition
   {
       Target SayHello => t => t
           .Executes(() => Logger.LogInformation("Hello, World!"));
   }
   ```

2. Execute `dotnet run Build.cs SayHello`

   ```
   25-12-16 +10:00  DecSm.Atom.Build.BuildExecutor:
   22:46:01.754 INF Executing build
   
   SayHello
   
   25-12-16 +10:00  SayHello | Build:
   22:46:01.790 INF Hello, World!    

   Build Summary
   
     SayHello │ Succeeded │ <0.01s
   ```

## Getting Started

To get started with DecSm.Atom, follow the [Getting Started Guide](https://decsm42.gitbook.io/atom/getting-started).

## Documentation

Full documentation is available on [GitBook](https://decsm42.gitbook.io/atom/).

## License

Atom is released under the [MIT License](LICENSE.txt).