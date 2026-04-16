namespace DecSm.Atom.Hosting;

/// <summary>
///     Marks an interface as a source for host configuration, triggering source generation to apply its logic.
/// </summary>
/// <remarks>
///     When an interface is decorated with this attribute, any methods it contains named <c>Configure</c>
///     with a single <see cref="IHost" /> parameter will be automatically discovered. The source generator
///     then implements <see cref="IConfigureHost.ConfigureBuildHost" /> on the build definition, calling these
///     methods to apply the specified configurations to the host after it is built.
/// </remarks>
/// <example>
///     <code>
/// // 1. Define an interface with the attribute
/// [ConfigureHost]
/// public interface IMyHostConfigurator
/// {
///     void Configure(IHost host)
///     {
///         // Configuration logic here
///     }
/// }
/// // 2. Implement the interface in your build definition
/// [BuildDefinition]
/// partial class Build : IMyHostConfigurator;
/// // The source generator will now apply the Configure method during host setup.
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class ConfigureHostAttribute : Attribute;
