namespace Invex.Atom.Module.GitVersion;

/// <summary>
///     Enables GitVersion-based build identification and versioning for the build.
/// </summary>
/// <remarks>
///     Inheriting this interface on a build definition replaces the default
///     <see cref="IBuildIdProvider" /> and <see cref="IBuildVersionProvider" /> registrations with
///     implementations that derive the build ID and version from GitVersion
///     (<see href="https://gitversion.net" />), based on the repository's Git history and the
///     <c>GitVersion.yml</c> configuration.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGitVersion
{
    /// <summary>
    ///     Configures the host builder to register the GitVersion-based build ID and version providers.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    protected static partial void ConfigureBuilderFromIGitVersion(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
