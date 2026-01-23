namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Provides extension methods for <see cref="TargetDefinition" /> to simplify adding multiple dependencies.
/// </summary>
[PublicAPI]
public static class TargetDefinitionExtensions
{
    extension(TargetDefinition targetDefinition)
    {
        /// <summary>
        ///     Adds dependencies on multiple targets.
        /// </summary>
        [PublicAPI]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public TargetDefinition DependsOn(
            Target target1,
            Target target2,
            [CallerArgumentExpression("target1")] string? target1Name = null,
            [CallerArgumentExpression("target2")] string? target2Name = null)
        {
            if (string.IsNullOrWhiteSpace(target1Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target1));

            if (string.IsNullOrWhiteSpace(target2Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target2));

            return targetDefinition
                .DependsOn(target1Name)
                .DependsOn(target2Name);
        }

        /// <summary>
        ///     Adds dependencies on multiple targets.
        /// </summary>
        [PublicAPI]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public TargetDefinition DependsOn(
            Target target1,
            Target target2,
            Target target3,
            [CallerArgumentExpression("target1")] string? target1Name = null,
            [CallerArgumentExpression("target2")] string? target2Name = null,
            [CallerArgumentExpression("target3")] string? target3Name = null)
        {
            if (string.IsNullOrWhiteSpace(target1Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target1));

            if (string.IsNullOrWhiteSpace(target2Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target2));

            if (string.IsNullOrWhiteSpace(target3Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target3));

            return targetDefinition
                .DependsOn(target1Name)
                .DependsOn(target2Name)
                .DependsOn(target3Name);
        }

        /// <summary>
        ///     Adds dependencies on multiple targets.
        /// </summary>
        [PublicAPI]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public TargetDefinition DependsOn(
            Target target1,
            Target target2,
            Target target3,
            Target target4,
            [CallerArgumentExpression("target1")] string? target1Name = null,
            [CallerArgumentExpression("target2")] string? target2Name = null,
            [CallerArgumentExpression("target3")] string? target3Name = null,
            [CallerArgumentExpression("target4")] string? target4Name = null)
        {
            if (string.IsNullOrWhiteSpace(target1Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target1));

            if (string.IsNullOrWhiteSpace(target2Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target2));

            if (string.IsNullOrWhiteSpace(target3Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target3));

            if (string.IsNullOrWhiteSpace(target4Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target4));

            return targetDefinition
                .DependsOn(target1Name)
                .DependsOn(target2Name)
                .DependsOn(target3Name)
                .DependsOn(target4Name);
        }

        /// <summary>
        ///     Adds dependencies on multiple targets.
        /// </summary>
        [PublicAPI]
        [SuppressMessage("ReSharper", "LocalizableElement")]
        public TargetDefinition DependsOn(
            Target target1,
            Target target2,
            Target target3,
            Target target4,
            Target target5,
            [CallerArgumentExpression("target1")] string? target1Name = null,
            [CallerArgumentExpression("target2")] string? target2Name = null,
            [CallerArgumentExpression("target3")] string? target3Name = null,
            [CallerArgumentExpression("target4")] string? target4Name = null,
            [CallerArgumentExpression("target5")] string? target5Name = null)
        {
            if (string.IsNullOrWhiteSpace(target1Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target1));

            if (string.IsNullOrWhiteSpace(target2Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target2));

            if (string.IsNullOrWhiteSpace(target3Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target3));

            if (string.IsNullOrWhiteSpace(target4Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target4));

            if (string.IsNullOrWhiteSpace(target5Name))
                throw new ArgumentException(
                    "Unable to infer target name from argument expression. Please use DependsOn(\"TargetName\") overload.",
                    nameof(target5));

            return targetDefinition
                .DependsOn(target1Name)
                .DependsOn(target2Name)
                .DependsOn(target3Name)
                .DependsOn(target4Name)
                .DependsOn(target5Name);
        }
    }
}
