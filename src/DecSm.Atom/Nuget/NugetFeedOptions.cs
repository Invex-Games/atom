namespace DecSm.Atom.Nuget;

/// <summary>
///     Represents the configuration for a NuGet feed to be added during a build workflow.
/// </summary>
/// <remarks>
///     This record defines a NuGet package source, including its name, URL, and the secret required for authentication.
///     It is typically used to grant build workflows access to private or custom NuGet repositories.
/// </remarks>
/// <example>
///     <code>
/// new NugetFeedOptions
/// {
///     FeedName = "MyPrivateFeed",
///     FeedUrl = "https://nuget.pkg.github.com/MyOrg/index.json",
///     SecretName = "PRIVATE_NUGET_API_KEY"
/// }
///     </code>
/// </example>
[PublicAPI]
public sealed record NugetFeedOptions
{
    /// <summary>
    ///     Gets the name used to identify the NuGet feed in configurations and logs.
    /// </summary>
    public required string FeedName { get; init; }

    /// <summary>
    ///     Gets the URL of the NuGet package source endpoint.
    /// </summary>
    public required string FeedUrl { get; init; }

    /// <summary>
    ///     Gets the name of the secret (e.g., in GitHub Secrets) that stores the API key or token
    ///     for authenticating with the NuGet feed.
    /// </summary>
    public required string SecretName { get; init; }
}
