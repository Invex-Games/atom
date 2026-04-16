namespace DecSm.Atom.Tests.BuildTests.Targets;

// ReSharper disable once RedundantExtendsListEntry
[BuildDefinition]
public partial class ConfigureBuilderAndHostBuild : MinimalBuildDefinition,
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
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted1", "true")]);
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface ITargetWithConfigureBuilderAndConfigureHost
{
    bool IsSetupExecuted2 { get; set; }

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted2", "true")]);

    protected static partial void ConfigureHost(IHost host) =>
        ((ITargetWithConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>())
        .IsSetupExecuted2 = true;
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface
    ITargetWithInheritAndConfigureBuilderAndConfigureHost : ITargetWithConfigureBuilderAndConfigureHost
{
    bool IsSetupExecuted3 { get; set; }

    protected new static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted3", "true")]);

    protected new static partial void ConfigureHost(IHost host) =>
        ((ITargetWithInheritAndConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>())
        .IsSetupExecuted3 = true;
}
