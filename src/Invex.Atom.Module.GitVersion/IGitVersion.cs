namespace Invex.Atom.Module.GitVersion;

/// <summary>
///     Enables GitVersion-based build identification and versioning for the build.
/// </summary>
/// <remarks>
///     <para>
///         Inheriting this interface on a build definition replaces the default
///         <see cref="IBuildIdProvider" /> and <see cref="IBuildVersionProvider" /> registrations with
///         implementations that derive the build ID and version from GitVersion
///         (<see href="https://gitversion.net" />), based on the repository's Git history and the
///         <c>GitVersion.yml</c> configuration.
///     </para>
///     <para>
///         By default GitVersion finds its configuration file itself. Set <see cref="GitVersionConfig" /> (the
///         <c>gitversion-config</c> parameter) to use another file, for example one per product in a monorepo. It can
///         be set per workflow target with
///         <c>BuildOptions.Inject.Param(nameof(IGitVersion.GitVersionConfig), "src/AppA/GitVersion.yml")</c>.
///     </para>
///     <para>
///         The build ID and build version share a single GitVersion run per configuration file, and its output is
///         cached in the build project's <c>obj</c> directory until the commit or the configuration file's contents
///         change. For the default configuration, that is the first of <c>GitVersion.yml</c>, <c>GitVersion.yaml</c>,
///         <c>.GitVersion.yml</c> and <c>.GitVersion.yaml</c> in the repository root.
///         <c>GitVersion.Tool</c> is installed globally as the latest 6.x release when no 6.x release of at least
///         6.8.0 is installed. A version pinned in a local tool manifest is used instead, and must be a 6.x release of
///         at least 6.8.0.
///     </para>
///     <para>
///         This interface extends <see cref="IDotnetToolInstallHelper" />, which the GitVersion runner uses to install
///         and check <c>GitVersion.Tool</c>, so a build definition only needs to implement <see cref="IGitVersion" />.
///     </para>
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IGitVersion : IDotnetToolInstallHelper
{
    /// <summary>
    ///     Gets the path of the GitVersion configuration file to use, relative to the repository root
    ///     (for example <c>src/AppA/GitVersion.yml</c>).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <c>null</c> or empty, GitVersion finds its configuration file itself (usually
    ///         <c>GitVersion.yml</c> in the repository root). Otherwise the path is resolved against the Atom root
    ///         directory and passed to GitVersion as <c>/config "&lt;absolute path&gt;"</c>. An
    ///         <see cref="InvalidOperationException" /> naming the parameter and the resolved path is thrown when the
    ///         build ID or version is requested and the file does not exist.
    ///     </para>
    ///     <para>
    ///         The value is read with the normal parameter resolution (the <c>--gitversion-config</c> argument, an
    ///         environment variable or configuration) each time a GitVersion provider is asked for a value. When
    ///         injecting it into a workflow target, use the property name
    ///         (<c>nameof(IGitVersion.GitVersionConfig)</c>), not the argument name.
    ///     </para>
    /// </remarks>
    [ParamDefinition("gitversion-config",
        "Path to the GitVersion config file, relative to the repository root (default: GitVersion's own lookup)")]
    string? GitVersionConfig => GetParam(() => GitVersionConfig);

    /// <summary>
    ///     Configures the host builder to register the GitVersion-based build ID and version providers.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     Both providers use one shared, internal GitVersion runner, registered as a singleton.
    /// </remarks>
    protected static partial void ConfigureHostBuilderFromIGitVersion(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<GitVersionRunner>()
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
