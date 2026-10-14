namespace Invex.Atom.Build.Tests.ClassTests.Util;

[TestFixture]
internal sealed class TaskExtensionsTests
{
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
