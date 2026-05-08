namespace DecSm.Atom.Build.Tests.BuildTests.Targets;

// ReSharper disable once RedundantExtendsListEntry
[BuildDefinition]
public partial class ConfigureBuilderAndHostBuild : BuildDefinition,
    ITargetWithConfigureBuilder,
    ITargetWithConfigureBuilderAndConfigureHost,
    ITargetWithInheritAndConfigureBuilderAndConfigureHost
{
    public bool IsSetupExecuted2 { get; set; }

    public bool IsSetupExecuted3 { get; set; }
}

[ConfigureHostBuilder]
public partial interface ITargetWithConfigureBuilder
{
    protected static partial void ConfigureBuilderFromITargetWithConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted1", "true")]);
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface ITargetWithConfigureBuilderAndConfigureHost
{
    bool IsSetupExecuted2 { get; set; }

    protected static partial void ConfigureBuilderFromITargetWithConfigureBuilderAndConfigureHost(
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

    protected static partial void ConfigureBuilderFromITargetWithInheritAndConfigureBuilderAndConfigureHost(
        IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted3", "true")]);

    protected static partial void ConfigureHostFromITargetWithInheritAndConfigureBuilderAndConfigureHost(IHost host) =>
        ((ITargetWithInheritAndConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>())
        .IsSetupExecuted3 = true;
}
