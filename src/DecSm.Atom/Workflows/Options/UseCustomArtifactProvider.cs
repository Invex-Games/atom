namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     A workflow option that enables a custom <see cref="IArtifactProvider" /> for artifact management.
/// </summary>
/// <remarks>
///     <para>
///         When <see cref="ToggleWorkflowOption{TSelf}.Value" />, the workflow will use the
///         <see cref="IStoreArtifact.StoreArtifact" /> and <see cref="IRetrieveArtifact.RetrieveArtifact" /> targets,
///         which delegate to the registered <see cref="IArtifactProvider" /> implementation.
///     </para>
///     <para>
///         If this option is disabled, the framework defaults to the native artifact capabilities of the CI/CD platform
///         (e.g., <c>actions/upload-artifact</c> in GitHub Actions).
///     </para>
///     <para>
///         Enabling this option ensures a consistent artifact management strategy across different CI/CD platforms and
///         local
///         builds.
///     </para>
/// </remarks>
/// <example>
///     To enable a custom artifact provider in a workflow:
///     <code>
/// // In your build definition:
/// public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
/// [
///     new("MyWorkflow")
///     {
///         Options = [UseCustomArtifactProvider.Enabled],
///         // ... other workflow configurations
///     }
/// ];
/// </code>
/// </example>
/// <seealso cref="IArtifactProvider" />
/// <seealso cref="IStoreArtifact" />
/// <seealso cref="IRetrieveArtifact" />
[PublicAPI]
public sealed record UseCustomArtifactProvider : ToggleWorkflowOption<UseCustomArtifactProvider>;
