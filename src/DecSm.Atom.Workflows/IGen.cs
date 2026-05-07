namespace DecSm.Atom.Workflows;

[PublicAPI]
public interface IGen : IBuildAccessor
{
    Target Gen =>
        t => t
            .DescribedAs("Generates workflow files");
}
