namespace Invex.Atom.Workflows.Dotnet.Nuget;

/// <summary>
///     A custom workflow step that configures NuGet feeds on a build agent by adding them to the <c>nuget.config</c> file.
/// </summary>
/// <remarks>
///     Attach this step to a job definition to ensure that the specified NuGet feeds are available before the main job
///     tasks run.
/// </remarks>
[PublicAPI]
public sealed record AddNugetFeedsStep : IAdditionalStepOption
{
    /// <summary>
    ///     Gets the list of NuGet feeds to be added to the build agent's configuration.
    /// </summary>
    public IReadOnlyList<NugetFeedOptions> FeedsToAdd { get; init; } = [];

    /// <summary>
    ///     Gets a value indicating whether the Atom tool version installed on the agent should be kept in
    ///     sync with the referenced Atom library version. Defaults to <c>true</c>.
    /// </summary>
    public bool SyncAtomToolVersionToLibraryVersion { get; init; } = true;

    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <inheritdoc />
    public int Order { get; init; } = -100;

    /// <summary>
    ///     Generates a standardized environment variable name for a given NuGet feed's authentication token.
    /// </summary>
    /// <param name="feedName">The name of the NuGet feed.</param>
    /// <returns>The conventional environment variable name for the feed's token (e.g., "NUGET_TOKEN_MY_FEED").</returns>
    public static string GetEnvVarNameForFeed(string feedName) =>
        $"NUGET_TOKEN_{feedName.Replace(" ", "_").ToUpper()}";
}
