namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ManualInputStabilityBuild : BuildDefinition, IGithubWorkflows, IManualInputStabilityTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("manual-input-stability-workflow")
        {
            Triggers =
            [
                new ManualTrigger
                {
                    Inputs =
                    [
                        ManualStringInput.ForParam(WorkflowParams.StringParamWithoutDefault),
                        ManualStringInput.ForParam(WorkflowParams.StringParamWithDefault),
                        ManualBoolInput.ForParam(WorkflowParams.BoolParamWithoutDefault),
                        ManualBoolInput.ForParam(WorkflowParams.BoolParamWithDefault),
                        ManualChoiceInput.ForParam(WorkflowParams.ChoiceParamWithoutDefault,
                            ["choice 1", "choice 2", "choice 3"]),
                        ManualChoiceInput.ForParam(WorkflowParams.ChoiceParamWithDefault,
                            ["choice 1", "choice 2", "choice 3"]),
                    ],
                },
            ],
            Targets = [WorkflowTargets.ManualInputTarget],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

public interface IManualInputStabilityTarget : IBuildAccessor
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
