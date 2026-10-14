namespace Invex.Atom.Build.Hosting;

/// <summary>
///     Marks an interface as a source of configuration for the built host of any build definition that implements it.
/// </summary>
/// <remarks>
///     The interface must be <see langword="partial" />. The source generator declares
///     <c>protected static partial void ConfigureHostFrom{InterfaceName}(IHost host)</c> on the interface, which the
///     interface must implement (analyzer AT0002 reports a missing implementation). The generated
///     <see cref="IConfigureHost.ConfigureHost" /> of every build definition that implements the interface calls this
///     method from <see cref="HostExtensions.UseAtom" />, after the host is built.
/// </remarks>
/// <example>
///     <code>
/// [ConfigureHost]
/// public partial interface IMyModule
/// {
///     protected static partial void ConfigureHostFromIMyModule(IHost host) =>
///         host.Services.GetRequiredService&lt;IMyService&gt;().Initialize();
/// }
///
/// [BuildDefinition]
/// internal partial class Build : BuildDefinition, IMyModule;
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostAttribute : Attribute;
