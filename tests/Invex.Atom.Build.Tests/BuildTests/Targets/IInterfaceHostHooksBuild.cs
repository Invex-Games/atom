namespace Invex.Atom.Build.Tests.BuildTests.Targets;

// An interface-based build can configure the host from the build interface itself
[BuildDefinition]
[ConfigureHostBuilder]
[ConfigureHost]
public partial interface IInterfaceHostHooksBuild : IBuildDefinition, ITargetWithConfigureBuilder
{
    protected static partial void ConfigureHostBuilderFromIInterfaceHostHooksBuild(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("InterfaceBuildHookExecuted", "true")]);

    protected static partial void ConfigureHostFromIInterfaceHostHooksBuild(IHost host) =>
        host
            .Services
            .GetRequiredService<InterfaceHostHooksRecorder>()
            .Calls
            .Add(nameof(ConfigureHostFromIInterfaceHostHooksBuild));
}

// ...or from the generated build class, which is partial
internal sealed partial class InterfaceHostHooksBuild
{
    static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<InterfaceHostHooksRecorder>();

    static partial void OnConfigureHost(IHost host) =>
        host
            .Services
            .GetRequiredService<InterfaceHostHooksRecorder>()
            .Calls
            .Add(nameof(OnConfigureHost));
}

public sealed class InterfaceHostHooksRecorder
{
    public List<string> Calls { get; } = [];
}
