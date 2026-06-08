namespace Invex.Atom.Workflows.Options;

[PublicAPI]
public record CheckoutStep : IAdditionalStepOption
{
    public bool Enabled { get; init; } = true;

    public int Order { get; init; } = -1000;
}
