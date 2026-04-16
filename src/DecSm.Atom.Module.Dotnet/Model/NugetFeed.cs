namespace DecSm.Atom.Module.Dotnet.Model;

/// <summary>
///     Represents a NuGet feed configuration, including its URL, name, and optional credentials.
/// </summary>
[PublicAPI]
public record NugetFeed
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NugetFeed" /> record with delegate-based credential providers.
    /// </summary>
    /// <param name="url">The URL of the NuGet feed.</param>
    /// <param name="name">Optional. The name of the NuGet feed.</param>
    /// <param name="username">Optional. A delegate that provides the username for the feed.</param>
    /// <param name="password">Optional. A delegate that provides the password for the feed.</param>
    /// <param name="plainTextPassword">
    ///     Optional. A delegate that provides the plain text password for the feed. Use with
    ///     caution.
    /// </param>
    public NugetFeed(
        string url,
        string? name = null,
        Func<string?>? username = null,
        Func<string?>? password = null,
        Func<string?>? plainTextPassword = null)
    {
        Url = url;
        Name = name;
        Username = username ?? (() => null);
        Password = password ?? (() => null);
        PlainTextPassword = plainTextPassword ?? (() => null);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NugetFeed" /> record with direct credential values.
    /// </summary>
    /// <param name="url">The URL of the NuGet feed.</param>
    /// <param name="name">Optional. The name of the NuGet feed.</param>
    /// <param name="username">Optional. The username for the feed.</param>
    /// <param name="password">Optional. The password for the feed.</param>
    /// <param name="plainTextPassword">Optional. The plain text password for the feed. Use with caution.</param>
    public NugetFeed(
        string url,
        string? name = null,
        string? username = null,
        string? password = null,
        string? plainTextPassword = null)
    {
        Url = url;
        Name = name;
        Username = () => username;
        Password = () => password;
        PlainTextPassword = () => plainTextPassword;
    }

    /// <summary>
    ///     Gets the URL of the NuGet feed.
    /// </summary>
    public string Url { get; private init; }

    /// <summary>
    ///     Gets the optional name of the NuGet feed.
    /// </summary>
    public string? Name { get; private init; }

    /// <summary>
    ///     Gets the delegate that provides the username for the NuGet feed.
    /// </summary>
    public Func<string?> Username { get; private init; }

    /// <summary>
    ///     Gets the delegate that provides the password for the NuGet feed.
    /// </summary>
    public Func<string?> Password { get; private init; }

    /// <summary>
    ///     Gets the delegate that provides the plain text password for the NuGet feed.
    /// </summary>
    public Func<string?> PlainTextPassword { get; private init; }
}
