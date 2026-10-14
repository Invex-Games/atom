namespace Invex.Atom.Build.Hosting;

/// <summary>
///     Marks an interface as a source of host builder configuration for any build definition that implements it.
/// </summary>
/// <remarks>
///     The interface must be <see langword="partial" />. The source generator declares
///     <c>protected static partial void ConfigureHostBuilderFrom{InterfaceName}(IHostApplicationBuilder builder)</c> on
///     the interface, which the interface must implement (analyzer AT0003 reports a missing implementation). The
///     generated <see cref="IConfigureHost.ConfigureHostBuilder" /> of every build definition that implements the
///     interface calls this method before the host is built, without constructing the build definition.
/// </remarks>
/// <example>
///     <code>
/// [ConfigureHostBuilder]
/// public partial interface IMyModule
/// {
///     protected static partial void ConfigureHostBuilderFromIMyModule(IHostApplicationBuilder builder) =>
///         builder.Services.AddSingleton&lt;IMyService, MyService&gt;();
/// }
///
/// [BuildDefinition]
/// internal partial class Build : BuildDefinition, IMyModule;
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostBuilderAttribute : Attribute;
