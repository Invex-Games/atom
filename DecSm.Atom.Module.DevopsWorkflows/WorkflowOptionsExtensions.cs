namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class DevopsPoolOptions
    {
        // Windows
        public DevopsPool Windows_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Windows_Latest,
            };

        public DevopsPool Windows_2025 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Windows_2025,
            };

        public DevopsPool Windows_2022 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Windows_2022,
            };

        // Linux
        public DevopsPool Ubuntu_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Ubuntu_Latest,
            };

        public DevopsPool Ubuntu_24_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Ubuntu_24_04,
            };

        public DevopsPool Ubuntu_22_04 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.Ubuntu_22_04,
            };

        // MacOS
        public DevopsPool MacOs_Latest =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.MacOs_Latest,
            };

        public DevopsPool MacOs_15 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.MacOs_15,
            };

        public DevopsPool MacOs_15_Arm64 =>
            field ??= new()
            {
                HostedImage = WorkflowLabels.Devops.DevopsPool.MacOs_15_Arm64,
            };

        public DevopsPool SetByMatrix =>
            field ??= new()
            {
                HostedImage = "$(job-runs-on)",
            };

        public DevopsPool FromName(WorkflowExpression name) =>
            new()
            {
                HostedImage = name,
            };

        public DevopsPool FromName(string name) =>
            new()
            {
                HostedImage = name,
            };

        public DevopsPool FromHostedImage(WorkflowExpression hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevopsPool FromHostedImage(string hostedImageName) =>
            new()
            {
                HostedImage = hostedImageName,
            };

        public DevopsPool FromDemands(params WorkflowExpression[] demands) =>
            new()
            {
                Demands = demands,
            };

        public DevopsPool FromDemands(params string[] demands) =>
            new()
            {
                Demands = demands
                    .Select(WorkflowExpressions.Literal)
                    .ToArray(),
            };

        public DevopsPool From(
            WorkflowExpression? name = null,
            WorkflowExpression? hostedImageName = null,
            IEnumerable<WorkflowExpression>? demands = null) =>
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
                    : WorkflowExpressions.Literal(name),
                HostedImage = hostedImageName is null
                    ? null
                    : WorkflowExpressions.Literal(hostedImageName),
                Demands = demands
                    ?.Select(WorkflowExpressions.Literal)
                    .ToArray() ?? [],
            };
    }

    [PublicAPI]
    public sealed class DevopsVariableGroupOptions
    {
        public DevopsVariableGroup Atom => field ??= new("Atom");
    }

    [PublicAPI]
    public sealed class Options
    {
        internal static Options Instance => field ??= new();

        public DevopsPoolOptions DevopsPool => field ??= new();

        public DevopsVariableGroupOptions VariableGroup => field ??= new();

        public ProvideDevopsRunIdAsWorkflowId UseDevopsRunIdAsWorkflowId => field ??= new();

        public DevopsCheckoutOption ConfigureCheckout(bool lfs = false, string? submodules = null) =>
            new(lfs, submodules);
    }

    extension(WorkflowOptions)
    {
        [PublicAPI]
        public static Options Devops => Options.Instance;
    }
}
