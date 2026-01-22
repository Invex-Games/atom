namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowLabelsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class RunsOn
    {
        // Linux x64 1x
        public string Ubuntu_Slim => "ubuntu-latest";

        // Linux x64 4x
        public string Ubuntu_Latest => "ubuntu-latest";

        public string Ubuntu_24_04 => "ubuntu-24.04";

        public string Ubuntu_22_04 => "ubuntu-22.04";

        // Linux ARM 4x
        public string Ubuntu_24_04_Arm => "ubuntu-24.04-arm";

        public string Ubuntu_22_04_Arm => "ubuntu-22.04-arm";

        // Windows x64 4x
        public string Windows_Latest => "windows-latest";

        public string Windows_2025 => "windows-2025";

        public string Windows_2022 => "windows-2022";

        // Windows ARM 4x
        public string Windows_11_Arm => "windows-11-arm";

        // MacOS x64 4x
        public string MacOs_13 => "macos-14";

        public string MacOs_15_Intel => "macos-15-intel";

        // MacOS ARM 3x
        public string MacOs_Latest => "macos-latest";

        public string MacOs_26 => "macos-26";

        public string MacOs_15 => "macos-15";

        public string MacOs_14 => "macos-14";

        // MacOS x64 12x
        public string MacOs_Latest_Large => "macos-latest-large";

        public string MacOs_15_Large => "macos-15-large";

        public string MacOs_14_Large => "macos-14-large";

        public string MacOs_13_Large => "macos-13-large";

        // MacOS ARM 5x
        public string MacOs_Latest_XLarge => "macos-latest-xlarge";

        public string MacOs_26_XLarge => "macos-26-xlarge";

        public string MacOs_15_XLarge => "macos-15-xlarge";

        public string MacOs_14_XLarge => "macos-14-xlarge";

        public string MacOs_13_XLarge => "macos-13-xlarge";
    }

    [PublicAPI]
    public sealed class Dependabot
    {
        public string NugetType => "nuget-feed";

        public string NugetEcosystem => "nuget";

        public string NugetUrl => "https://api.nuget.org/v3/index.json";
    }

    [PublicAPI]
    public sealed class Labels
    {
        internal static Labels Instance => field ??= new();

        public RunsOn RunsOn => field ??= new();

        public Dependabot Dependabot => field ??= new();
    }

    extension(WorkflowLabels)
    {
        [PublicAPI]
        public static Labels Github => Labels.Instance;
    }
}
