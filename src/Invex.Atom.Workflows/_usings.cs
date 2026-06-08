global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using Invex.Atom.Build;
global using Invex.Atom.Build.Args;
global using Invex.Atom.Build.Artifacts;
global using Invex.Atom.Build.BuildOptions;
global using Invex.Atom.Build.Definition;
global using Invex.Atom.Build.Model;
global using Invex.Atom.Build.Exceptions;
global using Invex.Atom.Build.FileSystem;
global using Invex.Atom.Build.Hosting;
global using Invex.Atom.Build.Params;
global using Invex.Atom.Build.Secrets;
global using Invex.Atom.Build.Util;
global using Invex.Atom.Workflows.Definition;
global using Invex.Atom.Workflows.Definition.Triggers;
global using Invex.Atom.Workflows.Dotnet.Nuget;
global using Invex.Atom.Workflows.Model;
global using Invex.Atom.Workflows.Options;
global using Invex.Atom.Workflows.Options.Injections;
global using Invex.Atom.Workflows.WorkflowContext;
global using Invex.Atom.Workflows.Writer;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Invex.Atom.Build.Tests")]
[assembly: InternalsVisibleTo("Invex.Atom.Workflows.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: SuppressMessage("ReSharper", "LocalizableElement")]
