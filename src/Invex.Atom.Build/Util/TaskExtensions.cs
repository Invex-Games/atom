namespace Invex.Atom.Build.Util;

/// <summary>
///     Provides extension methods that retry asynchronous operations created by a task factory.
/// </summary>
[PublicAPI]
public static class TaskExtensions
{
    /// <summary>
    ///     Retries an asynchronous operation produced by a factory when it faults. A new task is created for each attempt.
    /// </summary>
    /// <param name="taskFactory">A delegate that creates a new <see cref="Task" /> for each attempt.</param>
    /// <param name="retryCount">The number of retries to attempt after the initial failure.</param>
    /// <param name="retryDelay">The delay between retry attempts. Defaults to 1 second if not specified.</param>
    /// <param name="cancellationToken">A token to observe before each attempt and during delays.</param>
    /// <returns>
    ///     A task that completes when an attempt succeeds, or throws an <see cref="AggregateException" /> after all
    ///     retries fail.
    /// </returns>
    /// <remarks>
    ///     Cancellation-related exceptions are immediately rethrown and do not trigger a retry.
    /// </remarks>
    public static Task WithRetry(
        this Func<Task> taskFactory,
        int retryCount = 5,
        TimeSpan retryDelay = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (retryDelay == TimeSpan.Zero)
            retryDelay = TimeSpan.FromSeconds(1);

        return Task.Run(async () =>
            {
                Exception? exception = null;

                for (var attempt = 0; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await taskFactory()
                            .ConfigureAwait(false);

                        return;
                    }
                    catch (StackOverflowException)
                    {
                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        exception = exception switch
                        {
                            null => ex,
                            AggregateException aggregateException => new AggregateException(
                                aggregateException.InnerExceptions.Append(ex)),
                            _ => new AggregateException(exception, ex),
                        };

                        if (attempt < retryCount)
                            await Task
                                .Delay(retryDelay, cancellationToken)
                                .ConfigureAwait(false);
                        else
                            throw exception;
                    }
                }
            },
            cancellationToken);
    }

    /// <summary>
    ///     Retries an asynchronous operation produced by a factory when it faults. A new task is created for each attempt.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    /// <param name="taskFactory">A delegate that creates a new <see cref="Task{TResult}" /> for each attempt.</param>
    /// <param name="retryCount">The number of retries to attempt after the initial failure.</param>
    /// <param name="retryDelay">The delay between retry attempts. Defaults to 1 second if not specified.</param>
    /// <param name="cancellationToken">A token to observe before each attempt and during delays.</param>
    /// <returns>
    ///     A task that completes with a result if successful, or throws an <see cref="AggregateException" /> after all
    ///     retries fail.
    /// </returns>
    /// <remarks>
    ///     Cancellation-related exceptions are immediately rethrown and do not trigger a retry.
    /// </remarks>
    public static Task<T> WithRetry<T>(
        this Func<Task<T>> taskFactory,
        int retryCount = 5,
        TimeSpan retryDelay = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (retryDelay == TimeSpan.Zero)
            retryDelay = TimeSpan.FromSeconds(1);

        return Task.Run(async () =>
            {
                Exception? exception = null;

                for (var attempt = 0; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        return await taskFactory()
                            .ConfigureAwait(false);
                    }
                    catch (StackOverflowException)
                    {
                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        exception = exception switch
                        {
                            null => ex,
                            AggregateException aggregateException => new AggregateException(
                                aggregateException.InnerExceptions.Append(ex)),
                            _ => new AggregateException(exception, ex),
                        };

                        if (attempt < retryCount)
                            await Task
                                .Delay(retryDelay, cancellationToken)
                                .ConfigureAwait(false);
                        else
                            throw exception;
                    }
                }

                // Should never reach here
                // ReSharper disable once HeuristicUnreachableCode
                return default!;
            },
            cancellationToken);
    }
}
