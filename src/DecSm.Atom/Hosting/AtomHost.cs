namespace DecSm.Atom.Hosting;

/// <summary>
///     Provides static methods for creating and running an Atom host application.
/// </summary>
/// <remarks>
///     This class simplifies the setup and execution of an Atom host by configuring the necessary services
///     and application settings based on a specified build definition.
/// </remarks>
[PublicAPI]
public static class AtomHost
{
    /// <summary>
    ///     Creates and configures a <see cref="HostApplicationBuilder" /> for an Atom application.
    /// </summary>
    /// <typeparam name="T">The <see cref="MinimalBuildDefinition" /> type to configure the host with.</typeparam>
    /// <param name="args">The command-line arguments for the application.</param>
    /// <returns>A configured <see cref="HostApplicationBuilder" /> ready for further customization or building.</returns>
    public static HostApplicationBuilder CreateAtomBuilder<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string[] args)
        where T : MinimalBuildDefinition
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = args,
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                              Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            ApplicationName = "Atom",
        });

        var environment = builder.Environment.EnvironmentName;

        builder.Configuration.AddJsonFile("appsettings.json", true, true);

        if (environment.Length > 0)
            builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

        builder.AddAtom<HostApplicationBuilder, T>(args);

        return builder;
    }

    /// <summary>
    ///     Builds and runs an Atom application using the specified build definition.
    /// </summary>
    /// <typeparam name="T">The <see cref="MinimalBuildDefinition" /> type to configure and run the application with.</typeparam>
    /// <param name="args">The command-line arguments for the application.</param>
    /// <remarks>
    ///     This method handles the complete setup, build, and execution lifecycle of the host.
    /// </remarks>
    public static void Run<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string[] args)
        where T : MinimalBuildDefinition =>
        CreateAtomBuilder<T>(args)
            .Build()
            .UseAtom()
            .Run();
}
