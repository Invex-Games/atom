namespace DecSm.Atom.Process;

[PublicAPI]
public static class ProcessHostExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddProcessRunner() =>
            serviceCollection.AddSingleton<IProcessRunner, ProcessRunner>();
    }
}
