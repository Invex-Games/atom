namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
public static class WorkflowLabelsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class RunsOn
    {
        public readonly string MacOs_14 = "macos-14";

        public readonly string MacOs_15 = "macos-15";

        public readonly string MacOs_15_Arm64 = "macos-15-arm64";

        // MacOS
        public readonly string MacOs_Latest = "macos-latest";

        public readonly string Ubuntu_22_04 = "ubuntu-22.04";

        public readonly string Ubuntu_24_04 = "ubuntu-24.04";

        // Linux
        public readonly string Ubuntu_Latest = "ubuntu-latest";

        public readonly string Windows_2022 = "windows-2022";

        public readonly string Windows_2025 = "windows-2025";

        // Windows
        public readonly string Windows_Latest = "windows-latest";
    }

    [PublicAPI]
    public sealed class Labels
    {
        internal static Labels Instance => field ??= new();

        public RunsOn DevopsPool => field ??= new();
    }

    extension(WorkflowLabels)
    {
        [PublicAPI]
        public static Labels Devops => Labels.Instance;
    }
}
