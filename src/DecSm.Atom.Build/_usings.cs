global using System.Collections.Concurrent;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Globalization;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using DecSm.Atom.Build.Args;
global using DecSm.Atom.Build.BuildOptions;
global using DecSm.Atom.Build.BuildInfo;
global using DecSm.Atom.Build.Definition;
global using DecSm.Atom.Build.Exceptions;
global using DecSm.Atom.Build.FileSystem;
global using DecSm.Atom.Build.Hosting;
global using DecSm.Atom.Build.Logging;
global using DecSm.Atom.Build.Model;
global using DecSm.Atom.Build.Params;
global using DecSm.Atom.Build.Reports;
global using DecSm.Atom.Build.Secrets;
global using DecSm.Atom.Build.Util;
global using DecSm.Atom.Build.Util.Scope;
global using DecSm.Atom.Build.Variables;
global using DecSm.Atom.FileSystem;
global using DecSm.Atom.Process;
global using DecSm.Atom.SemanticVersion;
global using JetBrains.Annotations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Spectre.Console;
global using Spectre.Console.Rendering;
global using IHelpService = DecSm.Atom.Build.Help.IHelpService;
global using HelpService = DecSm.Atom.Build.Help.HelpService;

[assembly: InternalsVisibleTo("DecSm.Atom.Build.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: SuppressMessage("ReSharper", "LocalizableElement")]
