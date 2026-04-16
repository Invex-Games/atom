namespace DecSm.Atom.Module.Dotnet.Helpers;

/// <summary>
///     Provides helper methods for running .NET tests and staging their results and code coverage reports.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IDotnetCliHelper" /> for executing `dotnet test` commands,
///     <see cref="IBuildInfo" /> for build version information, <see cref="IDotnetToolInstallHelper" />
///     for installing necessary tools, and <see cref="IReportsHelper" /> for generating build reports.
/// </remarks>
[PublicAPI]
public partial interface IDotnetTestHelper : IDotnetCliHelper, IBuildInfo, IDotnetToolInstallHelper, IReportsHelper
{
    /// <summary>
    ///     Runs tests for a .NET project by its name, then stages the test results and optionally code coverage reports.
    /// </summary>
    /// <param name="projectName">The name of the project to test (e.g., "MyProject.Tests").</param>
    /// <param name="options">Optional. Configuration options for the testing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}" /> that returns the exit code of the `dotnet test` command.</returns>
    /// <exception cref="StepFailedException">Thrown if the project file cannot be located.</exception>
    /// <remarks>
    ///     This method first locates the project file based on its name, then calls
    ///     <see cref="DotnetTestAndStage(RootedPath, DotnetTestAndStageOptions?, CancellationToken)" />
    ///     with the resolved path.
    /// </remarks>
    [PublicAPI]
    Task<int> DotnetTestAndStage(
        string projectName,
        DotnetTestAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetTestAndStage(projectPath, options, cancellationToken);
    }

    /// <summary>
    ///     Runs tests for a .NET project specified by its path, then stages the test results and optionally code coverage
    ///     reports.
    /// </summary>
    /// <param name="projectPath">The full path to the project file (e.g., "tests/MyProject.Tests/MyProject.Tests.csproj").</param>
    /// <param name="options">Optional. Configuration options for the testing and staging process.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}" /> that returns the exit code of the `dotnet test` command.</returns>
    /// <remarks>
    ///     <para>
    ///         This method performs the following steps:
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                     Configures `dotnet test` options, including TRX and HTML loggers, and optionally code
    ///                     coverage.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Cleans up existing test result directories.</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Applies version transformations to project files if
    ///                     <see cref="DotnetTestAndStageOptions.SetVersionsFromProviders" /> is <c>true</c>.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Executes the `dotnet test` command using
    ///                     <see cref="IDotnetCli.Test(RootedPath, TestOptions?, ProcessRunOptions?, CancellationToken)" />.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Copies the generated HTML test report to the Atom publish directory.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Generates a detailed test report using <see cref="GenerateTestReport" />.</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     If <see cref="DotnetTestAndStageOptions.IncludeCoverage" /> is <c>true</c>, installs
    ///                     `dotnet-reportgenerator-globaltool` and generates code coverage reports.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Generates a code coverage report using <see cref="GenerateCoverageReport" />.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         The <see cref="IBuildInfo.BuildVersion" /> is used for version transformation.
    ///     </para>
    /// </remarks>
    [PublicAPI]
    async Task<int> DotnetTestAndStage(
        RootedPath projectPath,
        DotnetTestAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectName = projectPath.FileNameWithoutExtension;

        options ??= new();
        var testOptions = options.TestOptions ?? new();

        testOptions = testOptions with
        {
            Output = testOptions.Output is { Length: > 0 }
                ? testOptions.Output
                : FileSystem.AtomRootDirectory / projectName / "TestResults",
            Logger = testOptions.Logger ??
            [
                $"\"trx;LogFileName={projectName}.trx\"", $"\"html;LogFileName={projectName}.html\"",
            ],
            Collect = testOptions.Collect ??
                      (options.IncludeCoverage
                          ? ["\"XPlat Code Coverage\""]
                          : null),
        };

        var testOutputDirectory = FileSystem.CreateRootedPath(testOptions.Output);

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        var testResultsPublishDirectory = publishDirectory / "test-results";

        if (FileSystem.Directory.Exists(testResultsPublishDirectory))
            FileSystem.Directory.Delete(testResultsPublishDirectory, true);

        FileSystem.Directory.CreateDirectory(testResultsPublishDirectory);

        var coverageResultsPublishDirectory = publishDirectory / "coverage-results";

        if (options.IncludeCoverage && FileSystem.Directory.Exists(coverageResultsPublishDirectory))
            FileSystem.Directory.Delete(coverageResultsPublishDirectory, true);

        if (options.IncludeCoverage)
            FileSystem.Directory.CreateDirectory(coverageResultsPublishDirectory);

        Logger.LogInformation("Running unit tests for project {Project}", projectName);

        if (FileSystem.Directory.Exists(testOutputDirectory))
        {
            Logger.LogDebug("Deleting existing test output directory {TestOutputDirectory}", testOutputDirectory);
            FileSystem.Directory.Delete(testOutputDirectory, true);
        }

        Logger.LogDebug(
            "Transforming project properties: SetVersionsFromProviders={SetVersionsFromProviders}, CustomPropertiesTransform={CustomPropertiesTransform}",
            options.SetVersionsFromProviders,
            options.CustomPropertiesTransform is not null
                ? "true"
                : "false");

        await using var transformFilesScope =
            (options.SetVersionsFromProviders, options.CustomPropertiesTransform) switch
            {
                (true, not null) => await TransformProjectVersionScope
                    .CreateAsync(DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                        BuildVersion,
                        cancellationToken)
                    .AddAsync(options.CustomPropertiesTransform),

                (true, null) => await TransformProjectVersionScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    BuildVersion,
                    cancellationToken),

                (false, not null) => await TransformMultiFileScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    options.CustomPropertiesTransform!,
                    cancellationToken),

