global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using DecSm.Atom.Build;
global using DecSm.Atom.Build.Args;
global using DecSm.Atom.Build.Artifacts;
global using DecSm.Atom.Build.BuildOptions;
global using DecSm.Atom.Build.Definition;
global using DecSm.Atom.Build.Model;
global using DecSm.Atom.Build.Exceptions;
global using DecSm.Atom.Build.FileSystem;
global using DecSm.Atom.Build.Params;
global using DecSm.Atom.Build.Secrets;
global using DecSm.Atom.Build.Util;
global using DecSm.Atom.FileSystem;
global using DecSm.StructuredText.Expressions;
global using DecSm.Atom.Workflows.Definition;
global using DecSm.Atom.Workflows.Definition.Triggers;
global using DecSm.Atom.Workflows.Dotnet.Nuget;
global using DecSm.Atom.Workflows.Model;
global using DecSm.Atom.Workflows.Options;
global using DecSm.Atom.Workflows.Options.Injections;
global using DecSm.Atom.Workflows.WorkflowContext;
global using DecSm.Atom.Workflows.Writer;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("DecSm.Atom.Build.Tests")]
[assembly: InternalsVisibleTo("DecSm.Atom.Workflows.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: SuppressMessage("ReSharper", "LocalizableElement")]
