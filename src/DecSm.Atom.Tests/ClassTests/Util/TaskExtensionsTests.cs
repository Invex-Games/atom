namespace DecSm.Atom.Tests.ClassTests.Util;

[TestFixture]
public class TaskExtensionsTests
{
    [Test]
    public async Task WithRetry_NullTask_ReturnsCompletedTask()
    {
        // Arrange
        Task? task = null;

        // Act & Assert
        await task.WithRetry();
    }

    [Test]
    public async Task WithRetry_SuccessfulTask_CompletesWithoutException()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act & Assert
        await task.WithRetry(3, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void WithRetry_NegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => task.WithRetry(-1));

        // Assert
        ex.ParamName.ShouldBe("retryCount");
    }

    [Test]
    public async Task WithRetry_FaultedTask_WithZeroRetries_ThrowsOriginalException()
    {
        // Arrange
        var original = new InvalidOperationException("boom");
        var task = Task.FromException(original);

        // Act
        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await task.WithRetry(0, TimeSpan.FromMilliseconds(1)));

        // Assert
        ex.Message.ShouldBe("boom");
    }

    [Test]
    public async Task WithRetry_FaultedTask_WithRetries_ThrowsAggregateWithExpectedInnerCount()
    {
        // Arrange
        var task = Task.FromException(new InvalidOperationException("boom"));
        const int retryCount = 2; // total attempts = retryCount + 1 = 3

        // Act
        var aggregate = await Should.ThrowAsync<AggregateException>(async () =>
            await task.WithRetry(retryCount, TimeSpan.FromMilliseconds(1)));

        // Assert
        aggregate.InnerExceptions.Count.ShouldBe(retryCount + 1);

        aggregate
            .InnerExceptions
            .All(e => e is InvalidOperationException)
            .ShouldBeTrue();
    }

    [Test]
    public async Task WithRetry_CanceledTask_IsNotRetried_ThrowsTaskCanceledImmediately()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var task = Task.FromCanceled(cts.Token);

        // Act
        var ex = await Should.ThrowAsync<TaskCanceledException>(async () =>
            await task.WithRetry(3, TimeSpan.FromMilliseconds(25)));

        // Assert
        ex.ShouldNotBeNull();
    }

    [Test]
    public async Task WithRetry_OperationCanceledException_IsRethrown()
    {
        // Arrange
        var task = Task.Run(() => throw new OperationCanceledException());

        // Act
        var ex = await Should.ThrowAsync<OperationCanceledException>(async () =>
            await task.WithRetry(3, TimeSpan.FromMilliseconds(25)));

        // Assert
        ex.ShouldNotBeNull();
    }
}
