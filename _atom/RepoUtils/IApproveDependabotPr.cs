namespace Atom.RepoUtils;

internal interface IApproveDependabotPr : IGithubHelper, IPullRequestHelper
{
    const string DependabotActorName = "dependabot[bot]";

    [SecretDefinition("dependabot-enable-auto-merge-pat",
        "A GitHub PAT with permissions to enable auto-merge on pull requests.")]
    string? DependabotEnableAutoMergePat => GetParam(() => DependabotEnableAutoMergePat);

    Target ApproveDependabotPr =>
        t => t
            .RequiresParam(nameof(PullRequestNumber), nameof(DependabotEnableAutoMergePat))
            .Executes(async cancellationToken =>
            {
                var actor = Github.Variables.Actor;
                var owner = Github.Variables.RepositoryOwner;

                var repo = Github.Variables
                    .Repository
                    .Split('/')
                    .Last();

                Logger.LogInformation("Github API action context: {Context}",
                    new
                    {
                        Actor = actor,
                        PullRequestNumber,
                        Owner = owner,
                        Repo = repo,
                    });

                if (actor != DependabotActorName)
                    throw new StepFailedException(
                        $"Only pull requests from {DependabotActorName} can be auto-approved.");

                var productHeader = new ProductHeaderValue("Atom");

                var connection = new Connection(productHeader,
                    new InMemoryCredentialStore(DependabotEnableAutoMergePat));

                var prQuery = new Query()
                    .Repository(repo, owner)
                    .PullRequest(PullRequestNumber)
                    .Select(p => new
                    {
                        p.Id,
                        p.HeadRefOid,
                    })
                    .Compile();

                var prQueryResult = await connection.Run(prQuery, cancellationToken: cancellationToken);

                if (prQueryResult.Id.Value is null)
                    throw new StepFailedException("Could not find pull request.");

                var enableAutoMergeMutation = new Mutation()
                    .EnablePullRequestAutoMerge(new EnablePullRequestAutoMergeInput
                    {
                        PullRequestId = prQueryResult.Id,
                        MergeMethod = PullRequestMergeMethod.Merge,
                    })
                    .Select(x => new
                    {
                        x.ClientMutationId,
                    })
                    .Compile();

                var enableAutoMergeResult =
                    await connection.Run(enableAutoMergeMutation, cancellationToken: cancellationToken);

                if (enableAutoMergeResult is null)
                    throw new StepFailedException("Could not enable auto merge.");
            });
}
