namespace DecSm.Atom.Tests.BuildTests.Targets;

[TestFixture]
public class TargetTests
{
    [Test]
    public void TestTargetBuild_WithTestTargetArg_Executes_TestTarget()
    {
        // Arrange
        var executed = false;

        var host = CreateTestHost<TestTargetAtomBuild>(commandLineArgs: new(true, [new CommandArg("TestTarget")]));

        ((ITestTarget)host.Services.GetRequiredService<IBuildDefinition>()).Execute = () =>
        {
            executed = true;

            return Task.CompletedTask;
        };

        // Act
        host.Run();

        // Assert
        executed.ShouldBeTrue();
    }

    [Test]
    public void TestTargetBuild_WithoutTestTargetArg_Skips_TestTarget()
    {
        // Arrange
        var executed = false;

        var host = CreateTestHost<TestTargetAtomBuild>();

        ((ITestTarget)host.Services.GetRequiredService<IBuildDefinition>()).Execute = () =>
        {
            executed = true;

            return Task.CompletedTask;
        };

        // Act
        host.Run();

        // Assert
        executed.ShouldBeFalse();
    }

    [Test]
    public void TestDependencyTargetBuild_WithFirstTargetArg_ExecutesFirstTargetOnly()
    {
        // Arrange
        var host = CreateTestHost<DependencyTargetBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDependencyTarget1.DependencyTarget1))]));

        var build = (DependencyTargetBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.DependencyTarget1Executed.ShouldBeTrue();
        build.DependencyTarget2Executed.ShouldBeFalse();
    }

    [Test]
    public void TestDependencyTargetBuild_WithSecondTargetArg_ExecutesBothTargets()
    {
        // Arrange
        var host = CreateTestHost<DependencyTargetBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDependencyTarget2.DependencyTarget2))]));

        var build = (DependencyTargetBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.DependencyTarget1Executed.ShouldBeTrue();
        build.DependencyTarget2Executed.ShouldBeTrue();
    }

    [Test]
    public void TestDependencyTargetBuild_WithSecondTargetArg_AndFirstTargetFail_DoesNotExecuteSecondTarget()
    {
        // Arrange
        var host = CreateTestHost<DependencyTargetBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDependencyFailTarget2.DependencyFailTarget2))]));

        var build = (DependencyTargetBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.DependencyTarget1Executed.ShouldBeFalse();
        build.DependencyTarget2Executed.ShouldBeFalse();
        build.DependencyFailTarget1Executed.ShouldBeTrue();
        build.DependencyFailTarget2Executed.ShouldBeFalse();
    }

    [Test]
    public void TestCircularDependency1TargetBuild_Throws_Exception()
    {
        // Arrange
        var host = CreateTestHost<CircularTargetDependencyBuild>();
        var buildDefinition = (CircularTargetDependencyBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act / Assert
        Should.Throw<Exception>(() => host.RunAsync(), TimeSpan.FromSeconds(5));
        buildDefinition.CircularTarget1Executed.ShouldBeFalse();
        buildDefinition.CircularTarget2Executed.ShouldBeFalse();
    }

    [Test]
    public void TestCircularDependency2TargetBuild_Throws_Exception()
    {
        // Arrange
        var host = CreateTestHost<CircularTargetDependencyBuild2>();
        var buildDefinition = (CircularTargetDependencyBuild2)host.Services.GetRequiredService<IBuildDefinition>();

        // Act / Assert
        Should.Throw<Exception>(() => host.RunAsync(), TimeSpan.FromSeconds(5));
        buildDefinition.CircularTarget3Executed.ShouldBeFalse();
        buildDefinition.CircularTarget4Executed.ShouldBeFalse();
        buildDefinition.CircularTarget5Executed.ShouldBeFalse();
    }

    [Test]
    public void Target_Extension_ExecutesBaseTarget()
    {
        // Arrange
        var host = CreateTestHost<ExtensionTargetBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IExtendedExtensionTarget.ExtendedExtensionTarget))]));

        var build = (ExtensionTargetBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.BaseExtensionTargetExecuted.ShouldBeTrue();
        build.ExtendedExtensionTargetExecuted.ShouldBeTrue();
    }

    [Test]
    public void Target_Override_ExecutesOverrideTarget()
    {
        // Arrange
        var host = CreateTestHost<TargetOverrideBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IBaseOverrideTarget.OverrideTarget))]));

        var build = (TargetOverrideBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.BaseOverrideTargetExecuted.ShouldBeFalse();
        build.OverrideOverrideTargetExecuted.ShouldBeTrue();
    }

    [Test]
    public async Task Build_WithUnspecifiedTargets_IncludesUnspecifiedTargets()
    {
        // Arrange
        var host = CreateTestHost<UnspecifiedTargetsBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IUnspecifiedTarget2.UnspecifiedTarget2))]));

        // Act
        var build = host.Services.GetRequiredService<BuildModel>();

        // Assert
        await Verify(build);
    }

    [Test]
    public void Build_WithConfigureBuilder_ExecutesSetupMethods()
    {
        // Arrange / Act
        var host = CreateTestHost<ConfigureBuilderAndHostBuild>();

        // Assert
        var configuration = host.Services.GetService<IConfiguration>();

        using var _ = Assert.EnterMultipleScope();

        configuration.ShouldNotBeNull();

        configuration["SetupExecuted1"]
            .ShouldBe("true");

        configuration["SetupExecuted2"]
            .ShouldBe("true");

        configuration["SetupExecuted3"]
            .ShouldBe("true");
    }

    [Test]
    public void Build_WithConfigureHost_ExecutesSetupMethods()
    {
        // Arrange
        var host = CreateTestHost<ConfigureBuilderAndHostBuild>();

        // Act
        host.UseAtom();

        // Assert
        var target =
            (ITargetWithInheritAndConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>();

        target.IsSetupExecuted2.ShouldBeTrue();
        target.IsSetupExecuted3.ShouldBeTrue();
    }

    [Test]
    public void ApplyExtensions_WithDuplicateDependencies_DeduplicatesThem()
    {
        // Arrange
        var host = CreateTestHost<DuplicateDependenciesBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDuplicateDependenciesTarget.DuplicateDependenciesTarget))]));

        // Act
        var build = host.Services.GetRequiredService<BuildModel>();

        // Assert
        var target =
            build.Targets.Single(t => t.Name == nameof(IDuplicateDependenciesTarget.DuplicateDependenciesTarget));

        target.Dependencies.Count.ShouldBe(1, "Dependencies should be deduplicated");
    }

    [Test]
    public void ApplyExtensions_WithDuplicateArtifacts_DeduplicatesThem()
    {
        // Arrange
        var host = CreateTestHost<DuplicateArtifactsBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDuplicateArtifactsTarget.DuplicateArtifactsTarget))]));

        // Act
        var build = host.Services.GetRequiredService<BuildModel>();

        // Assert
        var target = build.Targets.Single(t => t.Name == nameof(IDuplicateArtifactsTarget.DuplicateArtifactsTarget));
        target.ConsumedArtifacts.Count.ShouldBe(1, "Consumed artifacts should be deduplicated");
        target.ProducedArtifacts.Count.ShouldBe(1, "Produced artifacts should be deduplicated");
    }

    [Test]
    public void ApplyExtensions_WithDuplicateVariables_DeduplicatesThem()
    {
        // Arrange
        var host = CreateTestHost<DuplicateVariablesBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IDuplicateVariablesTarget.DuplicateVariablesTarget))]));

        // Act
        var build = host.Services.GetRequiredService<BuildModel>();

        // Assert
        var target = build.Targets.Single(t => t.Name == nameof(IDuplicateVariablesTarget.DuplicateVariablesTarget));
        target.ConsumedVariables.Count.ShouldBe(1, "Consumed variables should be deduplicated");
        target.ProducedVariables.Count.ShouldBe(1, "Produced variables should be deduplicated");
    }
}
