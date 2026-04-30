namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public interface IAdditionalStepOption : IBuildOption
{
    /// <summary>
    ///     Indicates whether the step is enabled.
    ///     When set to true, the step is processed as part of the workflow; otherwise, it is ignored.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     The order that the step should be executed within the job.
    ///     Values less than 0 are run before the target, and values greater than 0 are run after the target.
    ///     A value of 0 is invalid.
    /// </summary>
    int Order { get; }
}
