namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Computes the key that identifies the inputs a cached GitVersion result was generated from.
/// </summary>
/// <remarks>
///     GitVersion's output depends on more than the current commit, so the key combines every input that can change
///     the result without moving <c>HEAD</c>: the commit hash, the checked-out branch name (renaming or switching
///     branches changes the pre-release label), every tag in the repository and the object it refers to (GitVersion uses
///     tags on ancestor commits as version sources), the contents of the repository's
///     GitVersion configuration file, the pinned <see cref="GitVersionTool.Version" />, and the CI environment
///     variables GitVersion reads the branch from. The combined value is hashed so it always fits on a single line.
/// </remarks>
internal static class GitVersionCacheKey
{
    private static readonly string[] ConfigFileNames =
    [
        "GitVersion.yml", "GitVersion.yaml", ".GitVersion.yml", ".GitVersion.yaml",
    ];

    private static readonly string[] BranchEnvironmentVariables =
    [
        "GITHUB_REF", "GITHUB_HEAD_REF", "BUILD_SOURCEBRANCH", "SYSTEM_PULLREQUEST_SOURCEBRANCH",
    ];

    /// <summary>
    ///     Computes the cache key for the current repository state.
    /// </summary>
    /// <param name="processRunner">The process runner used to query Git.</param>
    /// <param name="fileSystem">The file system used to read the GitVersion configuration file.</param>
    /// <returns>A lowercase hexadecimal SHA-256 hash of the GitVersion inputs.</returns>
    internal static string Compute(IProcessRunner processRunner, IRootedFileSystem fileSystem)
    {
        var commit = RunGit(processRunner, "rev-parse HEAD");

        // Prints 'HEAD' when detached, which is still a stable part of the key.
        var branch = RunGit(processRunner, "rev-parse --abbrev-ref HEAD");

        // GitVersion also takes version sources from tags on ancestor commits, so every tag and the object it refers to
        // is part of the key: adding, moving or deleting any tag invalidates the cache. Prints nothing when there are
        // no tags.
        var tags = RunGit(processRunner, "for-each-ref refs/tags --format=%(refname)=%(objectname)")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Order(StringComparer.Ordinal);

        var builder = new StringBuilder()
            .Append("commit=")
            .Append(commit)
            .Append('\n')
            .Append("branch=")
            .Append(branch)
            .Append('\n')
            .Append("tags=")
            .AppendJoin(',', tags)
            .Append('\n')
            .Append("tool=")
            .Append(GitVersionTool.Version)
            .Append('\n');

        foreach (var configFileName in ConfigFileNames)
        {
            var configFile = fileSystem.AtomRootDirectory / configFileName;

            if (!fileSystem.File.Exists(configFile))
                continue;

            builder
                .Append("config:")
                .Append(configFileName)
                .Append('=')
                .Append(Convert.ToHexStringLower(SHA256.HashData(fileSystem.File.ReadAllBytes(configFile))))
                .Append('\n');
        }

        foreach (var variable in BranchEnvironmentVariables)
            builder
                .Append("env:")
                .Append(variable)
                .Append('=')
                .Append(Environment.GetEnvironmentVariable(variable))
                .Append('\n');

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static string RunGit(IProcessRunner processRunner, string args) =>
        processRunner
            .Run(new("git", args)
            {
                InvocationLogLevel = LogLevel.Debug,
            })
            .Output
            .Trim();
}
