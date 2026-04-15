namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public static class WorkflowOptionExtensions
{
    extension<TOption>(TOption)
        where TOption : IWorkflowOption
    {
        [PublicAPI]
        public static TOption? GetOption(IEnumerable<IWorkflowOption> workflowOptions) =>
            workflowOptions
                .OfType<TOption>()
                .LastOrDefault();

        [PublicAPI]
        public static TOption? GetOption(WorkflowModel workflow) =>
            workflow
                .Options
                .OfType<TOption>()
                .LastOrDefault();

        [PublicAPI]
        public static TOption? GetOption(WorkflowModel workflow, WorkflowStepModel step) =>
            workflow
                .Options
                .Concat(step.Options)
                .OfType<TOption>()
                .LastOrDefault();

        [PublicAPI]
        public static IEnumerable<TOption> GetOptions(WorkflowModel workflow, WorkflowStepModel step) =>
            workflow
                .Options
                .Concat(step.Options)
                .OfType<TOption>();
    }

    extension<TOption>(TOption)
        where TOption : ToggleWorkflowOption<TOption>
    {
        [PublicAPI]
        public static bool IsOptionEnabled(IEnumerable<IWorkflowOption> workflowOptions) =>
            workflowOptions
                .OfType<TOption>()
                .LastOrDefault() is { Value: true };

        [PublicAPI]
        public static bool IsOptionEnabled(WorkflowModel workflow) =>
            workflow
                .Options
                .OfType<TOption>()
                .LastOrDefault() is { Value: true };

        [PublicAPI]
        public static bool IsOptionEnabled(WorkflowModel workflow, WorkflowStepModel step) =>
            workflow
                .Options
                .Concat(step.Options)
                .OfType<TOption>()
                .LastOrDefault() is { Value: true };
    }
}
