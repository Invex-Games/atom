namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class ExtensionTargetBuild : MinimalBuildDefinition, IBaseExtensionTarget, IExtendedExtensionTarget
{
    public bool BaseExtensionTargetExecuted { get; set; }

    public bool ExtendedExtensionTargetExecuted { get; set; }
}

public interface IBaseExtensionTarget
{
    bool BaseExtensionTargetExecuted { set; }

    Target BaseExtensionTarget =>
        t => t.Executes(() =>
        {
            BaseExtensionTargetExecuted = true;

            return Task.CompletedTask;
        });
}

public interface IExtendedExtensionTarget
{
    bool ExtendedExtensionTargetExecuted { set; }

    Target ExtendedExtensionTarget =>
        t => t
            .Extends<IBaseExtensionTarget>(definition => definition.BaseExtensionTarget)
            .Executes(() =>
            {
                ExtendedExtensionTargetExecuted = true;

                return Task.CompletedTask;
            });
}