                _ => null,
            };

        var result = await DotnetCli.Test(projectPath,
            testOptions,
            new("", "")
            {
                TransformError = s => s.Contains(", is an invalid character")
                    ? null
                    : s,
            },
            cancellationToken);

        // Copy html file to publish directory
        FileSystem.File.Copy(testOutputDirectory / $"{projectName}.html",
            testResultsPublishDirectory / $"{projectName}.html");

        GenerateTestReport(projectName,
            testOptions.Configuration,
            testOptions.Framework,
            testOutputDirectory / $"{projectName}.trx");

        if (!options.IncludeCoverage)
            return result.ExitCode;

        await DotnetCli.ToolExecute("dotnet-reportgenerator-globaltool",
            [
                $"-reports:{testOutputDirectory / "**" / "coverage.cobertura.xml"}",
                $"-targetdir:{coverageResultsPublishDirectory}",
                "-reporttypes:HtmlInline;JsonSummary",
                "-sourcedirs:" + FileSystem.AtomRootDirectory,
            ],
            new()
            {
                Yes = true,
            },
            new("", "")
            {
                AllowFailedResult = true,
            },
            cancellationToken);

        GenerateCoverageReport(projectName,
            testOptions.Configuration,
            testOptions.Framework,
            coverageResultsPublishDirectory / "Summary.json",
            false);

        Logger.LogInformation("Ran unit tests for Atom project {AtomProjectName}", projectName);

        return result.ExitCode;
    }

    /// <summary>
    ///     Generates a test report from a TRX file and adds it to the build report.
    /// </summary>
    /// <param name="projectName">The name of the project that was tested.</param>
    /// <param name="configuration">The build configuration (e.g., "Release", "Debug").</param>
    /// <param name="framework">The target framework (e.g., "net8.0").</param>
    /// <param name="trxFile">The path to the TRX test results file.</param>
    /// <param name="includeTitle">Whether to include a title in the generated report data.</param>
    [UnconditionalSuppressMessage("Trimming",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Deserialized type `TestRun` is manually preserved via DynamicallyAccessedMembers.")]
    [PublicAPI]
    void GenerateTestReport(
        string projectName,
        string? configuration,
        string? framework,
        string trxFile,
        bool includeTitle = true)
    {
        var serializer = new XmlSerializer(typeof(TestRun));

        using var reader = new StreamReader(trxFile);
        var testRun = (TestRun)serializer.Deserialize(reader)!;

        AddReportData(new TableReportData([
            ["⌛ Run duration", PrintDuration(testRun.Times.Finish - testRun.Times.Start)],
            ["🔢 Total tests", testRun.ResultSummary.Counters.Total.ToString()],
            ["✅ Passed tests", testRun.ResultSummary.Counters.Passed.ToString()],
            ["❌ Failed tests", testRun.ResultSummary.Counters.Failed.ToString()],
            ["⏩ Skipped tests", testRun.ResultSummary.Counters.NotExecuted.ToString()],
        ])
        {
            Title = includeTitle
                ? $"Test run summary | {projectName} | {configuration ?? "Release"} | {framework ?? RuntimeInformation.FrameworkDescription}"
                : null,
            ColumnAlignments = [ColumnAlignment.Left, ColumnAlignment.Right],
        });

        var failedCaseRows = new List<string>();

        var failedTests = testRun
            .Results
            .Where(x => x.Outcome == "Failed")
            .Select(x => (Result: x, Definition: testRun.TestDefinitions.First(y => y.Id == x.TestId)));

        var failedTestsByClass = failedTests.GroupBy(x => x.Definition.TestMethod);

        foreach (var classWithTests in failedTestsByClass)
        {
            failedCaseRows.Add($"- {classWithTests.Key.ClassName}");

            failedCaseRows.AddRange(classWithTests.Select(test =>
                $"  - {test.Definition.Name} | {PrintDuration(test.Result.EndTime - test.Result.StartTime)} | {test.Result.Output.ErrorInfo.Message}"));
        }

        if (failedCaseRows.Count > 0)
            AddReportData(new ListReportData(failedCaseRows)
            {
                Title = "❌ Failed cases",
                Prefix = string.Empty,
            });
    }

    /// <summary>
    ///     Generates a code coverage report from a JSON summary file and adds it to the build report.
    /// </summary>
    /// <param name="projectName">The name of the project for which coverage was generated.</param>
    /// <param name="configuration">The build configuration (e.g., "Release", "Debug").</param>
    /// <param name="framework">The target framework (e.g., "net8.0").</param>
    /// <param name="coverageJsonFile">The path to the JSON code coverage summary file.</param>
    /// <param name="includeTitle">Whether to include a title in the generated report data.</param>
    [PublicAPI]
    void GenerateCoverageReport(
        string projectName,
        string? configuration,
        string? framework,
        string coverageJsonFile,
        bool includeTitle = true)
    {
        var coverageJson = FileSystem.File.ReadAllText(coverageJsonFile);

        var summary = JsonSerializer.Deserialize(coverageJson, CoverageModelContext.Default.CoverageModel)!.Summary;

        AddReportData(new TableReportData([
            ["Lines", summary.TotalLines.ToString()],
            ["Covered", summary.CoveredLines.ToString()],
            ["Uncovered", summary.UncoveredLines.ToString()],
            ["Coverable", summary.CoverableLines.ToString()],
            ["Line coverage", (summary.LineCoverage / 100).ToString("P")],
            ["Branch coverage", (summary.BranchCoverage / 100).ToString("P")],
        ])
        {
            Title = includeTitle
                ? $"Test run summary | {projectName} | {configuration ?? "Release"} | {framework ?? RuntimeInformation.FrameworkDescription}"
                : null,
            ColumnAlignments = [ColumnAlignment.Left, ColumnAlignment.Right],
        });
    }

    /// <summary>
    ///     Formats a <see cref="TimeSpan" /> into a human-readable string.
    /// </summary>
    /// <param name="duration">The duration to format.</param>
    /// <returns>A string representation of the duration (e.g., "1.23s", "5m 30s", "1h 15m 0s").</returns>
    private static string PrintDuration(TimeSpan duration) =>
        duration.TotalSeconds switch
        {
            < 60 => $"{duration.TotalSeconds:0.##}s",
            < 3600 => $"{duration.Minutes}m {duration.TotalSeconds % 60:0.##}s",
            _ => $"{duration.Hours}h {duration.Minutes}m {duration.Seconds}s",
        };

    /// <summary>
    ///     Provides a <see cref="JsonSerializerContext" /> for source generation of <see cref="CoverageModel" />.
    /// </summary>
    [JsonSerializable(typeof(CoverageModel))]
    internal partial class CoverageModelContext : JsonSerializerContext;
}

/// <summary>
///     Represents options for the
///     <see cref="IDotnetTestHelper.DotnetTestAndStage(RootedPath, DotnetTestAndStageOptions?, CancellationToken)" />
///     operation.
/// </summary>
[PublicAPI]
public sealed record DotnetTestAndStageOptions
{
    /// <summary>
    ///     Gets or sets the specific options to pass to the `dotnet test` command.
    /// </summary>
    public TestOptions? TestOptions { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to automatically set project versions
    ///     from the build version providers (<see cref="IBuildInfo.BuildVersion" />).
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, the module will attempt to inject the build version into project files
    ///     before running tests. Defaults to <c>true</c>.
    /// </remarks>
    public bool SetVersionsFromProviders { get; init; } = true;

    /// <summary>
    ///     Gets or sets a custom transformation function to apply to project property files
    ///     before running tests.
    /// </summary>
    /// <remarks>
    ///     This function can be used to modify the content of `.csproj` or `.props` files
    ///     (e.g., to inject custom properties or modify existing ones) during the testing process.
    /// </remarks>
    public Func<string, string>? CustomPropertiesTransform { get; init; }

    /// <summary>
    ///     Gets or sets a value indicating whether to include code coverage generation and reporting.
    /// </summary>
    /// <remarks>
    ///     If <c>true</c>, the `dotnet test` command will be configured to collect code coverage,
    ///     and a code coverage report will be generated and added to the build report.
    ///     Defaults to <c>true</c>.
    /// </remarks>
    public bool IncludeCoverage { get; init; } = true;
}
