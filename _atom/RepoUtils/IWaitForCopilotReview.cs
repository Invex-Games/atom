namespace Atom.RepoUtils;

/// <summary>
///     Provides a build target that blocks until GitHub Copilot has finished reviewing a pull request.
///     This fills the gap where GitHub cannot natively wait for a Copilot review before completing an
///     auto-merge, allowing the target to be sequenced ahead of an auto-merge step.
/// </summary>
internal interface IWaitForCopilotReview : ICopilotReviewHelper, IPullRequestHelper, IGithubHelper
{
    /// <summary>
    ///     The login of the Copilot reviewer bot to wait for. Defaults to
    ///     <see cref="ICopilotReviewHelper.DefaultCopilotReviewerLogin" />.
    /// </summary>
    [ParamDefinition("copilot-reviewer-login", "The login of the Copilot reviewer bot to wait for.")]
    string CopilotReviewerLogin => GetParam(() => CopilotReviewerLogin, DefaultCopilotReviewerLogin);

    /// <summary>
    ///     The maximum amount of time, in seconds, to wait for Copilot to finish reviewing before failing
    ///     the target. Defaults to 600 seconds (10 minutes).
    /// </summary>
    [ParamDefinition("copilot-review-timeout-seconds",
        "Maximum time in seconds to wait for Copilot to finish reviewing.")]
    int CopilotReviewTimeoutSeconds => GetParam(() => CopilotReviewTimeoutSeconds, 600);

    /// <summary>
    ///     The delay, in seconds, between successive polls of the pull request's review state. Defaults to
    ///     15 seconds.
    /// </summary>
    [ParamDefinition("copilot-review-poll-interval-seconds",
        "Polling interval in seconds while waiting for Copilot to finish reviewing.")]
    int CopilotReviewPollIntervalSeconds => GetParam(() => CopilotReviewPollIntervalSeconds, 15);

    /// <summary>
    ///     An optional GitHub personal access token used to read the Copilot review state. When not
    ///     supplied, the default <see cref="IGithubHelper.GithubToken" /> is used instead. A dedicated PAT
    ///     can be provided for cases where the default workflow token cannot read the review state (for
    ///     example on some Dependabot-created pull requests).
    /// </summary>
    [SecretDefinition("copilot-review-token",
        "Optional GitHub PAT used to read Copilot review state; falls back to the default GitHub token.")]
    string? CopilotReviewToken => GetParam(() => CopilotReviewToken);

    /// <summary>
    ///     Waits until Copilot has finished reviewing the target pull request. The target succeeds
    ///     immediately when Copilot was not requested as a reviewer, and fails when Copilot does not
    ///     finish reviewing within the configured timeout.
    /// </summary>
    Target WaitForCopilotReview =>
        t => t
            .RequiresParam(nameof(PullRequestNumber))
            .UsesParam(nameof(CopilotReviewerLogin))
            .UsesParam(nameof(GithubToken))
            .UsesParam(nameof(CopilotReviewTimeoutSeconds))
            .UsesParam(nameof(CopilotReviewPollIntervalSeconds))
            .UsesParam(nameof(CopilotReviewToken))
            .Executes(async cancellationToken =>
            {
                // Prefer the dedicated PAT when supplied, otherwise fall back to the default token.
                var token = string.IsNullOrWhiteSpace(CopilotReviewToken)
                    ? GithubToken
                    : CopilotReviewToken;

                if (string.IsNullOrWhiteSpace(token))
                    throw new StepFailedException(
                        $"A GitHub token is required. Provide '{nameof(GithubToken)}' or '{nameof(CopilotReviewToken)}'.");

                await WaitForCopilotReviewToComplete(PullRequestNumber,
                    token,
                    CopilotReviewerLogin,
                    TimeSpan.FromSeconds(CopilotReviewTimeoutSeconds),
                    TimeSpan.FromSeconds(CopilotReviewPollIntervalSeconds),
                    cancellationToken);
            });
}
