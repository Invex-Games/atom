namespace Invex.Atom.Build.Hosting;

/// <summary>
///     Holds the build definition's static <see cref="IConfigureHost.ConfigureHost" /> hook, captured by
///     <see cref="HostExtensions.AddAtom{TBuilder, TBuild}" /> so <see cref="HostExtensions.UseAtom" /> can call it
///     without knowing the build definition type or constructing it.
/// </summary>
/// <param name="configureHost">The build definition's host configuration hook.</param>
internal sealed class AtomHostConfigurator(Action<IHost> configureHost)
{
    /// <summary>
    ///     Applies the build definition's host configuration to <paramref name="host" />.
    /// </summary>
    /// <param name="host">The built host.</param>
    public void ConfigureHost(IHost host) =>
        configureHost(host);
}
