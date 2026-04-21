namespace DecSm.Atom.Module.GitVersion;

[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGitVersion
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
