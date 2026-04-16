namespace DecSm.Atom.BuildInfo;

/// <summary>
///     The default provider for determining the build version, used when no custom implementation is registered.
/// </summary>
/// <remarks>
///     This provider extracts the version from a <c>Directory.Build.props</c> file in the project root.
///     It searches for common version-related MSBuild properties and falls back to "1.0.0" if no version is found.
/// </remarks>
/// <param name="fileSystem">The file system service for accessing project files.</param>
internal sealed partial class DefaultBuildVersionProvider(IAtomFileSystem fileSystem) : IBuildVersionProvider
{
    /// <summary>
    ///     Gets the build version by parsing a <c>Directory.Build.props</c> file.
    /// </summary>
    /// <remarks>
    ///     The version is resolved by searching for the following MSBuild properties in order of precedence:
    ///     <c>InformationalVersion</c>, <c>PackageVersion</c>, <c>Version</c>, a combination of <c>VersionPrefix</c> and
    ///     <c>VersionSuffix</c>, and finally <c>VersionPrefix</c> alone.
    ///     If none of these are found or are invalid, it defaults to <c>1.0.0</c>.
    /// </remarks>
    public SemVer Version
    {
        get
        {
            var directoryBuildProps = fileSystem.AtomRootDirectory / "Directory.Build.props";

            if (!directoryBuildProps.FileExists)
                return SemVer.One;

            var contents = fileSystem.File.ReadAllText(directoryBuildProps);

            var matches = VersionTagRegex()
                .Matches(contents);

            string? version = null;
            string? versionPrefix = null;
            string? versionSuffix = null;
            string? packageVersion = null;
            string? informationalVersion = null;

            foreach (Match match in matches)
            {
                var value = match.Groups["value"].Value;

                if (string.IsNullOrWhiteSpace(value) || value.Contains("$("))
                    continue;

                switch (match.Groups[1].Value)
                {
                    case "Version":
                        version = value;

                        break;
                    case "VersionPrefix":
                        versionPrefix = value;

                        break;
                    case "VersionSuffix":
                        versionSuffix = value;

                        break;
                    case "PackageVersion":
                        packageVersion = value;

                        break;
                    case "InformationalVersion":
                        informationalVersion = value;

                        break;
                }
            }

            return SemVer.TryParse(informationalVersion, out var informationalSemVer)
                ? informationalSemVer
                : SemVer.TryParse(packageVersion, out var packageSemVer)
                    ? packageSemVer
                    : SemVer.TryParse(version, out var versionSemVer)
                        ? versionSemVer
                        : SemVer.TryParse($"{versionPrefix}-{versionSuffix}", out var combinedSemVer)
                            ? combinedSemVer
                            : SemVer.TryParse(versionPrefix, out var prefixSemVer)
                                ? prefixSemVer
                                : SemVer.One;
        }
    }

    /// <summary>
    ///     A regular expression to find and extract version-related tags from MSBuild property files.
    /// </summary>
    [GeneratedRegex("<(Version|VersionPrefix|VersionSuffix|PackageVersion|InformationalVersion)>(?<value>.+)</\\1>")]
    private static partial Regex VersionTagRegex();
}
