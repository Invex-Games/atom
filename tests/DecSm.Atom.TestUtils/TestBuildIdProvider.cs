namespace DecSm.Atom.TestUtils;

[PublicAPI]
public class TestBuildIdProvider : IBuildIdProvider
{
    public string BuildId { get; set; } = "12345678";
}
