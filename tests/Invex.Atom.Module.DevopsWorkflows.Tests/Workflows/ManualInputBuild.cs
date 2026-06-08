namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ManualInputBuild : WorkflowBuildDefinition, IDevopsWorkflows, IManualInputTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("manual-input-workflow")
        {
            Triggers =
            [
                new ManualTrigger
                {
                    Inputs =
                    [
                        ManualStringInput.ForParam(
                            ParamDefinitions[nameof(IManualInputTarget.StringParamWithoutDefault)]),
                        ManualStringInput.ForParam(
                            ParamDefinitions[nameof(IManualInputTarget.StringParamWithDefault)]),
                        ManualBoolInput.ForParam(
                            ParamDefinitions[nameof(IManualInputTarget.BoolParamWithoutDefault)]),
                        ManualBoolInput.ForParam(ParamDefinitions[nameof(IManualInputTarget.BoolParamWithDefault)]),
                        ManualChoiceInput.ForParam(
                            ParamDefinitions[nameof(IManualInputTarget.ChoiceParamWithoutDefault)],
                            ["choice 1", "choice 2", "choice 3"]),
                        ManualChoiceInput.ForParam(
                            ParamDefinitions[nameof(IManualInputTarget.ChoiceParamWithDefault)],
                            ["choice 1", "choice 2", "choice 3"]),
                    ],
                },
            ],
            Targets = [new(nameof(IManualInputTarget.ManualInputTarget))],
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}

public interface IManualInputTarget : IBuildAccessor
{
    [ParamDefinition("string-param-without-default", "String param")]
    string StringParamWithoutDefault => GetParam(() => StringParamWithoutDefault)!;

    [ParamDefinition("string-param-with-default", "String param")]
    string StringParamWithDefault => GetParam(() => StringParamWithDefault, "default-value");

    [ParamDefinition("bool-param-without-default", "Bool param")]
    bool? BoolParamWithoutDefault => GetParam(() => BoolParamWithoutDefault);

    [ParamDefinition("bool-param-with-default", "Bool param")]
    bool BoolParamWithDefault => GetParam(() => BoolParamWithDefault, true);

    [ParamDefinition("choice-param-without-default", "Choice param")]
    string ChoiceParamWithoutDefault => GetParam(() => ChoiceParamWithoutDefault)!;

    [ParamDefinition("choice-param-with-default", "Choice param")]
    string ChoiceParamWithDefault => GetParam(() => ChoiceParamWithDefault, "choice 1");

    Target ManualInputTarget => t => t;
}
