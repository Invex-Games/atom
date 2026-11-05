namespace Invex.Atom.Module.Dotnet.Util;

/// <summary>
///     Reads installed tool versions from <c>dotnet tool list</c> output and compares them with requested versions.
/// </summary>
internal static class DotnetToolVersion
{
    /// <summary>
    ///     Finds the version of a tool in the output of <c>dotnet tool list</c>.
    /// </summary>
    /// <param name="toolListOutput">
    ///     The standard output of <c>dotnet tool list</c> (global, local or tool path).
    /// </param>
    /// <param name="packageId">The package ID of the tool, matched case-insensitively against the first column.</param>
    /// <returns>
    ///     The value of the version column of the row whose package ID equals <paramref name="packageId" />, or
    ///     <c>null</c> when no row matches.
    /// </returns>
    /// <remarks>
    ///     The header and the dashed separator line are skipped, and the package ID column is matched exactly, so a
    ///     tool whose ID merely starts with <paramref name="packageId" /> (for example <c>GitVersion.Tool.Extra</c>)
    ///     is not reported.
    /// </remarks>
    internal static string? FindInstalledVersion(string toolListOutput, string packageId)
    {
        var lines = toolListOutput.Split('\n');

        // Rows follow the dashed separator under the header; without a separator every line is a candidate.
        var separatorIndex = Array.FindIndex(lines,
            line => line.Trim() is { Length: > 0 } trimmed && trimmed.All(c => c == '-'));

        foreach (var line in lines.Skip(separatorIndex + 1))
        {
            var columns = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            if (columns.Length >= 2 && string.Equals(columns[0], packageId, StringComparison.OrdinalIgnoreCase))
                return columns[1];
        }

        return null;
    }

    /// <summary>
    ///     Determines whether an installed tool version satisfies a requested version.
    /// </summary>
    /// <param name="installedVersion">The installed version, as reported by <c>dotnet tool list</c>.</param>
    /// <param name="requestedVersion">
    ///     The requested version: <c>null</c> or empty for any version, an exact version such as <c>6.8.2</c>, or a
    ///     floating version with a trailing <c>.*</c> wildcard such as <c>6.*</c> or <c>6.8.*</c> (or <c>*</c> alone).
    /// </param>
    /// <returns>
    ///     <c>true</c> if <paramref name="installedVersion" /> satisfies <paramref name="requestedVersion" />;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         An exact version matches when every numeric component is equal (missing trailing components count as
    ///         zero, so <c>1.2</c> equals <c>1.2.0</c>) and the pre-release labels are equal, ignoring case. Build
    ///         metadata (after <c>+</c>) is ignored.
    ///     </para>
    ///     <para>
    ///         A floating version matches a stable installed version whose leading components equal the components
    ///         before the wildcard, so <c>6.*</c> matches <c>6.8.2</c> but not <c>7.0.0</c> or <c>16.0.0</c>.
    ///         Like NuGet, a floating stable version never matches a pre-release version.
    ///     </para>
    /// </remarks>
    internal static bool IsMatch(string installedVersion, string? requestedVersion)
    {
        if (string.IsNullOrWhiteSpace(requestedVersion))
            return true;

        requestedVersion = requestedVersion.Trim();

        var (installedRelease, installedPreRelease) = Split(installedVersion.Trim());

        if (requestedVersion == "*")
            return installedPreRelease is null;

        if (requestedVersion.EndsWith(".*", StringComparison.Ordinal))
        {
            if (installedPreRelease is not null)
                return false;

            var prefix = requestedVersion[..^2]
                .Split('.');

            if (prefix.Length > installedRelease.Length)
                return false;

            for (var i = 0; i < prefix.Length; i++)
                if (!ComponentEquals(prefix[i], installedRelease[i]))
                    return false;

            return true;
        }

        var (requestedRelease, requestedPreRelease) = Split(requestedVersion);
        var length = Math.Max(installedRelease.Length, requestedRelease.Length);

        for (var i = 0; i < length; i++)
        {
            var installedComponent = i < installedRelease.Length
                ? installedRelease[i]
                : "0";

            var requestedComponent = i < requestedRelease.Length
                ? requestedRelease[i]
                : "0";

            if (!ComponentEquals(installedComponent, requestedComponent))
                return false;
        }

        return string.Equals(installedPreRelease, requestedPreRelease, StringComparison.OrdinalIgnoreCase);
    }

    private static (string[] Release, string? PreRelease) Split(string version)
    {
        var metadataIndex = version.IndexOf('+');

        if (metadataIndex >= 0)
            version = version[..metadataIndex];

        var preReleaseIndex = version.IndexOf('-');

        return preReleaseIndex < 0
            ? (version.Split('.'), null)
            : (version[..preReleaseIndex]
                .Split('.'), version[(preReleaseIndex + 1)..]);
    }

    private static bool ComponentEquals(string left, string right) =>
        int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out var leftNumber) &&
        int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out var rightNumber)
            ? leftNumber == rightNumber
            : string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
