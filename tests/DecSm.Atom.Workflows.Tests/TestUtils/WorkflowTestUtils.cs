namespace DecSm.Atom.Workflows.Tests.TestUtils;

[PublicAPI]
internal static class WorkflowTestUtils
{
    public static IHost CreateWorkflowTestHost<T>(
        TestConsole? console = null,
        MockFileSystem? fileSystem = null,
        CommandLineArgs? commandLineArgs = null,
        TestBuildIdProvider? buildIdProvider = null,
        TestBuildVersionProvider? buildVersionProvider = null,
        TestWorkflowWriter? workflowWriter = null,
        Action<HostApplicationBuilder>? configure = null)
        where T : BuildDefinition =>
        Atom.TestUtils.TestUtils.CreateTestHost<T>(console,
            fileSystem,
            commandLineArgs ??
            new CommandLineArgs(true, [new CommandArg(nameof(IGenerateWorkflowFiles.GenerateWorkflowFiles))]),
            buildIdProvider,
            buildVersionProvider,
            x =>
            {
                if (workflowWriter != null)
                    x.Services.AddSingleton<IWorkflowWriter>(workflowWriter);
                else
                    x.Services.AddSingleton<IWorkflowWriter, TestWorkflowWriter>();

                configure?.Invoke(x);
            });
}
