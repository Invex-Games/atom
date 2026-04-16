namespace DecSm.Atom.TestUtils;

[PublicAPI]
public static class TestUtils
{
    public static IHost CreateTestHost<T>(
        TestConsole? console = null,
        MockFileSystem? fileSystem = null,
        CommandLineArgs? commandLineArgs = null,
        TestBuildIdProvider? buildIdProvider = null,
        TestBuildVersionProvider? buildVersionProvider = null,
        Action<HostApplicationBuilder>? configure = null)
        where T : MinimalBuildDefinition
    {
        var builder = AtomHost.CreateAtomBuilder<T>([]);

        console ??= new();
        fileSystem ??= FileSystemUtils.DefaultMockFileSystem;
        commandLineArgs ??= new(true, []);

        if (!commandLineArgs.HasProject)
            commandLineArgs = commandLineArgs with
            {
                Args = commandLineArgs
                    .Args
                    .Append(new ProjectArg("AtomTest"))
                    .ToArray(),
            };

        buildIdProvider ??= new();
        buildVersionProvider ??= new();

        builder.Services.AddKeyedSingleton<IAnsiConsole>("StaticAccess", console);
        builder.Services.AddKeyedSingleton<IFileSystem>("RootFileSystem", fileSystem);
        builder.Services.AddSingleton(commandLineArgs);
        builder.Services.AddSingleton(buildIdProvider);
        builder.Services.AddSingleton(buildVersionProvider);

        configure?.Invoke(builder);

        return builder.Build();
    }
}
