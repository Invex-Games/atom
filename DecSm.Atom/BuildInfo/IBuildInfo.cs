namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Provides centralized access to essential build metadata, such as version, name, and identifiers.
/// </summary>
/// <remarks>
///     This interface defines parameters that supply consistent build information throughout the execution lifecycle.
///     For values that change over time (e.g., <see cref="BuildTimestamp" />), it is recommended to consume them from
///     <see cref="ISetupBuildInfo" />. This ensures that a stable value is captured at the start of the workflow and
///     reused across all targets, providing consistency.
/// </remarks>
[PublicAPI]
public interface IBuildInfo : IBuildAccessor
{
    /// <summary>
    ///     Gets the human-readable name of the build, derived from the solution or root directory name.
    /// </summary>
    /// <remarks>
    ///     This value can be overridden using the <c>--build-name</c> parameter. If not provided, it is automatically
    ///     determined by searching for a <c>.slnx</c> or <c>.sln</c> file in the root directory and using its name.
    ///     If no solution file is found, it defaults to the name of the root directory.
    /// </remarks>
    [ParamDefinition("build-name", "The name of the build.")]
    string BuildName => GetParam(() => BuildName, DefaultBuildName);

    /// <summary>
    ///     Gets the unique identifier for the current build run.
    /// </summary>
    /// <remarks>
    ///     This value can be overridden using the <c>--build-id</c> parameter. If not provided, it defaults to the
    ///     value from the registered <see cref="IBuildIdProvider" />.
    /// </remarks>
    [ParamDefinition("build-id", "The unique identifier for the build run.")]
    string BuildId => GetParam(() => BuildId, DefaultBuildId);

    /// <summary>
    ///     Gets the semantic version of the build.
    /// </summary>
    /// <remarks>
    ///     This value can be overridden using the <c>--build-version</c> parameter. If not provided, it defaults to the
    ///     version from the registered <see cref="IBuildVersionProvider" />. The value is automatically parsed into a
    ///     <see cref="SemVer" /> object.
    /// </remarks>
    [ParamDefinition("build-version", "The semantic version of the build.")]
    SemVer BuildVersion =>
        GetParam(() => BuildVersion,
            DefaultBuildVersion,
            s => s is not null
                ? SemVer.Parse(s)
                : throw new ArgumentException("Invalid BuildVersion"));

    /// <summary>
    ///     Gets the build timestamp as a Unix epoch timestamp (seconds).
    /// </summary>
    /// <remarks>
    ///     This value can be overridden using the <c>--build-timestamp</c> parameter. If not provided, it defaults to the
    ///     timestamp from the registered <see cref="IBuildTimestampProvider" />.
    ///     <para>
    ///         <b>Warning:</b> This value may be inconsistent across different process instances. For a stable
    ///         timestamp, consume this parameter from <see cref="ISetupBuildInfo" /> so that a consistent value is used
    ///         across all targets.
    ///     </para>
    /// </remarks>
    [ParamDefinition("build-timestamp", "The build timestamp in Unix epoch seconds.")]
    long BuildTimestamp => GetParam(() => BuildTimestamp, DefaultBuildTimestamp);

    /// <summary>
    ///     Gets an optional identifier for a specific variation of a build, often used in CI/CD matrix jobs.
    /// </summary>
    /// <remarks>
    ///     This value can be configured using the <c>--build-slice</c> parameter. It is useful for distinguishing
    ///     between parallel build jobs (e.g., different target frameworks or operating systems). It is <c>null</c> if not
    ///     specified.
    /// </remarks>
    [ParamDefinition("build-slice", "An identifier for a build variation, used in matrix jobs.")]
    string? BuildSlice => GetParam(() => BuildSlice);

    /// <summary>
    ///     Gets the default build name from the solution file or root directory.
    /// </summary>
    private string DefaultBuildName
    {
        get
        {
            var solutionFile = FileSystem
                                   .Directory
                                   .GetFiles(FileSystem.AtomRootDirectory, "*.slnx", SearchOption.TopDirectoryOnly)
                                   .FirstOrDefault() ??
                               FileSystem
                                   .Directory
                                   .GetFiles(FileSystem.AtomRootDirectory, "*.sln", SearchOption.TopDirectoryOnly)
                                   .FirstOrDefault();

            Logger.LogDebug("Determined solution file: {SolutionFile}", solutionFile);

            return solutionFile is not null
                ? new RootedPath(FileSystem, solutionFile).FileNameWithoutExtension
                : FileSystem
                      .AtomRootDirectory
                      .DirectoryName
                      ?.Split(FileSystem.Path.DirectorySeparatorChar, FileSystem.Path.AltDirectorySeparatorChar)[^1] ??
                  "Unknown";
        }
    }

    /// <summary>
    ///     Gets the default build ID from the registered <see cref="IBuildIdProvider" />.
    /// </summary>
    private string DefaultBuildId =>
        GetService<IBuildIdProvider>()
            .BuildId;

    /// <summary>
    ///     Gets the default build version from the registered <see cref="IBuildVersionProvider" />.
    /// </summary>
    private SemVer DefaultBuildVersion =>
        GetService<IBuildVersionProvider>()
            .Version;

    /// <summary>
    ///     Gets the default build timestamp from the registered <see cref="IBuildTimestampProvider" />.
    /// </summary>
    private long DefaultBuildTimestamp =>
        GetService<IBuildTimestampProvider>()
            .Timestamp;
}
