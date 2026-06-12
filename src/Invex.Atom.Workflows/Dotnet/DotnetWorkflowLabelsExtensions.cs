namespace Invex.Atom.Workflows.Dotnet;

/// <summary>
///     Extends the <see cref="WorkflowLabels" /> anchor class with .NET-related label constants.
/// </summary>
[PublicAPI]
public static class DotnetWorkflowLabelsExtensions
{
    /// <summary>
    ///     Provides target framework moniker labels (e.g., <c>net8.0</c>) for use in build matrices.
    /// </summary>
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class Framework
    {
        /// <summary>The <c>net10.0</c> target framework.</summary>
        public readonly string Net_10_0 = "net10.0";

        /// <summary>The <c>net471</c> target framework.</summary>
        public readonly string Net_4_7_1 = "net471";

        /// <summary>The <c>net48</c> target framework.</summary>
        public readonly string Net_4_8 = "net48";

        /// <summary>The <c>net481</c> target framework.</summary>
        public readonly string Net_4_8_1 = "net481";

        /// <summary>The <c>net8.0</c> target framework.</summary>
        public readonly string Net_8_0 = "net8.0";

        /// <summary>The <c>net9.0</c> target framework.</summary>
        public readonly string Net_9_0 = "net9.0";

        /// <summary>The <c>netstandard2.0</c> target framework.</summary>
        public readonly string Net_Standard_2_0 = "netstandard2.0";

        /// <summary>The <c>netstandard2.1</c> target framework.</summary>
        public readonly string Net_Standard_2_1 = "netstandard2.1";
    }

    /// <summary>
    ///     Provides .NET-related workflow labels.
    /// </summary>
    [PublicAPI]
    public sealed class DotnetLabels
    {
        internal static DotnetLabels Instance => field ??= new();

        /// <summary>
        ///     Gets the target framework moniker labels.
        /// </summary>
        public Framework Framework => field ??= new();
    }

    extension(WorkflowLabels)
    {
        /// <summary>
        ///     Gets .NET-related workflow labels.
        /// </summary>
        [PublicAPI]
        public static DotnetLabels Dotnet => DotnetLabels.Instance;
    }
}
