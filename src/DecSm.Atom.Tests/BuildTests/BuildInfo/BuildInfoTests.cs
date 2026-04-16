namespace DecSm.Atom.Tests.BuildTests.BuildInfo;

[TestFixture]
public class BuildInfoTests
{
    private sealed record TestTimestampProvider(long Timestamp = 12345678) : IBuildTimestampProvider;

    [Test]
    public async Task BuildInfo_UsesDefaultValues()
    {
        // Arrange
        var testTimestampProvider = new TestTimestampProvider();

        var host = CreateTestHost<BuildInfoBuild>(commandLineArgs: new(true, [new CommandArg("BuildInfoTarget")]),
            configure: b => b.Services.AddSingleton<IBuildTimestampProvider>(testTimestampProvider));

        // Act
        var buildDefinition = host.Services.GetRequiredService<IBuildDefinition>();
        await host.RunAsync();

        // Assert

        buildDefinition
            .ShouldBeOfType<BuildInfoBuild>()
            .ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("Atom"),
                b => b.BuildIdResult.ShouldBe($"1.0.0-{testTimestampProvider.Timestamp}"),
                b => b.BuildVersionResult.ShouldBe(SemVer.Parse("1.0.0")),
                b => b.BuildTimestampResult.ShouldBe(testTimestampProvider.Timestamp));
    }

    [Test]
    public async Task BuildInfo_Config_OverridesDefaultValues()
    {
        // Arrange
        var testTimestampProvider = new TestTimestampProvider();

        var host = CreateTestHost<BuildInfoBuild>(commandLineArgs: new(true, [new CommandArg("BuildInfoTarget")]),
            configure: b =>
            {
                b.Services.AddSingleton<IBuildTimestampProvider>(testTimestampProvider);

                b.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Params:build-name", "TestBuildName" },
                    { "Params:build-id", "TestBuildId" },
                    { "Params:build-version", "2.1.3" },
                    { "Params:build-timestamp", "87654321" },
                });
            });

        // Act
        var buildDefinition = host.Services.GetRequiredService<IBuildDefinition>();
        await host.RunAsync();

        // Assert
        buildDefinition
            .ShouldBeOfType<BuildInfoBuild>()
            .ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("TestBuildName"),
                b => b.BuildIdResult.ShouldBe("TestBuildId"),
                b => b.BuildVersionResult.ShouldBe(SemVer.Parse("2.1.3")),
                b => b.BuildTimestampResult.ShouldBe(87654321));
    }

    [Test]
    [Parallelizable(ParallelScope.None)]
    public async Task BuildInfo_EnvironmentVariable_OverridesConfig()
    {
        // Arrange
        var testTimestampProvider = new TestTimestampProvider();

        Environment.SetEnvironmentVariable("BUILD_NAME", "EnvBuildName");
        Environment.SetEnvironmentVariable("BUILD_ID", "EnvBuildId");
        Environment.SetEnvironmentVariable("BUILD_VERSION", "4.3.2");
        Environment.SetEnvironmentVariable("BUILD_TIMESTAMP", "98765432");

        try
        {
            var host = CreateTestHost<BuildInfoBuild>(commandLineArgs: new(true, [new CommandArg("BuildInfoTarget")]),
                configure: b =>
                {
                    b.Services.AddSingleton<IBuildTimestampProvider>(testTimestampProvider);

                    b.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "Params:build-name", "ConfigBuildName" },
                        { "Params:build-id", "ConfigBuildId" },
                        { "Params:build-version", "3.2.1" },
                        { "Params:build-timestamp", "11223344" },
                    });
                });

            // Act
            var buildDefinition = host.Services.GetRequiredService<IBuildDefinition>();
            await host.RunAsync();

            // Assert
            buildDefinition
                .ShouldBeOfType<BuildInfoBuild>()
                .ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("EnvBuildName"),
                    b => b.BuildIdResult.ShouldBe("EnvBuildId"),
                    b => b.BuildVersionResult.ShouldBe(SemVer.Parse("4.3.2")),
                    b => b.BuildTimestampResult.ShouldBe(98765432));
        }
        finally
        {
            Environment.SetEnvironmentVariable("BUILD_NAME", null);
            Environment.SetEnvironmentVariable("BUILD_ID", null);
            Environment.SetEnvironmentVariable("BUILD_VERSION", null);
            Environment.SetEnvironmentVariable("BUILD_TIMESTAMP", null);
        }
    }
}
