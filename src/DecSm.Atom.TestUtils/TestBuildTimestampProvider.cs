namespace DecSm.Atom.TestUtils;

[PublicAPI]
public sealed class TestBuildTimestampProvider : IBuildTimestampProvider
{
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
