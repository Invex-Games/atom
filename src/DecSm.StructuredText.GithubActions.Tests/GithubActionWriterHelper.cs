using Environment = System.Environment;

namespace DecSm.StructuredText.GithubActions.Tests;

/// <summary>
///     Shared helper for all GitHub Action writer tests.
/// </summary>
internal static class GithubActionWriterHelper
{
    /// <summary>
    ///     Writes a <see cref="GithubAction" /> and returns the resulting text.
    /// </summary>
    public static string Write(GithubAction action)
    {
        var writer = new GithubActionWriter();
        writer.Write(action);

        return writer.TextWriter.ToString();
    }

    /// <summary>
    ///     Joins lines with <see cref="System.Environment.NewLine" />, adding a trailing newline.
    /// </summary>
    public static string JoinLines(params string[] lines) =>
        string.Concat(lines.Select(l => l + Environment.NewLine));

    /// <summary>
    ///     Creates a minimal <see cref="Job" /> suitable for use in writer tests.
    /// </summary>
    public static Job MinimalJob(string name = "build") =>
        new()
        {
            Name = new RawExpression(name),
            RunsOn = new()
            {
                Labels = [new RawExpression("ubuntu-latest")],
            },
            Steps =
            [
                new Step.RunStep
                {
                    Run = [new RawExpression("echo hello")],
                },
            ],
        };

    /// <summary>
    ///     Creates a minimal <see cref="GithubAction" /> using the supplied events (defaults to push on main).
    /// </summary>
    public static GithubAction MinimalAction(IReadOnlyList<On>? events = null) =>
        new()
        {
            On = events ??
            [
                new On.Push
                {
                    Branches = ["main"],
                    BranchesIgnore = null,
                    Tags = null,
                    TagsIgnore = null,
                    Paths = null,
                    PathsIgnore = null,
                },
            ],
            Jobs = [MinimalJob()],
        };
}
