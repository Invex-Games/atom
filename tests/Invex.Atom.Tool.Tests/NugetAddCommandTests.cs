namespace Invex.Atom.Tool.Tests;

[TestFixture]
internal sealed class NugetAddCommandTests
{
    private const string ListOutput = """
                                      Registered Sources:
                                        1.  nuget.org [Enabled]
                                            https://api.nuget.org/v3/index.json
                                        2.  my-feed-extra [Disabled]
                                            https://pkgs.example.com/feed/index.json
                                      """;

    [TestCase("nuget.org", "https://other.example.com", true)]
    [TestCase("other", "https://api.nuget.org/v3/index.json", true)]
    [TestCase("my-feed", "https://pkgs.example.com/other/index.json", false)]
    [TestCase("feed", "https://pkgs.example.com/feed", false)]
    [TestCase("MY-FEED-EXTRA", "https://x", true)]
    public void IsSourcePresent_MatchesWholeNamesAndUrlsOnly(string name, string url, bool expected) =>
        NugetAddCommand
            .IsSourcePresent(ListOutput, name, url)
            .ShouldBe(expected);
}
