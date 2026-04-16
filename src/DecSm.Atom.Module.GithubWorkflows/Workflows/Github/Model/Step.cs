namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
[Union]
public partial record Step
{
    public string? Id { get; init; }

    public string? If { get; init; }

    public string? Name { get; init; }

    public string? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, SingleOrList<string>>? With { get; init; }

    public IReadOnlyDictionary<string, string>? Env { get; init; }

    public string? ContinueOnError { get; init; }

    public string? TimeoutMinutes { get; init; }

    public partial record UsesStep
    {
        public required string Uses { get; init; }
    }

    public partial record RunStep
    {
        public required SingleOrList<string> Run { get; init; }

        public string? Shell { get; init; }
    }
}
