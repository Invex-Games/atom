namespace DecSm.Atom.Tool.Commands;

/// <summary>
///     Handles the command for adding a NuGet package source to the user's NuGet configuration.
/// </summary>
/// <remarks>
///     This command checks for existing NuGet sources and adds a new one if it doesn't already exist.
///     It attempts to retrieve an API key from environment variables for authentication.
/// </remarks>
internal static class NugetAddCommand
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

        await listSourceProcess.WaitForExitAsync(cancellationToken);
        var listSourceOutput = await listSourceProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        var listSourceError = await listSourceProcess.StandardError.ReadToEndAsync(cancellationToken);

        if (listSourceProcess.ExitCode is not 0)
        {
            await Console.Error.WriteLineAsync("Error: Failed to list NuGet sources.");
            await Console.Error.WriteLineAsync(listSourceOutput);
            await Console.Error.WriteLineAsync(listSourceError);

            return 1;
        }

        if (listSourceOutput.Contains(name, StringComparison.OrdinalIgnoreCase) ||
            listSourceOutput.Contains(url, StringComparison.OrdinalIgnoreCase))
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
                "--username",
                "USERNAME", // Username is often a placeholder when using API keys
                "--password",
                secret,
                "--store-password-in-clear-text", // Required for some feeds, or when API key is treated as password
                feedUrl,
            },
            RedirectStandardError = true,
        })!;

        await addSourceProcess.WaitForExitAsync(cancellationToken);

        var addSourceError = await addSourceProcess.StandardError.ReadToEndAsync(cancellationToken);

        if (addSourceProcess.ExitCode is 0)
        {
            Console.WriteLine($"NuGet feed '{name}' added successfully.");

            return 0;
        }

        await Console.Error.WriteLineAsync($"Error: Failed to add NuGet feed '{name}'.");
        await Console.Error.WriteLineAsync(addSourceError);

        return 1;
    }
}
