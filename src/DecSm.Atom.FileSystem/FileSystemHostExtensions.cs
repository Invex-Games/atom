namespace DecSm.Atom.FileSystem;

[PublicAPI]
public static class FileSystemHostExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddAtomFileSystem() =>
            serviceCollection
                .AddKeyedSingleton<IFileSystem>("RootFileSystem", new System.IO.Abstractions.FileSystem())
                .AddSingleton<IAtomFileSystem>(services =>
                    new AtomFileSystem(services.GetRequiredService<ILogger<AtomFileSystem>>())
                    {
                        FileSystem = services.GetRequiredKeyedService<IFileSystem>("RootFileSystem"),
                        PathProviders = services
                            .GetServices<IPathProvider>()
                            .OrderByDescending(l => l.Priority)
                            .ToList(),
                    })
                .AddSingleton<IFileSystem>(services => services.GetRequiredService<IAtomFileSystem>());

        /// <summary>
        ///     Registers a custom path provider with the dependency injection container.
        /// </summary>
        /// <param name="locate">A function that resolves a <see cref="RootedPath" /> based on a key.</param>
        /// <param name="priority">The priority of the provider. Higher values take precedence.</param>
        [PublicAPI]
        public void ProvidePath(Func<string, RootedPath?> locate, int priority = 1) =>
            serviceCollection.AddSingleton<IPathProvider>(new FunctionPathProvider
            {
                Priority = priority,
                Provider = locate,
            });

        /// <summary>
        ///     Registers a custom path provider with the dependency injection container.
        /// </summary>
        /// <param name="locate">A function that resolves a <see cref="RootedPath" /> based on a key.</param>
        /// <param name="priority">The priority of the provider. Higher values take precedence.</param>
        [PublicAPI]
        public void ProvidePath(Func<string, IAtomFileSystem, RootedPath?> locate, int priority = 1) =>
            serviceCollection.AddSingleton<IPathProvider>(provider => new FunctionPathProvider
            {
                Priority = priority,
                Provider = key => locate(key, provider.GetRequiredService<IAtomFileSystem>()),
            });
    }
}
