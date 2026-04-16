namespace DecSm.Atom;

/// <summary>
///     Defines a target responsible for validating the Atom build configuration.
/// </summary>
/// <remarks>
///     <para>
///         The primary purpose of <c>IValidateBuild</c> is to enhance the reliability and maintainability
///         of Atom build scripts by catching common configuration mistakes early in the process.
///     </para>
/// </remarks>
[PublicAPI]
public interface IValidateBuild : IReportsHelper
{
    /// <summary>
    ///     Gets the build target that performs validation checks on the Atom build.
    /// </summary>
    /// <example>
    ///     This target is included in <see cref="BuildDefinition" /> and is automatically available if the build
    ///     inherits from that type.
    ///     Otherwise, it can be manually included:
    ///     <code lang="csharp">
    /// public class Build : BuildDefinition
    /// {
    ///     // ... other targets ...
    ///     // The ValidateBuild target from IValidateBuild would be automatically available
    ///     // and could be set as a dependency for other targets.
    ///     Target MyCustomTarget => t => t
    ///         .DependsOn(nameof(IValidateBuild.ValidateBuild)) // ValidateBuild is the property from IValidateBuild
    ///         .Executes(() => { /* Do something after validation */ });
    /// }
    /// </code>
    /// </example>
    /// <example>
    ///     The target can then be run with
    ///     <code lang="bash">
    /// atom ValidateBuild
    /// </code>
    /// </example>
    /// <exception cref="StepFailedException">
    ///     Thrown if the validation process encounters critical errors that prevent the build from proceeding.
    ///     (Currently, this condition is not met as the 'errors' list is not populated).
    /// </exception>
    Target ValidateBuild =>
        t => t
            .DescribedAs("Checks the atom build for common issues.")
            .Executes(() =>
            {
                // ReSharper disable once CollectionNeverUpdated.Local - TODO
                var errors = new List<string>();
                var warnings = new List<string>();

                var build = GetService<BuildModel>();

                var targets = build.Targets;

                warnings.AddRange(targets
                    .Where(x => x.Description is not { Length: > 0 })
                    .Select(x => $"Target '{x.Name}' has no description."));

                if (warnings.Count > 0)
                    AddReportData(new ListReportData(warnings)
                    {
                        Title = "Warnings",
                    });

                // ReSharper disable once InvertIf
                if (errors.Count > 0)
                {
                    AddReportData(new ListReportData(errors)
                    {
                        Title = "Errors",
                    });

                    throw new StepFailedException("Validation failed.");
                }
            });
}
