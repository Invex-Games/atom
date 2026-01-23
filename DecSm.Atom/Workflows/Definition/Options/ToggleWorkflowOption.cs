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
/// var options = new List&lt;IWorkflowOption&gt; { new UseCustomFeature { Value = true } };
/// bool isEnabled = options.HasEnabledToggle&lt;UseCustomFeature&gt;(); // true
///     </code>
/// </example>
[PublicAPI]
public abstract record ToggleWorkflowOption<TSelf> : IWorkflowOption
    where TSelf : ToggleWorkflowOption<TSelf>
{
    public bool Value { get; init; }
}

[PublicAPI]
public static class ToggleWorkflowOptionExtensions
{
    extension(IEnumerable<IWorkflowOption> options)
    {
        [PublicAPI]
        public bool HasEnabledToggle<T>()
            where T : ToggleWorkflowOption<T> =>
            options
                .OfType<T>()
                .Any(x => x.Value);
    }
}
