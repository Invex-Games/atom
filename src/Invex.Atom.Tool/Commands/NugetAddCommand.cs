namespace Invex.Atom.Tool.Commands;

/// <summary>
///     Handles the command for adding a NuGet package source to the user's NuGet configuration.
/// </summary>
/// <remarks>
///     This command checks for existing NuGet sources and adds a new one if it doesn't already exist.
///     It attempts to retrieve an API key from environment variables for authentication.
/// </remarks>
internal static partial class NugetAddCommand
{
    /// <summary>
    ///     Executes the logic to add a NuGet package source.
    /// </summary>
    /// <param name="name">The name of the NuGet source to add.</param>
    /// <param name="url">The URL of the NuGet source to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The exit code of the operation (0 for success, 1 for failure).</returns>
    public static async Task<int> Handle(string name, string url, CancellationToken cancellationToken)
    {
        Console.WriteLine("Fetching NuGet sources...");

        // List existing NuGet sources to check for duplicates
        var listSourceProcess = Process.Start(new ProcessStartInfo("dotnet", "nuget list source")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        })!;

        // Read both streams before waiting for exit, so a full pipe buffer cannot deadlock the child process
        var listSourceOutputTask = listSourceProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        var listSourceErrorTask = listSourceProcess.StandardError.ReadToEndAsync(cancellationToken);
        await listSourceProcess.WaitForExitAsync(cancellationToken);
        var listSourceOutput = await listSourceOutputTask;
        var listSourceError = await listSourceErrorTask;

        if (listSourceProcess.ExitCode is not 0)
        {
            await Console.Error.WriteLineAsync("Error: Failed to list NuGet sources.");
            await Console.Error.WriteLineAsync(listSourceOutput);
            await Console.Error.WriteLineAsync(listSourceError);

            return 1;
        }

        if (IsSourcePresent(listSourceOutput, name, url))
        {
            Console.WriteLine($"NuGet feed '{name}' with URL '{url}' is already present, skipping addition.");

            return 0;
        }

        // Attempt to retrieve API key from environment variable
        // Environment variable name format: NUGET_TOKEN_<FEED_NAME_UPPERCASE_UNDERSCORE>
        var secret = Environment.GetEnvironmentVariable($"NUGET_TOKEN_{name.Replace(" ", "_").ToUpperInvariant()}");

        // Basic sanitization for command-line arguments
        var feedName = new string(name
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray());

        var feedUrl = new string(url
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '/' || c == ':' || c == '.')
            .ToArray());

        // Sanitize secret (remove quotes and newlines if present)
        secret = secret
                     ?.Replace("\"", "")
                     .Replace("\n", "")
                     .Replace("\r", "") ??
                 string.Empty;

        Console.WriteLine($"Adding NuGet feed '{name}'...");

        // Execute 'dotnet nuget add source' command
        var addSourceProcess = Process.Start(new ProcessStartInfo("dotnet")
        {
            ArgumentList =
            {
                "nuget",
                "add",
                "source",
                "--name",
                feedName,
            },
            RedirectStandardError = true,
        }.WithCredentials(secret, feedUrl))!;

        var addSourceErrorTask = addSourceProcess.StandardError.ReadToEndAsync(cancellationToken);
        await addSourceProcess.WaitForExitAsync(cancellationToken);
        var addSourceError = await addSourceErrorTask;

        if (addSourceProcess.ExitCode is 0)
        {
            Console.WriteLine($"NuGet feed '{name}' added successfully.");

            return 0;
        }

        await Console.Error.WriteLineAsync($"Error: Failed to add NuGet feed '{name}'.");
        await Console.Error.WriteLineAsync(addSourceError);

        return 1;
    }

    /// <summary>
    ///     Determines whether <c>dotnet nuget list source</c> output already contains a source with the given name or URL.
    /// </summary>
    /// <param name="listSourceOutput">The output of <c>dotnet nuget list source</c>.</param>
    /// <param name="name">The source name to look for.</param>
    /// <param name="url">The source URL to look for.</param>
    /// <returns><c>true</c> if a source with exactly that name or URL is registered.</returns>
    internal static bool IsSourcePresent(string listSourceOutput, string name, string url)
    {
        foreach (var line in listSourceOutput.Split('\n'))
        {
            var trimmed = line.Trim();

            if (trimmed.Equals(url.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;

            var match = SourceNameRegex()
                .Match(trimmed);

            if (match.Success &&
                match
                    .Groups["name"]
                    .Value
                    .Equals(name.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static ProcessStartInfo WithCredentials(this ProcessStartInfo startInfo, string secret, string feedUrl)
    {
        if (secret is { Length: > 0 })
        {
            startInfo.ArgumentList.Add("--username");
            startInfo.ArgumentList.Add("USERNAME"); // Username is often a placeholder when using API keys
            startInfo.ArgumentList.Add("--password");
            startInfo.ArgumentList.Add(secret);

            // Encrypted passwords are only supported on Windows
            if (!OperatingSystem.IsWindows())
                startInfo.ArgumentList.Add("--store-password-in-clear-text");
        }

        startInfo.ArgumentList.Add(feedUrl);

        return startInfo;
    }

    // Matches lines like "  1.  nuget.org [Enabled]"
    [GeneratedRegex(@"^\d+\.\s+(?<name>.+?)\s+\[[^\]]+\]$")]
    private static partial Regex SourceNameRegex();
}
