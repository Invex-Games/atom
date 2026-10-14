namespace Invex.Atom.Module.Dotnet.Model;

/// <summary>
///     Represents a NuGet feed configuration, including its URL, name, and optional credentials.
/// </summary>
/// <param name="Url">The URL of the NuGet feed.</param>
/// <param name="Name">Optional. The name of the NuGet feed. Defaults to <c>Feed{index}</c> when written to a config file.</param>
/// <remarks>
///     Credentials are supplied as delegates so that secrets are only resolved when the NuGet.Config file is written.
/// </remarks>
/// <example>
///     <code>
/// var feed = new NugetFeed("https://pkgs.dev.azure.com/org/_packaging/feed/nuget/v3/index.json", "MyFeed")
/// {
///     Username = () => "user",
///     Password = () => FeedToken,
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record NugetFeed(string Url, string? Name = null)
{
    /// <summary>
    ///     Gets the delegate that provides the username for the NuGet feed, or <c>null</c> if none is required.
    /// </summary>
    public Func<string?>? Username { get; init; }

    /// <summary>
    ///     Gets the delegate that provides the (encrypted) password for the NuGet feed, or <c>null</c> if none is required.
    /// </summary>
    public Func<string?>? Password { get; init; }

    /// <summary>
    ///     Gets the delegate that provides the clear text password for the NuGet feed, or <c>null</c> if none is required.
    ///     Use with caution.
    /// </summary>
    public Func<string?>? PlainTextPassword { get; init; }
}
