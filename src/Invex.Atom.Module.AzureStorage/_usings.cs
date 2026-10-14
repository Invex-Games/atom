global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using Azure.Core;
global using Azure.Identity;
global using Azure.Storage.Blobs;
global using Azure.Storage.Blobs.Models;
global using Invex.Atom.Build.Args;
global using Invex.Atom.Build.Artifacts;
global using Invex.Atom.Build.BuildInfo;
global using Invex.Atom.Build.FileSystem;
global using Invex.Atom.Build.Hosting;
global using Invex.Atom.Build.Params;
global using Invex.Atom.Build.Reports;
global using Invex.Atom.Build.Util;
global using Invex.Atom.Shared.Azure;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Invex.Atom.Module.AzureStorage.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
