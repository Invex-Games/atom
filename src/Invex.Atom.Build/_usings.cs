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
global using Invex.Atom.Build.Args;
global using Invex.Atom.Build.BuildOptions;
global using Invex.Atom.Build.BuildInfo;
global using Invex.Atom.Build.Definition;
global using Invex.Atom.Build.Exceptions;
global using Invex.Atom.Build.FileSystem;
global using Invex.Atom.Build.Hosting;
global using Invex.Atom.Build.Logging;
global using Invex.Atom.Build.Model;
global using Invex.Atom.Build.Params;
global using Invex.Atom.Build.Reports;
global using Invex.Atom.Build.Secrets;
global using Invex.Atom.Build.Util;
global using Invex.Atom.Build.Util.Scope;
global using Invex.Atom.Build.Variables;
global using JetBrains.Annotations;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Spectre.Console;
global using Spectre.Console.Rendering;
global using IHelpService = Invex.Atom.Build.Help.IHelpService;
global using HelpService = Invex.Atom.Build.Help.HelpService;

[assembly: InternalsVisibleTo("Invex.Atom.Build.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: SuppressMessage("ReSharper", "LocalizableElement")]
