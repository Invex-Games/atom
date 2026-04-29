namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public record CheckoutStep : IAdditionalStepOption
{
    public bool Value { get; init; } = true;

    public int Order { get; init; } = -1000;
}
