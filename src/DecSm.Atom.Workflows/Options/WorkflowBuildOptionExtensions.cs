namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public static class WorkflowBuildOptionExtensions
{
    extension<T>(T)
        where T : IBuildOption
    {
        [PublicAPI]
        public static IReadOnlyList<T> GetOptions(WorkflowModel workflow, WorkflowStepModel step) =>
            workflow
                .WorkflowOptions
                .Concat(step.Options)
                .OfType<T>()
                .ToList();

        [PublicAPI]
        public static T? Get(WorkflowModel workflow, WorkflowStepModel step) =>
            workflow
                .WorkflowOptions
                .Concat(step.Options)
                .OfType<T>()
                .LastOrDefault();

        [PublicAPI]
        public static T? Get(WorkflowModel workflow) =>
            workflow
                .WorkflowOptions
                .OfType<T>()
                .LastOrDefault();
    }
}
