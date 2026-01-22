namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

/// <summary>
///     Represents a workflow option for specifying a custom snapshot image for GitHub Actions runners.
/// </summary>
/// <param name="ImageName">The name of the Docker image to use (e.g., "ubuntu-22.04").</param>
/// <param name="Version">An optional version tag for the Docker image (e.g., "latest", "20231026").</param>
/// <remarks>
///     This option allows users to define a specific Docker image and an optional version
///     to be used as the runner environment for a GitHub Actions job, enabling custom
///     toolchains or environments.
/// </remarks>
[PublicAPI]
public sealed record GithubSnapshotImageOption(string ImageName, string? Version) : IWorkflowOption;
