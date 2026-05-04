namespace DecSm.Atom.Process.Tests;

[TestFixture]
internal sealed class ProcessHostExtensionsTests
{
    [Test]
    public void AddProcessRunner_RegistersIProcessRunner()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProcessRunner();

        // Act
        using var sp = services.BuildServiceProvider();
        var runner = sp.GetService<IProcessRunner>();

        // Assert
        runner.ShouldNotBeNull();
    }

    [Test]
    public void AddProcessRunner_IProcessRunner_IsRegisteredAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProcessRunner();

        // Act
        using var sp = services.BuildServiceProvider();
        var first = sp.GetRequiredService<IProcessRunner>();
        var second = sp.GetRequiredService<IProcessRunner>();

        // Assert
        second.ShouldBeSameAs(first);
    }

    [Test]
    public void AddProcessRunner_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returned = services.AddProcessRunner();

        // Assert
        returned.ShouldBeSameAs(services);
    }
}
