namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

/// <summary>
///     Shared helper for all pipeline writer tests.
/// </summary>
internal static class PipelineWriterHelper
{
    /// <summary>
    ///     Writes a pipeline and returns the resulting text.
    /// </summary>
    public static string Write(DevopsPipeline pipeline)
    {
        var writer = new DevopsPipelineWriter();
        writer.Write(pipeline);

        return writer.TextWriter.ToString();
    }

    /// <summary>
    ///     Joins lines with Environment.NewLine, adding a trailing newline.
    /// </summary>
    public static string JoinLines(params string[] lines) =>
        string.Concat(lines.Select(l => l + Environment.NewLine));
}
