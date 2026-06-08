namespace Invex.Atom.Build;

/// <summary>
///     Defines a hook that can participate in the Atom build lifecycle.
/// </summary>
/// <remarks>
///     <para>
///         Implementations of this interface can be registered in the dependency injection container
///         to receive callbacks at specific points in the Atom build lifecycle. Multiple hooks can
///         be registered and will be invoked in registration order.
///     </para>
///     <para>
///         All methods have default no-op implementations, so implementers only need to override the
///         methods they are interested in.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     public class MyLifecycleHook : IAtomLifecycleHook
///     {
///         public Task BeforeExecute(CancellationToken cancellationToken)
///         {
///             // Check preconditions before targets are executed
///             return Task.CompletedTask;
///         }
///     }
///     </code>
/// </example>
[PublicAPI]
public interface IAtomLifecycleHook
{
    /// <summary>
    ///     Called after the build model has been resolved and before build targets are executed.
    /// </summary>
    /// <remarks>
    ///     This is an ideal point for modules to perform validation or generate files
    ///     that must be up-to-date before the build proceeds (e.g., workflow generation checks).
    ///     Throwing an exception from this method will prevent the build from executing.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeforeExecute(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <summary>
    ///     Called after all build targets have completed execution (regardless of success or failure).
    /// </summary>
    /// <remarks>
    ///     This hook is called in a finally-like manner, allowing modules to perform cleanup or
    ///     post-execution processing.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AfterExecute(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
