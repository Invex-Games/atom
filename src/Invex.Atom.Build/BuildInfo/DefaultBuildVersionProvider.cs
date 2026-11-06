namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     The default provider for determining the build version, used when no custom implementation is registered.
/// </summary>
/// <remarks>
///     This provider extracts the version from a <c>Directory.Build.props</c> file in the project root.
///     It searches for common version-related MSBuild properties and falls back to "1.0.0" if no version is found.
///     When a monorepo product with a <see cref="Product.Directory" /> is being built, the product folder's
///     <c>Directory.Build.props</c> is read first.
/// </remarks>
/// <param name="fileSystem">The file system service for accessing project files.</param>
/// <param name="productContext">Provides the monorepo product being built, if any.</param>
internal sealed partial class DefaultBuildVersionProvider(IRootedFileSystem fileSystem, IProductContext productContext)
    : IBuildVersionProvider
{
    /// <summary>
    ///     Gets the build version by parsing a <c>Directory.Build.props</c> file.
    /// </summary>
    /// <remarks>
    ///     The version is resolved by searching for the following MSBuild properties in order of precedence:
    ///     <c>InformationalVersion</c>, <c>PackageVersion</c>, <c>Version</c>, a combination of <c>VersionPrefix</c> and
    ///     <c>VersionSuffix</c>, and finally <c>VersionPrefix</c> alone.
    ///     If none of these are found or are invalid, it defaults to <c>1.0.0</c>. When a product with a
    ///     <see cref="Product.Directory" /> is being built, its folder's file is tried before the root's.
    /// </remarks>
    public SemVer Version =>
        (productContext.Current?.Directory is { } productDirectory &&
         !string.IsNullOrWhiteSpace(productDirectory)
            ? TryReadVersion(fileSystem.AtomRootDirectory / productDirectory.Trim() / "Directory.Build.props")
            : null) ??
        TryReadVersion(fileSystem.AtomRootDirectory / "Directory.Build.props") ??
        SemVer.One;

    /// <summary>
    ///     Reads the version from an MSBuild property file.
    /// </summary>
    /// <param name="directoryBuildProps">The file to read.</param>
    /// <returns>The version, or <see langword="null" /> if the file does not exist or sets no valid version.</returns>
    private SemVer? TryReadVersion(RootedPath directoryBuildProps)
    {
        if (!directoryBuildProps.FileExists)
            return null;

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
                            : null;
    }

    /// <summary>
    ///     A regular expression to find and extract version-related tags from MSBuild property files.
    /// </summary>
    [GeneratedRegex("<(Version|VersionPrefix|VersionSuffix|PackageVersion|InformationalVersion)>(?<value>.+)</\\1>")]
    private static partial Regex VersionTagRegex();
}
