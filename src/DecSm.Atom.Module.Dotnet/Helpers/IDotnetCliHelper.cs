namespace DecSm.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides access to the .NET CLI for executing various `dotnet` commands.
/// </summary>
/// <remarks>
///     Implementing this interface in your build definition makes the <see cref="IDotnetCli" />
///     service available, allowing you to programmatically interact with the .NET command-line tools.
/// </remarks>
[PublicAPI]
[ConfigureHostBuilder]
public partial interface IDotnetCliHelper : IBuildAccessor
{
    /// <summary>
    ///     Gets an instance of <see cref="IDotnetCli" /> for executing .NET CLI commands.
    /// </summary>
    IDotnetCli DotnetCli => GetService<IDotnetCli>();

    /// <summary>
    ///     Configures the host builder to add the .NET CLI service.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <remarks>
    ///     This method registers <see cref="DotnetCli" /> as the singleton implementation for <see cref="IDotnetCli" />.
    /// </remarks>
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IDotnetCli, DotnetCli>();
}
