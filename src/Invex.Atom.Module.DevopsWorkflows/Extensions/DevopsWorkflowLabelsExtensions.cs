namespace Invex.Atom.Module.DevopsWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class DevopsWorkflowLabelsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class Pool
    {
        // Windows
        public string Windows_2025_Vs2026 => "windows-2025-vs2026";

        public string Windows_Latest => "windows-latest";

        public string Windows_2025 => "windows-2025";

        public string Windows_2022 => "windows-2022";

        // Linux
        public string Ubuntu_Latest => "ubuntu-latest";

        public string Ubuntu_24_04 => "ubuntu-24.04";

        public string Ubuntu_22_04 => "ubuntu-22.04";

        // MacOS
        public string MacOs_Latest => "macos-latest";

        public string MacOs_15 => "macos-15";

        public string MacOs_14 => "macos-14";
    }

    [PublicAPI]
    public sealed class DevopsLabels
    {
        internal static DevopsLabels Instance => field ??= new();

        public Pool Pool => field ??= new();
    }

    extension(WorkflowLabels)
    {
        [PublicAPI]
        public static DevopsLabels Devops => DevopsLabels.Instance;
    }
}
