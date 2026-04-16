namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Matrix
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Map { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, string>>? Include { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, string>>? Exclude { get; init; }
}
