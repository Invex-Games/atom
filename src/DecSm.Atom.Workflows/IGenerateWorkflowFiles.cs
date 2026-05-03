namespace DecSm.Atom.Workflows;

[PublicAPI]
public interface IGenerateWorkflowFiles : IBuildAccessor
{
    Target GenerateWorkflowFiles =>
        t => t
            .WithAlias("Gen")
            .DescribedAs("Generates workflow files")
            .Executes(() => GetService<WorkflowGenerator>()
                .GenerateWorkflows());
}
