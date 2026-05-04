namespace DecSm.StructuredText.AzureDevopsPipelines.DevopsPipelineModel.Supporting;

/// <summary>
///     Specifies which pool to use for a job of the pipeline.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - PoolName: Specify a private pool by name (simple string syntax)
///     - PoolSpec: Full syntax for using demands and Microsoft-hosted pools
/// </remarks>
[PublicAPI]
[Union]
public partial record Pool
{
    /// <summary>
    ///     Specify a private pool by name.
    /// </summary>
    [PublicAPI]
    public sealed partial record PoolName
    {
        /// <summary>
        ///     Name of the pool.
        /// </summary>
        public required WorkflowExpression<string> Name { get; init; }
    }

    /// <summary>
    ///     Full syntax for using demands and Microsoft-hosted pools.
    /// </summary>
    [PublicAPI]
    public sealed partial record PoolSpec
    {
        /// <summary>
        ///     Name of a pool.
        /// </summary>
        public WorkflowExpression<string>? Name { get; init; }

        /// <summary>
        ///     Demands (for a private pool).
        /// </summary>
        public WorkflowExpressionCollection<string>? Demands { get; init; }

        /// <summary>
        ///     Name of the VM image you want to use; valid only in the Microsoft-hosted pool.
        /// </summary>
        public WorkflowExpression<string>? VmImage { get; init; }
    }
}
