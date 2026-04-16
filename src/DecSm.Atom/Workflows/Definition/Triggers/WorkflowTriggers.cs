namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public static class WorkflowTriggers
{
    public static ManualTrigger Manual => field ??= new();

    public static GitPullRequestTrigger PullIntoMain =>
        field ??= new()
        {
            IncludedBranches = ["main"],
        };

    public static GitPushTrigger PushToMain =>
        field ??= new()
        {
            IncludedBranches = ["main"],
        };

    public static GitPullRequestTrigger PullInto(params string[] includedBranches) =>
        new()
        {
            IncludedBranches = includedBranches.ToList(),
        };

    public static GitPushTrigger PushTo(params string[] includedBranches) =>
        new()
        {
            IncludedBranches = includedBranches.ToList(),
        };

    public static ManualTrigger ManualWithInputs(params ManualInput[] inputs) =>
        new(inputs.ToList());

    public static GitPullRequestTrigger PullRequest(
        IReadOnlyList<string> includedBranches,
        IReadOnlyList<string> excludedBranches,
        IReadOnlyList<string> includedPaths,
        IReadOnlyList<string> excludedPaths,
        IReadOnlyList<string> types) =>
        new()
        {
            IncludedBranches = includedBranches,
            ExcludedBranches = excludedBranches,
            IncludedPaths = includedPaths,
            ExcludedPaths = excludedPaths,
            Types = types,
        };

    public static GitPushTrigger Push(
        IReadOnlyList<string> includedBranches,
        IReadOnlyList<string> excludedBranches,
        IReadOnlyList<string> includedPaths,
        IReadOnlyList<string> excludedPaths,
        IReadOnlyList<string> includedTags,
        IReadOnlyList<string> excludedTags) =>
        new()
        {
            IncludedBranches = includedBranches,
            ExcludedBranches = excludedBranches,
            IncludedPaths = includedPaths,
            ExcludedPaths = excludedPaths,
            IncludedTags = includedTags,
            ExcludedTags = excludedTags,
        };
}
