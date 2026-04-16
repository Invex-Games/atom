namespace DecSm.Atom.TestUtils;

[PublicAPI]
public class TestBuildVersionProvider : IBuildVersionProvider
{
    public SemVer Version { get; set; } = SemVer.Parse("1.0.0");
}
