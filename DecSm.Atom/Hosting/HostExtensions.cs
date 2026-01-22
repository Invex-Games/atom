namespace DecSm.Atom.Hosting;

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
    /// <typeparam name="TBuild">The type of the build definition, which must implement <see cref="MinimalBuildDefinition" />.</typeparam>
    /// <param name="builder">The host application builder instance to configure.</param>
    /// <param name="args">The command-line arguments provided to the application.</param>
    /// <returns>The configured host application builder instance.</returns>
    /// <remarks>
    ///     This method performs the following key configurations:
    ///     <list type="bullet">
    ///         <item>Registers the <see cref="AtomService" /> as a hosted service.</item>
    ///         <item>Registers the build definition (<typeparamref name="TBuild" />) and related interfaces.</item>
    ///         <item>
    ///             Adds core Atom services like <see cref="IParamService" />, <see cref="ReportService" />, and
    ///             <see cref="IAnsiConsole" />.
    ///         </item>
    ///         <item>
    ///             Registers default providers for build information: <see cref="IBuildIdProvider" />,
    ///             <see cref="IBuildVersionProvider" />, and <see cref="IBuildTimestampProvider" />.
    ///         </item>
    ///         <item>Configures file system access via <see cref="IAtomFileSystem" />.</item>
    ///         <item>Registers build execution and workflow generation services.</item>
    ///         <item>Sets up logging with Spectre.Console and report providers, filtering out verbose Microsoft host logs.</item>
    ///         <item>Parses command-line arguments and makes them available as <see cref="CommandLineArgs" />.</item>
    ///         <item>Resolves and registers the <see cref="BuildModel" />.</item>
    ///         <item>Invokes <see cref="IConfigureHost.ConfigureBuildHostBuilder" /> if implemented by the build definition.</item>
    ///     </list>
    /// </remarks>
    public static TBuilder AddAtom<TBuilder,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuild>(
        this TBuilder builder,
        string[] args)
        where TBuilder : IHostApplicationBuilder
        where TBuild : MinimalBuildDefinition
    {
        builder.Services.AddHostedService<AtomService>();
        builder.Services.AddSingleton<MinimalBuildDefinition, TBuild>();
        builder.Services.AddSingleton<IBuildDefinition>(x => x.GetRequiredService<MinimalBuildDefinition>());

        // ReSharper disable once SuspiciousTypeConversion.Global - Checked before casting
        if (typeof(TBuild).IsAssignableTo(typeof(IConfigureHost)))
            builder.Services.AddSingleton<IConfigureHost>(x =>
                (IConfigureHost)x.GetRequiredService<MinimalBuildDefinition>());

        builder.Services.AddSingletonWithStaticAccessor<IParamService, ParamService>();
        builder.Services.AddSingletonWithStaticAccessor<ReportService>();

        builder.Services.AddSingletonWithStaticAccessor<IAnsiConsole>((_, _) =>
        {
            // Wrap Spectre's console output so all rendered text is masked for secrets before being written
            var innerOutput = AnsiConsole.Console.Profile.Out;

            var settings = new AnsiConsoleSettings
            {
                Out = new MaskingAnsiConsoleOutput(innerOutput),
            };

            return AnsiConsole.Create(settings);
        });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IBuildIdProvider, DefaultBuildIdProvider>();
        builder.Services.AddSingleton<IBuildVersionProvider, DefaultBuildVersionProvider>();
        builder.Services.AddSingleton<IBuildTimestampProvider, DefaultBuildTimestampProvider>();

        builder
            .Services
            .AddKeyedSingleton<IFileSystem>("RootFileSystem", new FileSystem())
            .AddSingletonWithStaticAccessor<IAtomFileSystem>((x, _) =>
                new AtomFileSystem(x.GetRequiredService<ILogger<AtomFileSystem>>())
                {
                    FileSystem = x.GetRequiredKeyedService<IFileSystem>("RootFileSystem"),
                    PathLocators = x
                        .GetServices<IPathProvider>()
                        .OrderByDescending(l => l.Priority)
                        .ToList(),
                    ProjectName = x.GetRequiredService<CommandLineArgs>()
                        .ProjectName is { Length: > 0 } p
                        ? p
                        : Assembly.GetEntryAssembly()!.GetName()
                            .Name!,
                })
            .AddSingletonWithStaticAccessor<IFileSystem>((x, _) => x.GetRequiredService<IAtomFileSystem>());

        builder.Services.AddSingleton<BuildExecutor>();
        builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
        builder.Services.AddSingleton<IOutcomeReportWriter, ConsoleOutcomeReportWriter>();

        builder.Services.AddSingleton<WorkflowGenerator>();
        builder.Services.AddSingleton<IWorkflowExpressionGenerator, WorkflowExpressionGenerator>();
        builder.Services.AddSingleton<IWorkflowVariableProvider, AtomWorkflowVariableProvider>();
        builder.Services.TryAddSingleton<IWorkflowVariableService, WorkflowVariableService>();

        builder.Services.TryAddSingleton<IBuildTimestampProvider, DefaultBuildTimestampProvider>();
        builder.Services.TryAddSingleton<IBuildVersionProvider, DefaultBuildVersionProvider>();
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

        builder.Services.AddSingleton<BuildResolver>();
        builder.Services.AddSingleton<WorkflowResolver>();

        builder.Services.AddSingleton<BuildModel>(services => services
            .GetRequiredService<BuildResolver>()
            .Resolve());

        builder.Logging.ClearProviders();

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddProvider(new SpectreLoggerProvider());
        builder.Logging.AddProvider(new ReportLoggerProvider());

        builder.Logging.AddFilter((context, level) => (context, level) switch
        {
            ("Microsoft.Hosting.Lifetime", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting.Internal.Host", < LogLevel.Warning) => false,
            _ => true,
        });

        using var tempBuild = builder.Services.BuildServiceProvider();

        tempBuild
            .GetService<IConfigureHost>()
            ?.ConfigureBuildHostBuilder(builder);

        return builder;
    }

    /// <summary>
    ///     Configures the built <see cref="IHost" /> instance for Atom-specific behaviors.
    /// </summary>
    /// <param name="host">The <see cref="IHost" /> instance to configure.</param>
    /// <returns>The configured <see cref="IHost" /> instance.</returns>
    /// <remarks>
    ///     This method invokes <see cref="IConfigureHost.ConfigureBuildHost" /> if implemented by the build definition,
    ///     allowing for post-build host configuration.
    /// </remarks>
    public static IHost UseAtom(this IHost host)
    {
        host
            .Services
            .GetService<IConfigureHost>()
            ?.ConfigureBuildHost(host);

        return host;
    }
}
