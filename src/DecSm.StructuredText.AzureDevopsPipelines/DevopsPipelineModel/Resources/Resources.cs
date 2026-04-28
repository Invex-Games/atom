namespace DecSm.StructuredText.AzureDevopsPipelines.DevopsPipelineModel.Resources;

/// <summary>
///     Resources specifies builds, repositories, pipelines, and other resources used by the pipeline.
/// </summary>
[PublicAPI]
public sealed record Resources
{
    /// <summary>
    ///     List of build resources referenced by the pipeline.
    /// </summary>
    public IReadOnlyList<BuildResource>? Builds { get; init; }

    /// <summary>
    ///     List of container images.
    /// </summary>
    public IReadOnlyList<ContainerResource>? Containers { get; init; }

    /// <summary>
    ///     List of pipeline resources.
    /// </summary>
    public IReadOnlyList<PipelineResource>? Pipelines { get; init; }

    /// <summary>
    ///     List of repository resources.
    /// </summary>
    public IReadOnlyList<RepositoryResource>? Repositories { get; init; }

    /// <summary>
    ///     List of webhooks.
    /// </summary>
    public IReadOnlyList<WebhookResource>? Webhooks { get; init; }

    /// <summary>
    ///     List of package resources.
    /// </summary>
    public IReadOnlyList<PackageResource>? Packages { get; init; }
}

/// <summary>
///     Build resource definition.
/// </summary>
[PublicAPI]
public sealed record BuildResource
{
    /// <summary>
    ///     Identifier for the build resource.
    /// </summary>
    public required WorkflowExpression<string> Build { get; init; }

    /// <summary>
    ///     Type of build resource.
    /// </summary>
    public WorkflowExpression<string>? Type { get; init; }

    /// <summary>
    ///     Connection to the build resource.
    /// </summary>
    public WorkflowExpression<string>? Connection { get; init; }

    /// <summary>
    ///     Source (pipeline) to consume as a build resource.
    /// </summary>
    public WorkflowExpression<string>? Source { get; init; }

    /// <summary>
    ///     Version/tags to pick the build.
    /// </summary>
    public WorkflowExpression<string>? Version { get; init; }

    /// <summary>
    ///     Branch to pick the default version.
    /// </summary>
    public WorkflowExpression<string>? Branch { get; init; }

    /// <summary>
    ///     Trigger for this build resource.
    /// </summary>
    public WorkflowExpression<string>? Trigger { get; init; }
}

/// <summary>
///     Container image resource definition.
/// </summary>
[PublicAPI]
public sealed record ContainerResource
{
    /// <summary>
    ///     Identifier for the container resource.
    /// </summary>
    public required WorkflowExpression<string> Container { get; init; }

    /// <summary>
    ///     Container image tag.
    /// </summary>
    public required WorkflowExpression<string> Image { get; init; }

    /// <summary>
    ///     Container registry service connection.
    /// </summary>
    public WorkflowExpression<string>? Endpoint { get; init; }

    /// <summary>
    ///     Environment variables to set in the container.
    /// </summary>
    public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

    /// <summary>
    ///     Options to pass to the container at startup.
    /// </summary>
    public WorkflowExpression<string>? Options { get; init; }

    /// <summary>
    ///     Ports to expose on the container.
    /// </summary>
    public WorkflowExpressionCollection<string>? Ports { get; init; }

    /// <summary>
    ///     Volumes to mount in the container.
    /// </summary>
    public WorkflowExpressionCollection<string>? Volumes { get; init; }

    /// <summary>
    ///     Trigger for this container resource.
    /// </summary>
    public ContainerResourceTrigger? Trigger { get; init; }
}

/// <summary>
///     Container resource trigger definition.
/// </summary>
[PublicAPI]
public sealed record ContainerResourceTrigger
{
    /// <summary>
    ///     Enable or disable the container trigger.
    /// </summary>
    public WorkflowExpression<bool>? Enabled { get; init; }

    /// <summary>
    ///     Tags to trigger on.
    /// </summary>
    public IncludeExcludeFilters? Tags { get; init; }
}

/// <summary>
///     Pipeline resource definition.
/// </summary>
[PublicAPI]
public sealed record PipelineResource
{
    /// <summary>
    ///     Identifier for the pipeline resource.
    /// </summary>
    public required WorkflowExpression<string> Pipeline { get; init; }

