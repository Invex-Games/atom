namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a source for host builder configuration, triggering source generation to apply its logic.
/// </summary>
/// <remarks>
///     When an interface is decorated with this attribute, any methods it contains named <c>Configure</c>
///     with a single <see cref="IHostApplicationBuilder" /> parameter will be automatically discovered. The source
///     generator
///     then implements <see cref="IConfigureHost.ConfigureBuildHostBuilder" /> on the build definition, calling these
///     methods to apply the specified configurations to the host builder before the host is built.
/// </remarks>
/// <example>
///     <code>
/// // 1. Define an interface with the attribute
/// [ConfigureHostBuilder]
/// public interface IMyHostBuilderConfigurator
/// {
///     void Configure(IHostApplicationBuilder builder)
///     {
///         // Configuration logic here
///     }
/// }
/// // 2. Implement the interface in your build definition
/// [BuildDefinition]
/// partial class Build : IMyHostBuilderConfigurator;
/// // The source generator will now apply the Configure method during host builder setup.
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostBuilderAttribute : Attribute;
