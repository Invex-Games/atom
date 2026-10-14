namespace Invex.Atom.Build.Hosting;

/// <summary>
///     Defines the static host configuration hooks of a build definition.
/// </summary>
/// <remarks>
///     <para>
///         The source generator implements this interface on every build definition, whether or not any inherited
///         interface uses <see cref="ConfigureHostAttribute" /> or <see cref="ConfigureHostBuilderAttribute" />.
///         The generated <see cref="ConfigureHostBuilder" /> registers the build's module interfaces, calls each
///         <c>ConfigureHostBuilderFrom{InterfaceName}</c> partial method and then the build's optional
///         <c>OnConfigureHostBuilder</c> partial method. The generated <see cref="ConfigureHost" /> calls each
///         <c>ConfigureHostFrom{InterfaceName}</c> partial method and then the build's optional <c>OnConfigureHost</c>
///         partial method.
///     </para>
///     <para>
///         The members are static so the host can be configured without constructing the build definition:
///         <see cref="HostExtensions.AddAtom{TBuilder, TBuild}" /> calls <see cref="ConfigureHostBuilder" /> and
///         <see cref="HostExtensions.UseAtom" /> calls <see cref="ConfigureHost" />. This interface is intended for
///         framework and generated code; build authors implement the <c>OnConfigureHostBuilder</c> and
///         <c>OnConfigureHost</c> partial methods instead.
///     </para>
/// </remarks>
[PublicAPI]
public interface IConfigureHost
{
    /// <summary>
    ///     Configures the <see cref="IHostApplicationBuilder" /> before the host is built.
    /// </summary>
    /// <param name="builder">The host application builder to configure.</param>
    static abstract void ConfigureHostBuilder(IHostApplicationBuilder builder);

    /// <summary>
    ///     Configures the <see cref="IHost" /> after it has been built.
    /// </summary>
    /// <param name="host">The built host to configure.</param>
    static abstract void ConfigureHost(IHost host);
}
