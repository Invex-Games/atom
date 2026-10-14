global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using Azure;
global using Azure.Core;
global using Azure.Identity;
global using Azure.Security.KeyVault.Secrets;
global using Invex.Atom.Build;
global using Invex.Atom.Build.Args;
global using Invex.Atom.Build.BuildOptions;
global using Invex.Atom.Build.Definition;
global using Invex.Atom.Build.Hosting;
global using Invex.Atom.Build.Params;
global using Invex.Atom.Build.Secrets;
global using Invex.Atom.Module.AzureKeyVault.Options;
global using Invex.Atom.Shared.Azure;
global using Invex.Atom.Workflows.Options;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Invex.Atom.Module.AzureKeyVault.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
