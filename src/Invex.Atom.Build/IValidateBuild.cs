namespace Invex.Atom.Build;

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
    /// <remarks>
    ///     Errors: a target consumes an artifact or variable that the named target does not produce, or two parameters
    ///     share the same argument name. Warnings: a target has no description, or has neither tasks nor dependencies.
    /// </remarks>
    /// <exception cref="StepFailedException">Thrown if any validation error is found.</exception>
    Target ValidateBuild =>
        t => t
            .DescribedAs("Checks the atom build for common issues.")
            .Executes(() =>
            {
                var errors = new List<string>();
                var warnings = new List<string>();

                var build = GetService<BuildModel>();

                var targets = build.Targets;
                var targetsByName = targets.ToDictionary(x => x.Name);

                warnings.AddRange(targets
                    .Where(x => x.Description is not { Length: > 0 })
                    .Select(x => $"Target '{x.Name}' has no description."));

                warnings.AddRange(targets
                    .Where(x => x.Tasks.Count is 0 && x.Dependencies.Count is 0)
                    .Select(x => $"Target '{x.Name}' has no tasks and no dependencies."));

                foreach (var target in targets)
                {
                    errors.AddRange(target
                        .ConsumedArtifacts
                        .Where(x => targetsByName.TryGetValue(x.TargetName, out var producer) &&
                                    producer.ProducedArtifacts.All(p => p.ArtifactName != x.ArtifactName))
                        .Select(x =>
                            $"Target '{target.Name}' consumes artifact '{x.ArtifactName}' which target '{x.TargetName}' does not produce."));

                    errors.AddRange(target
                        .ConsumedVariables
                        .Where(x => targetsByName.TryGetValue(x.TargetName, out var producer) &&
                                    !producer.ProducedVariables.Contains(x.VariableName))
                        .Select(x =>
                            $"Target '{target.Name}' consumes variable '{x.VariableName}' which target '{x.TargetName}' does not produce."));
                }

                errors.AddRange(GetService<IBuildDefinition>()
                    .ParamDefinitions
                    .Values
                    .GroupBy(x => x.ArgName, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Count() > 1)
                    .Select(x =>
                        $"Parameters {string.Join(", ", x.Select(p => $"'{p.Name}'"))} share the argument name '{x.Key}'."));

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
