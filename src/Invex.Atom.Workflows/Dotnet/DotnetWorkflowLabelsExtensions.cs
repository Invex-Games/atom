namespace Invex.Atom.Workflows.Dotnet;

[PublicAPI]
public static class DotnetWorkflowLabelsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class Framework
    {
        public readonly string Net_10_0 = "net10.0";
        public readonly string Net_4_7_1 = "net471";
        public readonly string Net_4_8 = "net48";
        public readonly string Net_4_8_1 = "net481";
        public readonly string Net_8_0 = "net8.0";
        public readonly string Net_9_0 = "net9.0";
        public readonly string Net_Standard_2_0 = "netstandard2.0";
        public readonly string Net_Standard_2_1 = "netstandard2.1";
    }

    [PublicAPI]
    public sealed class DotnetLabels
    {
        internal static DotnetLabels Instance => field ??= new();

        public Framework Framework => field ??= new();
    }

    extension(WorkflowLabels)
    {
        [PublicAPI]
        public static DotnetLabels Dotnet => DotnetLabels.Instance;
    }
}
