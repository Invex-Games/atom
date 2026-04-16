namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public interface IAdditionalStepOption : IWorkflowOption
{
    /// <summary>
    ///     The order that the step should be executed within the job.
    ///     Values less than 0 are run before the target, and values greater than 0 are run after the target.
    ///     A value of 0 is invalid.
    /// </summary>
    int Order { get; }
}
