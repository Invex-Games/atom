namespace Invex.Atom.Build;

/// <summary>
///     Provides the <see cref="SetupBuildInfo" /> target, which initializes core build information
///     (name, ID, version, and timestamp) as build variables for consumption by other targets.
/// </summary>
/// <seealso cref="SetupBuildInfo" />
[PublicAPI]
public interface ISetupBuildInfo : IBuildInfo, IVariablesHelper, IReportsHelper
{
    /// <summary>
    ///     Gets the build target responsible for setting up the build ID, version, and timestamp.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <c>SetupBuildInfo</c> target is a foundational step in the Atom build pipeline. It performs several key
    ///         actions:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     Retrieves the build name (from <see cref="IBuildInfo.BuildName" />) and writes it as a build
    ///                     variable named
    ///                     "BuildName".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Retrieves the build ID (from <see cref="IBuildInfo.BuildId" />) and writes it as a build variable
    ///                     named "BuildId".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Retrieves the build version (from <see cref="IBuildInfo.BuildVersion" />) and writes it as a build
    ///                     variable
    ///                     named
    ///                     "BuildVersion".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Retrieves the build timestamp (from <see cref="IBuildInfo.BuildTimestamp" />), converts it to a
    ///                     string,
    ///                     and
    ///                     writes it
    ///                     as a build variable named "BuildTimestamp".
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Adds a "Run Information" section to the build report, displaying the build name, version,
    ///                     ID, and timestamp.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Logs the build ID, version, and timestamp using the configured logger for
    ///                     <c>ISetupBuildInfo</c>.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         This target is marked as hidden (<c>IsHidden()</c>), meaning it's not typically intended for direct invocation
    ///         by users but is
    ///         expected to run as a dependency for other targets or as part of an initial setup sequence.
    ///     </para>
    ///     <para>
    ///         It produces the following build variables, which can be consumed by subsequent targets in the build process:
    ///         <list type="bullet">
    ///             <item>
    ///                 <term>
    ///                     <c>BuildName</c>
    ///                 </term>
    ///                 <description>The name of the build.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildId</c>
    ///                 </term>
    ///                 <description>The unique identifier for the build execution.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildVersion</c>
    ///                 </term>
    ///                 <description>The version of the software being built.</description>
    ///             </item>
    ///             <item>
    ///                 <term>
    ///                     <c>BuildTimestamp</c>
    ///                 </term>
    ///                 <description>The timestamp of the build (as a string).</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Successful execution relies on the correct registration and functioning of <see cref="IBuildIdProvider" />,
    ///         <see cref="IBuildVersionProvider" />, and <see cref="IBuildTimestampProvider" /> services within the Atom
    ///         framework.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <para>
    ///         While <c>SetupBuildInfo</c> is often implicitly part of a default build definition, here's how a custom
    ///         target can depend on it and consume the variables it produces. Variables are consumed via the
    ///         corresponding build parameters (declared with <see cref="TargetDefinition.ConsumesVariable" /> and read
    ///         with <c>GetParam</c>).
    ///     </para>
    ///     <code lang="csharp">
    /// // In a target interface within your Atom build project
    /// public interface IPackage : ISetupBuildInfo
    /// {
    ///     Target Package => t => t
    ///         .DependsOn(nameof(ISetupBuildInfo.SetupBuildInfo)) // Ensures build info is set up
    ///         .ConsumesVariable(nameof(ISetupBuildInfo.SetupBuildInfo), nameof(BuildVersion))
    ///         .Executes(() =>
    ///         {
    ///             // BuildVersion is resolved from the variable written by SetupBuildInfo
    ///             Logger.LogInformation("Packaging {Name} version {Version}", BuildName, BuildVersion);
    ///             // ... further packaging logic ...
    ///         });
    /// }
    /// // To run this build (assuming Atom CLI):
    /// // atom Package
    /// </code>
    /// </example>
    /// <returns>
    ///     A <see cref="Target" /> configured to initialize build information variables and report data.
    /// </returns>
    /// <seealso cref="IBuildInfo.BuildName" />
    /// <seealso cref="IVariablesHelper.WriteVariable(string, string, CancellationToken)" />
    /// <seealso cref="IReportsHelper.AddReportData(IReportData)" />
    Target SetupBuildInfo =>
        t => t
            .DescribedAs("Sets up the build ID, version, and timestamp")
            .IsHidden()
            .ProducesVariable(nameof(BuildName))
            .ProducesVariable(nameof(BuildId))
            .ProducesVariable(nameof(BuildVersion))
            .ProducesVariable(nameof(BuildTimestamp))
            .Executes(async cancellationToken =>
            {
                var atomBuildName = BuildName;
                await WriteVariable(nameof(BuildName), atomBuildName, cancellationToken);

                var buildId = BuildId;
                await WriteVariable(nameof(BuildId), buildId, cancellationToken);

                var buildVersion = BuildVersion;
                await WriteVariable(nameof(BuildVersion), buildVersion, cancellationToken);

                var buildTimestamp = BuildTimestamp;
                await WriteVariable(nameof(BuildTimestamp), buildTimestamp.ToString(), cancellationToken);

                var reportedBuildId = buildId == buildVersion
                    ? buildId
                    : $"{buildVersion} - {buildId} [{buildTimestamp}]";

                AddReportData(new TextReportData($"{BuildName} | {reportedBuildId}")
                {
                    Title = "Run Information",
                    BeforeStandardData = true,
                });

                var logger = Services.GetRequiredService<ILogger<ISetupBuildInfo>>();

                logger.LogInformation("Build ID: {BuildId}", buildId);
                logger.LogInformation("Build Version: {BuildVersion}", buildVersion);
                logger.LogInformation("Build Timestamp: {BuildTimestamp}", buildTimestamp);
            });
}