    /// <summary>
    ///     Project for the pipeline resource.
    /// </summary>
    public WorkflowExpression<string>? Project { get; init; }

    /// <summary>
    ///     Source (pipeline) to consume as a pipeline resource.
    /// </summary>
    public required WorkflowExpression<string> Source { get; init; }

    /// <summary>
    ///     Version/tags to pick the pipeline.
    /// </summary>
    public WorkflowExpression<string>? Version { get; init; }

    /// <summary>
    ///     Branch to pick the default version.
    /// </summary>
    public WorkflowExpression<string>? Branch { get; init; }

    /// <summary>
    ///     Trigger for this pipeline resource.
    /// </summary>
    public PipelineResourceTrigger? Trigger { get; init; }
}

/// <summary>
///     Pipeline resource trigger definition.
/// </summary>
[PublicAPI]
public sealed record PipelineResourceTrigger
{
    /// <summary>
    ///     Enable or disable the pipeline trigger.
    /// </summary>
    public WorkflowExpression<bool>? Enabled { get; init; }

    /// <summary>
    ///     Branches to trigger on.
    /// </summary>
    public IncludeExcludeFilters? Branches { get; init; }

    /// <summary>
    ///     Tags to trigger on.
    /// </summary>
    public IncludeExcludeFilters? Tags { get; init; }

    /// <summary>
    ///     Stages to trigger on.
    /// </summary>
    public WorkflowExpressionCollection<string>? Stages { get; init; }
}

/// <summary>
///     Repository resource definition.
/// </summary>
[PublicAPI]
public sealed record RepositoryResource
{
    /// <summary>
    ///     Identifier for the repository resource.
    /// </summary>
    public required WorkflowExpression<string> Repository { get; init; }

    /// <summary>
    ///     Repository type (e.g., git, github, bitbucket).
    /// </summary>
    public required WorkflowExpression<string> Type { get; init; }

    /// <summary>
    ///     Service connection for the repository.
    /// </summary>
    public WorkflowExpression<string>? Endpoint { get; init; }

    /// <summary>
    ///     Repository name.
    /// </summary>
    public WorkflowExpression<string>? Name { get; init; }

    /// <summary>
    ///     Repository reference (branch, tag, or commit).
    /// </summary>
    public WorkflowExpression<string>? Ref { get; init; }

    /// <summary>
    ///     Trigger for this repository resource.
    /// </summary>
    public Trigger? Trigger { get; init; }
}

/// <summary>
///     Webhook resource definition.
/// </summary>
[PublicAPI]
public sealed record WebhookResource
{
    /// <summary>
    ///     Identifier for the webhook resource.
    /// </summary>
    public required WorkflowExpression<string> Webhook { get; init; }

    /// <summary>
    ///     Service connection for the webhook.
    /// </summary>
    public required WorkflowExpression<string> Connection { get; init; }

    /// <summary>
    ///     Type of webhook.
    /// </summary>
    public WorkflowExpression<string>? Type { get; init; }

    /// <summary>
    ///     Filters for the webhook.
    /// </summary>
    public IReadOnlyList<WebhookFilter>? Filters { get; init; }
}

/// <summary>
///     Webhook filter definition.
/// </summary>
[PublicAPI]
public sealed record WebhookFilter
{
    /// <summary>
    ///     JSON path to filter on.
    /// </summary>
    public required WorkflowExpression<string> Path { get; init; }

    /// <summary>
    ///     Value to match.
    /// </summary>
    public required WorkflowExpression<string> Value { get; init; }
}

/// <summary>
///     Package resource definition.
/// </summary>
[PublicAPI]
public sealed record PackageResource
{
    /// <summary>
    ///     Identifier for the package resource.
    /// </summary>
    public required WorkflowExpression<string> Package { get; init; }

    /// <summary>
    ///     Package type (e.g., NuGet, npm, Maven).
    /// </summary>
    public required WorkflowExpression<string> Type { get; init; }

    /// <summary>
    ///     Service connection for the package feed.
    /// </summary>
    public WorkflowExpression<string>? Connection { get; init; }

    /// <summary>
    ///     Package name.
    /// </summary>
    public WorkflowExpression<string>? Name { get; init; }

    /// <summary>
    ///     Package version.
    /// </summary>
    public WorkflowExpression<string>? Version { get; init; }

    /// <summary>
    ///     Tag for the package.
    /// </summary>
    public WorkflowExpression<string>? Tag { get; init; }
}
