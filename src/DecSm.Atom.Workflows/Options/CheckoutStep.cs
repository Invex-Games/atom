namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public record CheckoutStep : IAdditionalStepOption
{
    public int Order { get; init; } = -1000;
}
