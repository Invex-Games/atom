using DecSm.Atom.Build;

namespace Atom.Targets;

public interface IPullRequestHelper : IBuildAccessor
{
    [ParamDefinition("pull-request-number", "The pull request number to approve.")]
    int PullRequestNumber => GetParam(() => PullRequestNumber);
}
