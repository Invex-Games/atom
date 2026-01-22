namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ManualInputBuild : MinimalBuildDefinition, IGithubWorkflows, IManualInputTarget
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
                        ManualStringInput.ForParam(ParamDefinitions[WorkflowParams.StringParamWithoutDefault]),
                        ManualStringInput.ForParam(ParamDefinitions[WorkflowParams.StringParamWithDefault]),
                        ManualBoolInput.ForParam(ParamDefinitions[WorkflowParams.BoolParamWithoutDefault]),
                        ManualBoolInput.ForParam(ParamDefinitions[WorkflowParams.BoolParamWithDefault]),
                        ManualChoiceInput.ForParam(ParamDefinitions[WorkflowParams.ChoiceParamWithoutDefault],
                            ["choice 1", "choice 2", "choice 3"]),
                        ManualChoiceInput.ForParam(ParamDefinitions[WorkflowParams.ChoiceParamWithDefault],
                            ["choice 1", "choice 2", "choice 3"]),
                    ],
                },
            ],
            Targets = [WorkflowTargets.ManualInputTarget],
            WorkflowTypes = [Github.WorkflowType],
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
