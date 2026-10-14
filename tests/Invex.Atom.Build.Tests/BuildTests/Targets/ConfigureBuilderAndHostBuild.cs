namespace Invex.Atom.Build.Tests.BuildTests.Targets;

// ReSharper disable once RedundantExtendsListEntry
[BuildDefinition]
public partial class ConfigureBuilderAndHostBuild : BuildDefinition,
    ITargetWithConfigureBuilder,
    ITargetWithConfigureBuilderAndConfigureHost,
    ITargetWithInheritAndConfigureBuilderAndConfigureHost
{
    public bool IsSetupExecuted2 { get; set; }

    public bool IsSetupExecuted3 { get; set; }

    public bool IsOnConfigureHostExecuted { get; set; }

    static partial void OnConfigureHostBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([
            new("OnConfigureHostBuilderExecuted", "true"),

            // Module hooks run first, so their configuration is already visible here
            new("OnConfigureHostBuilderSawModuleHook", builder.Configuration["SetupExecuted1"]),
        ]);

    static partial void OnConfigureHost(IHost host) =>
        ((ConfigureBuilderAndHostBuild)host.Services.GetRequiredService<IBuildDefinition>()).IsOnConfigureHostExecuted =
            true;
}

[ConfigureHostBuilder]
public partial interface ITargetWithConfigureBuilder
{
    protected static partial void ConfigureHostBuilderFromITargetWithConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted1", "true")]);
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface ITargetWithConfigureBuilderAndConfigureHost
{
    bool IsSetupExecuted2 { get; set; }

    protected static partial void ConfigureHostBuilderFromITargetWithConfigureBuilderAndConfigureHost(
        IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted2", "true")]);

    protected static partial void ConfigureHostFromITargetWithConfigureBuilderAndConfigureHost(IHost host) =>
        ((ITargetWithConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>())
        .IsSetupExecuted2 = true;
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface
    ITargetWithInheritAndConfigureBuilderAndConfigureHost : ITargetWithConfigureBuilderAndConfigureHost
{
    bool IsSetupExecuted3 { get; set; }

    protected static partial void ConfigureHostBuilderFromITargetWithInheritAndConfigureBuilderAndConfigureHost(
        IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted3", "true")]);

    protected static partial void ConfigureHostFromITargetWithInheritAndConfigureBuilderAndConfigureHost(IHost host) =>
        ((ITargetWithInheritAndConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>())
        .IsSetupExecuted3 = true;
}
