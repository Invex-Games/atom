namespace Invex.Atom.Build.Hosting;

/// <summary>
///     Provides extension methods for configuring and integrating Atom services into a .NET host.
/// </summary>
/// <remarks>
///     This class simplifies the setup of an Atom application by registering essential services,
///     logging providers, and dependencies required for the Atom framework to function correctly.
/// </remarks>
[PublicAPI]
public static class HostExtensions
{
    /// <summary>
    ///     Configures the <see cref="IHostApplicationBuilder" /> with all necessary Atom services, dependencies,
    ///     logging, and default settings.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the host application builder.</typeparam>
    /// <typeparam name="TBuild">
    ///     The type of the build definition, which must derive from <see cref="BuildDefinition" /> and implement
    ///     <see cref="IConfigureHost" />. Classes generated for <see cref="BuildDefinitionAttribute" /> do both.
    /// </typeparam>
    /// <param name="builder">The host application builder instance to configure.</param>
    /// <param name="args">The command-line arguments provided to the application.</param>
    /// <returns>The configured host application builder instance.</returns>
    /// <remarks>
    ///     This method performs the following key configurations:
    ///     <list type="bullet">
    ///         <item>Registers the <see cref="AtomService" /> as a hosted service.</item>
    ///         <item>Registers the build definition (<typeparamref name="TBuild" />) and related interfaces.</item>
    ///         <item>
    ///             Adds core Atom services like <see cref="IParamService" />, <see cref="IReportService" />, and
    ///             <see cref="IAnsiConsole" />.
    ///         </item>
    ///         <item>
    ///             Registers default providers for build information: <see cref="IBuildIdProvider" />,
    ///             <see cref="IBuildVersionProvider" />, and <see cref="IBuildTimestampProvider" />.
    ///         </item>
    ///         <item>Configures file system access via <see cref="IRootedFileSystem" />.</item>
    ///         <item>Registers build execution and workflow generation services.</item>
    ///         <item>Sets up logging with Spectre.Console and report providers, filtering out verbose Microsoft host logs.</item>
    ///         <item>Parses command-line arguments and makes them available as <see cref="CommandLineArgs" />.</item>
    ///         <item>Resolves and registers the <see cref="BuildModel" />.</item>
    ///         <item>
    ///             Invokes the static <see cref="IConfigureHost.ConfigureHostBuilder" /> of
    ///             <typeparamref name="TBuild" />, without constructing the build definition, and records
    ///             <see cref="IConfigureHost.ConfigureHost" /> for <see cref="UseAtom" /> to call.
    ///         </item>
    ///     </list>
    /// </remarks>
    public static TBuilder AddAtom<TBuilder,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuild>(
        this TBuilder builder,
        string[] args)
        where TBuilder : IHostApplicationBuilder
        where TBuild : BuildDefinition, IConfigureHost
    {
        builder
            .Services
            .AddOptions<ServiceProviderOptions>()
            .Configure(o =>
            {
                o.ValidateScopes = true;
                o.ValidateOnBuild = true;
            });

        builder.Services.AddHostedService<AtomService>();
        builder.Services.AddSingleton<BuildDefinition, TBuild>();
        builder.Services.AddSingleton<IBuildDefinition>(x => x.GetRequiredService<BuildDefinition>());

        builder.Services.AddSingleton<IParamService, ParamService>();
        builder.Services.AddSingleton<IReportService, ReportService>();

        builder.Services.AddSingleton<IAnsiConsole>(services =>
        {
            // Wrap Spectre's console output so all rendered text is masked for secrets before being written

            var settings = new AnsiConsoleSettings
            {
                Out = new MaskingAnsiConsoleOutput(AnsiConsole.Console.Profile.Out,
                    services.GetRequiredService<IServiceProvider>()),
            };

            return AnsiConsole.Create(settings);
        });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<IBuildIdProvider, DefaultBuildIdProvider>();
        builder.Services.TryAddSingleton<IBuildVersionProvider, DefaultBuildVersionProvider>();
        builder.Services.TryAddSingleton<IBuildTimestampProvider, DefaultBuildTimestampProvider>();

        builder
            .Services
            .AddRootedFileSystem()
            .AddSingleton<IPathProvider, AtomPathProvider>();

        builder.Services.AddSingleton(services => new AtomProjectData
        {
            ProjectName = services.GetRequiredService<CommandLineArgs>()
                .ProjectName is { Length: > 0 } p
                ? p
                : Assembly.GetEntryAssembly()!.GetName()
                    .Name!,
            IsFileBasedApp = AppContext.GetData("EntryPointFilePath") is string s && s.EndsWith(".cs"),
        });

        builder.Services.AddSingleton<BuildExecutor>();
        builder.Services.AddProcessRunner();
        builder.Services.AddSingleton<IOutcomeReportWriter, ConsoleOutcomeReportWriter>();

        builder.Services.AddSingleton<IVariableProvider, AtomVariableProvider>();
        builder.Services.TryAddSingleton<IVariableService, VariableService>();

        builder.Services.TryAddSingleton<IHelpService, HelpService>();

        builder.Services.AddSingleton<CommandLineArgsParser>();

        builder.Services.AddSingleton<CommandLineArgs>(services =>
        {
            var parsedArgs = services
                .GetRequiredService<CommandLineArgsParser>()
                .Parse(args);

            if (parsedArgs.HasVerbose)
                LogOptions.IsVerboseEnabled = true;

            return parsedArgs;
        });

        builder.Services.AddSingleton<IBuildOptionService, BuildOptionService>();
        builder.Services.AddSingleton<BuildResolver>();

        builder.Services.AddSingleton<BuildModel>(services =>
        {
            try
            {
                return services
                    .GetRequiredService<BuildResolver>()
                    .Resolve();
            }
            catch (BuildConfigurationException ex)
            {
                // Log and re-throw to prevent app startup
                var logger = services.GetService<ILogger<BuildResolver>>();
                logger?.LogError("Build configuration error during initialization: {Message}", ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                var logger = services.GetService<ILogger<BuildResolver>>();
                logger?.LogError(ex, "Unexpected error during build model resolution");

                throw;
            }
        });

        builder.Logging.ClearProviders();

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Services.AddSingleton<ILoggerProvider, SpectreLoggerProvider>();
        builder.Services.AddSingleton<ILoggerProvider, ReportLoggerProvider>();

        builder.Logging.AddFilter((context, level) => (context, level) switch
        {
            ("Microsoft.Hosting.Lifetime", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting.Internal.Host", < LogLevel.Warning) => false,
            _ => true,
        });

        // Captured statically so UseAtom can run the build's host hooks without resolving the build definition
        builder.Services.AddSingleton(new AtomHostConfigurator(TBuild.ConfigureHost));

        TBuild.ConfigureHostBuilder(builder);

        return builder;
    }

    /// <summary>
    ///     Configures the built <see cref="IHost" /> instance for Atom-specific behaviors.
    /// </summary>
    /// <param name="host">The <see cref="IHost" /> instance to configure.</param>
    /// <returns>The configured <see cref="IHost" /> instance.</returns>
    /// <remarks>
    ///     This method invokes <see cref="IConfigureHost.ConfigureHost" /> of the build definition type passed to
    ///     <see cref="AddAtom{TBuilder, TBuild}" />, allowing for post-build host configuration. It does nothing when the
    ///     host was not configured with <see cref="AddAtom{TBuilder, TBuild}" />. The build definition itself is not
    ///     resolved.
    /// </remarks>
    public static IHost UseAtom(this IHost host)
    {
        host
            .Services
            .GetService<AtomHostConfigurator>()
            ?.ConfigureHost(host);

        return host;
    }
}
