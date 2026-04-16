namespace DecSm.Atom.Hosting;

/// <summary>
///     Defines methods for customizing the host configuration during application startup.
/// </summary>
/// <remarks>
///     This interface is automatically implemented on the build definition via source generation if any inherited
///     interfaces use <see cref="ConfigureHostAttribute" /> or <see cref="ConfigureHostBuilderAttribute" />.
///     It provides hooks for modifying the host builder and the host itself. This interface is intended for internal
///     framework use.
/// </remarks>
[PublicAPI]
public interface IConfigureHost
{
    /// <summary>
    ///     Configures the <see cref="IHostApplicationBuilder" /> before the host is built.
    /// </summary>
    /// <param name="builder">The host application builder to configure.</param>
    void ConfigureBuildHostBuilder(IHostApplicationBuilder builder);

    /// <summary>
    ///     Configures the <see cref="IHost" /> after it has been built.
    /// </summary>
    /// <param name="host">The built host to configure.</param>
    void ConfigureBuildHost(IHost host);
}
