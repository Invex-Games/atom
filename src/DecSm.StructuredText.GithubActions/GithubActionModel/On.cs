namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
[Union]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial record On
{
    partial record BranchProtectionRule(IReadOnlyList<BranchProtectionRule.BranchProtectionType> Types)
    {
        public enum BranchProtectionType
        {
            created,
            edited,
            deleted,
        }
    }

    partial record CheckRun(IReadOnlyList<CheckRun.CheckRunType> Types)
    {
        public enum CheckRunType
        {
            created,
            edited,
            deleted,
        }
    }

    partial record CheckSuite(IReadOnlyList<CheckSuite.CheckSuiteType> Types)
    {
        public enum CheckSuiteType
        {
            completed,
        }
    }

    partial record Create;

    partial record Delete;

    partial record Deployment;

    partial record DeploymentStatus;

    partial record Discussion(IReadOnlyList<Discussion.DiscussionType> Types)
    {
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

    partial record DiscussionComment(IReadOnlyList<DiscussionComment.DiscussionCommentType> Types)
    {
        public enum DiscussionCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    partial record Fork;

    partial record Gollum;

    partial record ImageVersion
    {
        public required IReadOnlyList<string>? Names { get; init; }

        public required IReadOnlyList<string>? Versions { get; init; }
    }

    partial record IssueComment(IReadOnlyList<IssueComment.IssueCommentType> Types)
    {
        public enum IssueCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    partial record Issues(IReadOnlyList<Issues.IssuesType> Types)
    {
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

    partial record Label(IReadOnlyList<Label.LabelType> Types)
    {
        public enum LabelType
        {
            created,
            edited,
        }
    }

    partial record MergeGroup(IReadOnlyList<MergeGroup.MergeGroupType> Types)
    {
        public enum MergeGroupType
        {
            checks_requested,
        }
    }

    partial record Milestone(IReadOnlyList<Milestone.MilestoneType> Types)
    {
        public enum MilestoneType
        {
            created,
            closed,
            opened,
            edited,
            deleted,
        }
    }

    partial record PageBuild;

    partial record Project(IReadOnlyList<Project.ProjectType> Types)
    {
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

    partial record ProjectCard(IReadOnlyList<ProjectCard.ProjectCardType> Types)
    {
        public enum ProjectCardType
        {
            created,
            moved,
            converted,
            edited,
            deleted,
        }
    }

    partial record ProjectColumn(IReadOnlyList<ProjectColumn.ProjectColumnType> Types)
    {
        public enum ProjectColumnType
        {
            created,
            updated,
            moved,
            deleted,
        }
    }

    partial record Public;

    partial record PullRequest(IReadOnlyList<PullRequest.PullRequestType> Types)
    {
        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<string>? BranchesIgnore { get; init; }

        public required IReadOnlyList<string>? Tags { get; init; }

        public required IReadOnlyList<string>? TagsIgnore { get; init; }

        public required IReadOnlyList<string>? Paths { get; init; }

        public required IReadOnlyList<string>? PathsIgnore { get; init; }

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
    }

    partial record PullRequestReview(IReadOnlyList<PullRequestReview.PullRequestReviewType> Types)
    {
        public enum PullRequestReviewType
        {
            submitted,
            edited,
            dismissed,
        }
    }

    partial record PullRequestReviewComment(IReadOnlyList<PullRequestReviewComment.PullRequestReviewCommentType> Types)
    {
        public enum PullRequestReviewCommentType
        {
            created,
            edited,
            deleted,
        }
    }

    partial record PullRequestTarget(IReadOnlyList<PullRequestTarget.PullRequestTargetType> Types)
    {
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

    partial record Push
    {
        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<string>? BranchesIgnore { get; init; }

        public required IReadOnlyList<string>? Tags { get; init; }

        public required IReadOnlyList<string>? TagsIgnore { get; init; }

        public required IReadOnlyList<string>? Paths { get; init; }

        public required IReadOnlyList<string>? PathsIgnore { get; init; }
    }

    partial record RegistryPackage(IReadOnlyList<RegistryPackage.RegistryPackageType> Types)
    {
        public enum RegistryPackageType
        {
            published,
            updated,
        }
    }

    partial record Release(IReadOnlyList<Release.ReleaseType> Types)
    {
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

    partial record RepositoryDispatch(IReadOnlyList<string> Types);

    partial record Schedule(IReadOnlyList<string> Crons);

    partial record Status;

    partial record Watch(params Watch.WatchType[] Types)
    {
        public enum WatchType
        {
            started,
        }
    }

    partial record WorkflowCall;

    partial record WorkflowDispatch(IReadOnlyList<WorkflowDispatchInput> Inputs);

    partial record WorkflowRun
    {
        public required IReadOnlyList<string>? Workflows { get; init; }

        public required IReadOnlyList<string>? Branches { get; init; }

        public required IReadOnlyList<WorkflowDispatchTypes>? Types { get; init; }

        public enum WorkflowDispatchTypes
        {
            requested,
            completed,
            in_progress,
        }
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

    public partial record String
    {
        public override string Type => "string";
    }

    public partial record Number
    {
        public override string Type => "number";
    }

    public partial record Boolean
    {
        public override string Type => "boolean";
    }

    public partial record Choice
    {
        public override string Type => "choice";

        public required IReadOnlyList<string>? Options { get; init; }
    }
}
