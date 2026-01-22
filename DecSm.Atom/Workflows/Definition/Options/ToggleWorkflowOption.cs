namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     An abstract base record for creating workflow options that act as boolean toggles.
/// </summary>
/// <typeparam name="TSelf">The concrete type implementing this toggle option.</typeparam>
/// <remarks>
///     This class provides a foundation for creating boolean workflow options with a helper method for checking the
///     enabled state.
/// </remarks>
/// <example>
///     <code>
/// public sealed record UseCustomFeature : ToggleWorkflowOption&lt;UseCustomFeature&gt;;
/// // Usage:
/// var options = new List&lt;IWorkflowOption&gt; { UseCustomFeature.Enabled };
/// bool isEnabled = UseCustomFeature.IsEnabled(options); // true
///     </code>
/// </example>
[PublicAPI]
public abstract record ToggleWorkflowOption<TSelf> : IWorkflowOption
    where TSelf : ToggleWorkflowOption<TSelf>
{
    public bool Value { get; init; }

    /// <summary>
    ///     Determines whether this toggle option is enabled within a collection of workflow options.
    /// </summary>
    /// <param name="options">The collection of workflow options to check.</param>
    /// <returns><c>true</c> if an enabled instance of this option exists in the collection; otherwise, <c>false</c>.</returns>
#pragma warning disable RCS1158
    public static bool IsEnabled(IEnumerable<IWorkflowOption> options) =>
        options
            .OfType<TSelf>()
            .Any(x => x.Value);
#pragma warning restore RCS1158
}
