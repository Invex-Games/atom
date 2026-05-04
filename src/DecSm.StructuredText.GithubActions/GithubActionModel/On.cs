namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
[Union]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial record On
{
    [PublicAPI]
    partial record BranchProtectionRule(IReadOnlyList<BranchProtectionRule.BranchProtectionType> Types)
    {
        [PublicAPI]
        public enum BranchProtectionType
        {
            created,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record CheckRun(IReadOnlyList<CheckRun.CheckRunType> Types)
    {
        [PublicAPI]
        public enum CheckRunType
        {
            created,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record CheckSuite(IReadOnlyList<CheckSuite.CheckSuiteType> Types)
    {
        [PublicAPI]
        public enum CheckSuiteType
        {
            completed,
        }
    }

    [PublicAPI]
    partial record Create;

    [PublicAPI]
    partial record Delete;

    [PublicAPI]
    partial record Deployment;

    [PublicAPI]
    partial record DeploymentStatus;

    [PublicAPI]
    partial record Discussion(IReadOnlyList<Discussion.DiscussionType> Types)
    {
        [PublicAPI]
        public enum DiscussionType
        {
            created,
            edited,
            deleted,
            transferred,
            pinned,
            unpinned,
            labeled,
            unlabeled,
            locked,
            unlocked,
            category_changed,
            answered,
            unanswered,
        }
    }

    [PublicAPI]
    partial record DiscussionComment(IReadOnlyList<DiscussionComment.DiscussionCommentType> Types)
    {
        [PublicAPI]
        public enum DiscussionCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record Fork;

    [PublicAPI]
    partial record Gollum;

    [PublicAPI]
    partial record ImageVersion
    {
        public required IReadOnlyList<string>? Names { get; init; }

        public required IReadOnlyList<string>? Versions { get; init; }
    }

    [PublicAPI]
    partial record IssueComment(IReadOnlyList<IssueComment.IssueCommentType> Types)
    {
        [PublicAPI]
        public enum IssueCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record Issues(IReadOnlyList<Issues.IssuesType> Types)
    {
        [PublicAPI]
        public enum IssuesType
        {
            opened,
            edited,
            deleted,
            transferred,
            pinned,
            unpinned,
            closed,
            reopened,
            assigned,
            unassigned,
            labeled,
            unlabeled,
            locked,
            unlocked,
            milestoned,
            demilestoned,
            typed,
            untyped,
        }
    }

    [PublicAPI]
    partial record Label(IReadOnlyList<Label.LabelType> Types)
    {
        [PublicAPI]
        public enum LabelType
        {
            created,
            edited,
        }
    }

    [PublicAPI]
    partial record MergeGroup(IReadOnlyList<MergeGroup.MergeGroupType> Types)
    {
        [PublicAPI]
        public enum MergeGroupType
        {
            checks_requested,
        }
    }

    [PublicAPI]
    partial record Milestone(IReadOnlyList<Milestone.MilestoneType> Types)
    {
        [PublicAPI]
        public enum MilestoneType
        {
            created,
            closed,
            opened,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record PageBuild;

    [PublicAPI]
    partial record Project(IReadOnlyList<Project.ProjectType> Types)
    {
        [PublicAPI]
        public enum ProjectType
        {
            created,
            updated,
            closed,
            reopened,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record ProjectCard(IReadOnlyList<ProjectCard.ProjectCardType> Types)
    {
        [PublicAPI]
        public enum ProjectCardType
        {
            created,
            moved,
            converted,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record ProjectColumn(IReadOnlyList<ProjectColumn.ProjectColumnType> Types)
    {
        [PublicAPI]
        public enum ProjectColumnType
        {
            created,
            updated,
            moved,
            deleted,
        }
    }

    [PublicAPI]
    partial record Public;

    [PublicAPI]
    partial record PullRequest(IReadOnlyList<PullRequest.PullRequestType> Types)
    {
        [PublicAPI]
        public enum PullRequestType
        {
            assigned,
            unassigned,
            labeled,
            unlabeled,
            opened,
            edited,
            closed,
            reopened,
            synchronized,
            converted_to_draft,
            ready_for_review,
            locked,
            unlocked,
            milestoned,
            demilestoned,
            review_requested,
            review_request_removed,
            auto_merge_enabled,
            auto_merge_disabled,
            enqueued,
            dequeued,
        }

        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<string>? BranchesIgnore { get; init; }

        public required IReadOnlyList<string>? Tags { get; init; }

        public required IReadOnlyList<string>? TagsIgnore { get; init; }

        public required IReadOnlyList<string>? Paths { get; init; }

        public required IReadOnlyList<string>? PathsIgnore { get; init; }
    }

    [PublicAPI]
    partial record PullRequestReview(IReadOnlyList<PullRequestReview.PullRequestReviewType> Types)
    {
        [PublicAPI]
        public enum PullRequestReviewType
        {
            submitted,
            edited,
            dismissed,
        }
    }

    [PublicAPI]
    partial record PullRequestReviewComment(IReadOnlyList<PullRequestReviewComment.PullRequestReviewCommentType> Types)
    {
        [PublicAPI]
        public enum PullRequestReviewCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    [PublicAPI]
    partial record PullRequestTarget(IReadOnlyList<PullRequestTarget.PullRequestTargetType> Types)
    {
        [PublicAPI]
        public enum PullRequestTargetType
        {
            assigned,
            unassigned,
            labeled,
            unlabeled,
            opened,
            edited,
            closed,
            reopened,
            synchronized,
            converted_to_draft,
            ready_for_review,
            locked,
            unlocked,
            milestoned,
            demilestoned,
            review_requested,
            review_request_removed,
            auto_merge_enabled,
            auto_merge_disabled,
            enqueued,
            dequeued,
        }
    }

    [PublicAPI]
    partial record Push
    {
        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<string>? BranchesIgnore { get; init; }

        public required IReadOnlyList<string>? Tags { get; init; }

        public required IReadOnlyList<string>? TagsIgnore { get; init; }

        public required IReadOnlyList<string>? Paths { get; init; }

        public required IReadOnlyList<string>? PathsIgnore { get; init; }
    }

    [PublicAPI]
    partial record RegistryPackage(IReadOnlyList<RegistryPackage.RegistryPackageType> Types)
    {
        [PublicAPI]
        public enum RegistryPackageType
        {
            published,
            updated,
        }
    }

    [PublicAPI]
    partial record Release(IReadOnlyList<Release.ReleaseType> Types)
    {
        [PublicAPI]
        public enum ReleaseType
        {
            published,
            unpublished,
            created,
            edited,
            deleted,
            prereleased,
            released,
        }
    }

    [PublicAPI]
    partial record RepositoryDispatch(IReadOnlyList<string> Types);

    [PublicAPI]
    partial record Schedule(IReadOnlyList<string> Crons);

    [PublicAPI]
    partial record Status;

    [PublicAPI]
    partial record Watch(params Watch.WatchType[] Types)
    {
        [PublicAPI]
        public enum WatchType
        {
            started,
        }
    }

    [PublicAPI]
    partial record WorkflowCall;

    [PublicAPI]
    partial record WorkflowDispatch(IReadOnlyList<WorkflowDispatchInput> Inputs);

    [PublicAPI]
    partial record WorkflowRun
    {
        [PublicAPI]
        public enum WorkflowDispatchTypes
        {
            requested,
            completed,
            in_progress,
        }

        public required IReadOnlyList<string>? Workflows { get; init; }

        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<WorkflowDispatchTypes>? Types { get; init; }
    }
}

[PublicAPI]
[Union]
public partial record WorkflowDispatchInput
{
    public required string Name { get; init; }

    public required string? Description { get; init; }

    public required bool? Required { get; init; }

    public required string? Default { get; init; }

    public abstract string Type { get; }

    [PublicAPI]
    public partial record String
    {
        public override string Type => "string";
    }

    [PublicAPI]
    public partial record Number
    {
        public override string Type => "number";
    }

    [PublicAPI]
    public partial record Boolean
    {
        public override string Type => "boolean";
    }

    [PublicAPI]
    public partial record Choice
    {
        public override string Type => "choice";

        public required IReadOnlyList<string>? Options { get; init; }
    }
}
