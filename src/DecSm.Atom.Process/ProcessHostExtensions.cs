namespace DecSm.Atom.Process;

/// <summary>
///     Provides extension methods for registering the process runner service with an
///     <see cref="IServiceCollection" />.
/// </summary>
[PublicAPI]
public static class ProcessHostExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Registers <see cref="IProcessRunner" /> as a singleton backed by the default
        ///     <see cref="ProcessRunner" /> implementation.
        /// </summary>
        /// <returns>The same <see cref="IServiceCollection" /> for further configuration.</returns>
        /// <remarks>
        ///     Call this once during host setup so that build targets and helpers can inject
        ///     <see cref="IProcessRunner" /> rather than constructing
        ///     <see cref="System.Diagnostics.Process" /> instances directly.
        /// </remarks>
        public IServiceCollection AddProcessRunner() =>
            serviceCollection.AddSingleton<IProcessRunner, ProcessRunner>();
    }
}
