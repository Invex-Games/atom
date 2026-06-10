namespace Atom.RepoUtils;

/// <summary>
///     Provides functionality for waiting until GitHub Copilot has finished reviewing a pull request.
///     GitHub does not currently expose a native way to block auto-merge on a Copilot review, so this
///     helper polls the pull request's review state until Copilot's review has landed (or a timeout
///     elapses).
/// </summary>
internal interface ICopilotReviewHelper : IBuildAccessor
{
    /// <summary>
    ///     The default login of the GitHub Copilot pull request reviewer bot.
    /// </summary>
    const string DefaultCopilotReviewerLogin = "Copilot";

    /// <summary>
    ///     Polls the supplied pull request until GitHub Copilot has finished reviewing it.
    /// </summary>
    /// <param name="pullRequestNumber">The number of the pull request to wait on.</param>
    /// <param name="githubToken">The GitHub token used to authenticate the GraphQL requests.</param>
    /// <param name="copilotReviewerLogin">The login of the Copilot reviewer bot to match against.</param>
    /// <param name="timeout">The maximum amount of time to wait for Copilot to finish reviewing.</param>
    /// <param name="pollInterval">The delay between successive polls of the pull request state.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    ///     <see langword="true" /> when Copilot has finished reviewing the pull request; <see langword="false" />
    ///     when Copilot was never requested as a reviewer and therefore there was nothing to wait for.
    /// </returns>
    /// <exception cref="StepFailedException">
    ///     Thrown when Copilot does not finish reviewing within <paramref name="timeout" />.
    /// </exception>
    /// <remarks>
    ///     While Copilot is reviewing it remains a pending review request on the pull request. Once it is
    ///     done it is removed from the pending review requests and a review authored by the Copilot bot
    ///     appears. The method therefore considers the review complete when Copilot is no longer pending
    ///     and a review from it exists.
    /// </remarks>
    async Task<bool> WaitForCopilotReviewToComplete(
        int pullRequestNumber,
        string githubToken,
        string copilotReviewerLogin,
        TimeSpan timeout,
        TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        var owner = Github.Variables.RepositoryOwner;

        // The repository variable is in "owner/name" form; we only need the repository name here.
        var repository = Github.Variables
            .Repository
            .Split('/')
            .Last();

        Logger.LogInformation("Waiting for Copilot ('{CopilotLogin}') to finish reviewing pull request #{PullRequest}.",
            copilotReviewerLogin,
            pullRequestNumber);

        // Establish an authenticated GraphQL connection to the GitHub API.
        var productHeader = new ProductHeaderValue("Atom");
        var connection = new Connection(productHeader, new InMemoryCredentialStore(githubToken));

        var deadline = DateTimeOffset.UtcNow + timeout;

        while (true)
        {
            var (isPending, hasReview) = await QueryCopilotReviewState(connection,
                repository,
                owner,
                pullRequestNumber,
                copilotReviewerLogin,
                cancellationToken);

            // Copilot was never requested and has not left a review: there is nothing to wait for.
            if (!isPending && !hasReview)
            {
                Logger.LogInformation(
                    "Copilot was not requested as a reviewer on pull request #{PullRequest}. Nothing to wait for.",
                    pullRequestNumber);

                return false;
            }

            // Copilot has submitted its review and is no longer pending: the review is complete.
            if (hasReview && !isPending)
            {
                Logger.LogInformation("Copilot has finished reviewing pull request #{PullRequest}.", pullRequestNumber);

                return true;
            }

            if (DateTimeOffset.UtcNow >= deadline)
                throw new StepFailedException(
                    $"Timed out after {timeout} waiting for Copilot to finish reviewing pull request #{pullRequestNumber}.");

            Logger.LogInformation("Copilot review of pull request #{PullRequest} is still pending. Waiting {Interval}.",
                pullRequestNumber,
                pollInterval);

            await Task.Delay(pollInterval, cancellationToken);
        }
    }

    /// <summary>
    ///     Queries the current Copilot review state of a pull request.
    /// </summary>
    /// <param name="connection">The authenticated GraphQL connection to use.</param>
    /// <param name="repository">The name of the repository the pull request belongs to.</param>
    /// <param name="owner">The owner (user or organization) of the repository.</param>
    /// <param name="pullRequestNumber">The number of the pull request to inspect.</param>
    /// <param name="copilotReviewerLogin">The login of the Copilot reviewer bot to match against.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    ///     A tuple describing whether Copilot is currently a pending review request and whether a review
    ///     authored by Copilot already exists.
    /// </returns>
    /// <exception cref="StepFailedException">Thrown when the pull request cannot be resolved.</exception>
    private async Task<(bool IsPending, bool HasReview)> QueryCopilotReviewState(
        Connection connection,
        string repository,
        string owner,
        int pullRequestNumber,
        string copilotReviewerLogin,
        CancellationToken cancellationToken)
    {
        var query = new Query()
            .Repository(repository, owner)
            .PullRequest(pullRequestNumber)
            .Select(p => new ReviewState
            {
                // Logins of the reviewers that still have a pending review request on the pull request.
                PendingReviewerLogins = p
                    .ReviewRequests(100, null, null, null)
                    .Nodes
                    .Select(rr => rr.RequestedReviewer.Switch<string?>(when => when
                        .User(user => user.Login)
                        .Bot(bot => bot.Login)
                        .Mannequin(mannequin => mannequin.Login)))
                    .ToList(),
                // Logins of the authors of any reviews that have already been submitted.
                ReviewAuthorLogins = p
                    .Reviews(100, null, null, null, null, null)
                    .Nodes
                    .Select(review => review.Author.Login)
                    .ToList(),
            })
            .Compile();

        var result = await connection.Run(query, cancellationToken: cancellationToken);

        if (result is null)
            throw new StepFailedException($"Could not find pull request #{pullRequestNumber}.");

        Logger.LogInformation(
            "Pull request #{PullRequest} review state. Pending reviewers: [{PendingReviewers}]. Review authors: [{ReviewAuthors}].",
            pullRequestNumber,
            string.Join(", ", result.PendingReviewerLogins.Where(login => login is not null)),
            string.Join(", ", result.ReviewAuthorLogins));

        // GitHub Copilot's reviewer bot surfaces with a login such as "copilot-pull-request-reviewer"
        // (the "Copilot" shown in the UI is only the display name), so match on a case-insensitive
        // substring rather than an exact login to remain robust across these representations.
        var isPending = result.PendingReviewerLogins.Any(login => IsCopilotLogin(login, copilotReviewerLogin));

        var hasReview = result.ReviewAuthorLogins.Any(login => IsCopilotLogin(login, copilotReviewerLogin));

        return (isPending, hasReview);

        static bool IsCopilotLogin(string? login, string copilotReviewerLogin)
        {
            return login is not null &&
                   (login.Contains(copilotReviewerLogin, StringComparison.OrdinalIgnoreCase) ||
                    copilotReviewerLogin.Contains(login, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    ///     The projected GraphQL result describing the review state of a pull request.
    /// </summary>
    private sealed class ReviewState
    {
        /// <summary>
        ///     The logins of the reviewers that still have a pending review request on the pull request.
        /// </summary>
        public required List<string?> PendingReviewerLogins { get; init; }

        /// <summary>
        ///     The logins of the authors of the reviews that have already been submitted.
        /// </summary>
        public required List<string> ReviewAuthorLogins { get; init; }
    }
}
