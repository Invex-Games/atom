namespace DecSm.Atom.Module.GitVersion;

/// <summary>
///     Provides integration with GitVersion to automatically determine build ID and version information.
/// </summary>
/// <remarks>
///     Implementing this interface in your build definition will configure DecSm.Atom to use
///     GitVersion for generating build IDs and version numbers, ensuring consistent and
///     semantically versioned builds based on your Git history.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGitVersion
{
    /// <summary>
    ///     Configures the host builder to use GitVersion for providing build ID and version information.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="GitVersionBuildIdProvider" /> as the singleton
    ///     implementation for <see cref="IBuildIdProvider" /> and <see cref="GitVersionBuildVersionProvider" />
    ///     as the singleton implementation for <see cref="IBuildVersionProvider" />.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
