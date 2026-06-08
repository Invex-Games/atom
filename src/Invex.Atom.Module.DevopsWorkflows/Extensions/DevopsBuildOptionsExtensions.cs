namespace Invex.Atom.Module.DevopsWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class DevopsBuildOptionsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class DevopsPoolOptions
    {
        // Windows
        public DevopsPool Windows_2025_Vs2026 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Windows_2025_Vs2026,
            };

        public DevopsPool Windows_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Windows_Latest,
            };

        public DevopsPool Windows_2025 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Windows_2025,
            };

        public DevopsPool Windows_2022 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Windows_2022,
            };

        // Linux
        public DevopsPool Ubuntu_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Ubuntu_Latest,
            };

        public DevopsPool Ubuntu_24_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Ubuntu_24_04,
            };

        public DevopsPool Ubuntu_22_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.Ubuntu_22_04,
            };

        // MacOS
        public DevopsPool MacOs_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.MacOs_Latest,
            };

        public DevopsPool MacOs_15 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.MacOs_15,
            };

        public DevopsPool MacOs_14 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.Pool.MacOs_14,
            };

        public DevopsPool SetByMatrix =>
            field ??= new()
            {
                HostedImage = "$(job-runs-on)",
            };

        public DevopsPool FromName(TextExpression name) =>
            new()
            {
                HostedImage = name,
            };

        public DevopsPool FromName(string name) =>
            new()
            {
                HostedImage = name,
            };

        public DevopsPool FromHostedImage(TextExpression hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevopsPool FromHostedImage(string hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevopsPool FromDemands(params TextExpression[] demands) =>
            new()
            {
                Demands = demands,
            };

        public DevopsPool FromDemands(params string[] demands) =>
            new()
            {
                Demands = demands
                    .Select(TextExpressions.Raw)
                    .ToArray(),
            };

        public DevopsPool From(
            TextExpression? name = null,
            TextExpression? hostedImageName = null,
            IEnumerable<TextExpression>? demands = null) =>
            new()
            {
                Name = name,
                HostedImage = hostedImageName,
                Demands = demands?.ToArray() ?? [],
            };

        public DevopsPool From(
            string? name = null,
            string? hostedImageName = null,
            IEnumerable<string>? demands = null) =>
            new()
            {
                Name = name is null
                    ? null
                    : TextExpressions.Raw(name),
                HostedImage = hostedImageName is null
                    ? null
                    : TextExpressions.Raw(hostedImageName),
                Demands = demands
                    ?.Select(TextExpressions.Raw)
                    .ToArray() ?? [],
            };
    }

    [PublicAPI]
    public sealed class DevopsVariableGroupOptions
    {
        public DevopsVariableGroup Atom => field ??= new("Atom");
    }

    [PublicAPI]
    public sealed class DevopsStepsOptions
    {
        public DevopsCheckoutStep Checkout(DevopsCheckoutStep step) =>
            step;
    }

    [PublicAPI]
    public sealed class DevopsOptions
    {
        internal static DevopsOptions Instance => field ??= new();

        public DevopsPoolOptions DevopsPool => field ??= new();

        public DevopsVariableGroupOptions VariableGroup => field ??= new();

        public DevopsStepsOptions Steps => field ??= new();

        public ProvideDevopsRunIdAsWorkflowId ProvideDevopsRunIdAsWorkflowId => field ??= new();
    }

    extension(BuildOptions)
    {
        [PublicAPI]
        public static DevopsOptions Devops => DevopsOptions.Instance;
    }
}
