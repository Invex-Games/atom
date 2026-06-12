namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that adds a source-checkout step to a workflow job.
/// </summary>
/// <remarks>
///     Rendered using the targeted platform's native checkout mechanism (e.g., <c>actions/checkout</c> in
///     GitHub Actions). The default <see cref="Order" /> of -1000 places the step at the start of the job,
///     before the target step.
/// </remarks>
[PublicAPI]
public record CheckoutStep : IAdditionalStepOption
{
    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public int Order { get; init; } = -1000;
}
