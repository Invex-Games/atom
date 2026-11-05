namespace Invex.Atom.Module.GitVersion.Tests;

/// <summary>
///     Answers the git, <c>dotnet tool</c> and <c>dotnet gitversion</c> calls made by the GitVersion module and
///     records every call as <c>"&lt;name&gt; &lt;args&gt;"</c>.
/// </summary>
internal sealed class FakeGitVersionProcessRunner : IProcessRunner
{
    internal const string GitVersionJson =
        """{"FullSemVer":"1.2.3-beta.4","Major":1,"Minor":2,"Patch":3,"PreReleaseTag":"beta.4"}""";

    public string GitHash { get; set; } = "0123456789abcdef";

    /// <summary>
    ///     The GitVersion.Tool version reported by <c>dotnet tool list</c>, or <c>null</c> when it is not installed.
    ///     A <c>dotnet tool update</c> call sets it to <see cref="UpdatedToolVersion" />.
    /// </summary>
    public string? InstalledToolVersion { get; set; } = "6.8.2";

    public string UpdatedToolVersion { get; set; } = "6.8.2";

    public List<string> Calls { get; } = [];

    public IEnumerable<string> GitVersionCalls =>
        Calls.Where(x => x.StartsWith("dotnet gitversion", StringComparison.Ordinal));

    public ProcessRunResult Run(ProcessRunOptions options)
    {
        Calls.Add($"{options.Name} {options.Args}");

        return (options.Name, options.Args) switch
        {
            ("git", "rev-parse HEAD") => new(options, 0, $"{GitHash}\n", string.Empty),
            ("dotnet", var args) when args.StartsWith("tool list", StringComparison.Ordinal) =>
                InstalledToolVersion is null
                    ? new(options, 1, "Package Id      Version      Commands\n", string.Empty)
                    : new(options, 0, ToolListOutput(InstalledToolVersion), string.Empty),
            ("dotnet", var args) when args.StartsWith("tool update", StringComparison.Ordinal) => Update(options),
            ("dotnet", var args) when args.StartsWith("gitversion", StringComparison.Ordinal) =>
                new(options, 0, GitVersionJson, string.Empty),
            _ => throw new InvalidOperationException($"Unexpected process call: {options.Name} {options.Args}"),
        };
    }

    public Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default) =>
        Task.FromResult(Run(options));

    private static string ToolListOutput(string version) =>
        $"""
         Package Id           Version      Commands
         ---------------------------------------------------
         gitversion.tool      {version}        dotnet-gitversion

         """;

    private ProcessRunResult Update(ProcessRunOptions options)
    {
        InstalledToolVersion = UpdatedToolVersion;

        return new(options, 0, string.Empty, string.Empty);
    }
}
