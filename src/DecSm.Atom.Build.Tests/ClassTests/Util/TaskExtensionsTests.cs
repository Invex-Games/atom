namespace DecSm.Atom.Build.Tests.ClassTests.Util;

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

    [Test]
    public async Task WithRetry_GenericNullTask_ReturnsDefaultValue()
    {
        Task<int>? task = null;
        var result = await task.WithRetry();
        result.ShouldBe(default);
    }

    [Test]
    public async Task WithRetry_SuccessfulGenericTask_ReturnsResult()
    {
        var task = Task.FromResult(42);
        var result = await task.WithRetry(3, TimeSpan.FromMilliseconds(1));
        result.ShouldBe(42);
    }

    [Test]
    public void WithRetry_GenericTask_NegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        var task = Task.FromResult(1);
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => task.WithRetry(-1));
        ex.ParamName.ShouldBe("retryCount");
    }

    [Test]
    public async Task WithRetry_GenericFaultedTask_WithZeroRetries_ThrowsOriginalException()
    {
        var task = Task.FromException<int>(new InvalidOperationException("oops"));

        var ex = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await task.WithRetry(0, TimeSpan.FromMilliseconds(1)));

        ex.Message.ShouldBe("oops");
    }

    [Test]
    public async Task WithRetry_GenericFaultedTask_WithRetries_ThrowsAggregateException()
    {
        var task = Task.FromException<int>(new InvalidOperationException("oops"));
        const int retryCount = 2;

        var aggregate = await Should.ThrowAsync<AggregateException>(async () =>
            await task.WithRetry(retryCount, TimeSpan.FromMilliseconds(1)));

        aggregate.InnerExceptions.Count.ShouldBe(retryCount + 1);
    }

    [Test]
    public async Task WithRetry_GenericCanceledTask_IsNotRetried()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var task = Task.FromCanceled<int>(cts.Token);

        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await task.WithRetry(3, TimeSpan.FromMilliseconds(25)));
    }

    [Test]
    public async Task WithRetry_FactorySucceeds_CompletesWithoutException()
    {
        var callCount = 0;

        var factory = () =>
        {
            callCount++;

            return Task.CompletedTask;
        };

        await factory.WithRetry(3, TimeSpan.FromMilliseconds(1));
        callCount.ShouldBe(1);
    }

    [Test]
    public async Task WithRetry_FactoryFailsThenSucceeds_RetriesAndSucceeds()
    {
        var callCount = 0;

        var factory = () =>
        {
            callCount++;

            return callCount < 3
                ? Task.FromException(new InvalidOperationException("fail"))
                : Task.CompletedTask;
        };

        await factory.WithRetry(5, TimeSpan.FromMilliseconds(1));
        callCount.ShouldBe(3);
    }

    [Test]
    public async Task WithRetry_FactoryAlwaysFails_ThrowsAggregateException()
    {
        var factory = () => Task.FromException(new InvalidOperationException("always fails"));

        var aggregate =
            await Should.ThrowAsync<AggregateException>(async () =>
                await factory.WithRetry(2, TimeSpan.FromMilliseconds(1)));

        aggregate.InnerExceptions.Count.ShouldBe(3); // initial + 2 retries
    }

    [Test]
    public void WithRetry_FactoryNull_ThrowsArgumentNullException()
    {
        Func<Task>? factory = null;
        Should.Throw<ArgumentNullException>(() => factory!.WithRetry());
    }

    [Test]
    public void WithRetry_FactoryNegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        var factory = () => Task.CompletedTask;
        Should.Throw<ArgumentOutOfRangeException>(() => factory.WithRetry(-1));
    }

    [Test]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task WithRetry_FactoryCanceled_IsNotRetried()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var factory = () => Task.FromCanceled(cts.Token);

        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await factory.WithRetry(3, TimeSpan.FromMilliseconds(1), cts.Token));
    }

    [Test]
    public async Task WithRetry_GenericFactorySucceeds_ReturnsResult()
    {
        var factory = () => Task.FromResult(99);
        var result = await factory.WithRetry(3, TimeSpan.FromMilliseconds(1));
        result.ShouldBe(99);
    }

    [Test]
    public async Task WithRetry_GenericFactoryFailsThenSucceeds_RetriesAndReturnsResult()
    {
        var callCount = 0;

        var factory = () =>
        {
            callCount++;

            return callCount < 3
                ? Task.FromException<int>(new InvalidOperationException("fail"))
                : Task.FromResult(7);
        };

        var result = await factory.WithRetry(5, TimeSpan.FromMilliseconds(1));
        result.ShouldBe(7);
        callCount.ShouldBe(3);
    }

    [Test]
    public async Task WithRetry_GenericFactoryAlwaysFails_ThrowsAggregateException()
    {
        var factory = () => Task.FromException<string>(new InvalidOperationException("always fails"));

        var aggregate =
            await Should.ThrowAsync<AggregateException>(async () =>
                await factory.WithRetry(2, TimeSpan.FromMilliseconds(1)));

        aggregate.InnerExceptions.Count.ShouldBe(3);
    }

    [Test]
    public void WithRetry_GenericFactoryNull_ThrowsArgumentNullException()
    {
        Func<Task<int>>? factory = null;
        Should.Throw<ArgumentNullException>(() => factory!.WithRetry());
    }

    [Test]
    public void WithRetry_GenericFactoryNegativeRetryCount_ThrowsArgumentOutOfRangeException()
    {
        var factory = () => Task.FromResult(1);
        Should.Throw<ArgumentOutOfRangeException>(() => factory.WithRetry(-1));
    }
}
