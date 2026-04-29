namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterVariablesTests
{
    private static DevopsPipeline.DevopsPipelineWithSteps PipelineWithStepAndVariables(Variables? variables) =>
        new()
        {
            Variables = variables,
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

    [Test]
    public void WriteVariables_Dictionary_WritesKeyValuePairs()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.Dictionary
        {
            Values = new Dictionary<string, TextExpression>
            {
                ["VAR1"] = new RawExpression("value1"),
                ["VAR2"] = new RawExpression("value2"),
            },
        }));

        output.ShouldContain("variables:");
        output.ShouldContain("VAR1: value1");
        output.ShouldContain("VAR2: value2");
    }

    [Test]
    public void WriteVariables_Dictionary_WithStringExpression_WritesQuotedValues()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.Dictionary
        {
            Values = new Dictionary<string, TextExpression>
            {
                ["MY_VAR"] = new StringExpression("hello world"),
            },
        }));

        output.ShouldContain("MY_VAR: 'hello world'");
    }

    [Test]
    public void WriteVariables_VariableList_Name_WritesNameValuePair()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Name
                {
                    VariableName = new RawExpression("MY_VAR"),
                    Value = new RawExpression("my-value"),
                },
            ],
        }));

        output.ShouldContain("variables:");
        output.ShouldContain("- name: MY_VAR");
        output.ShouldContain("value: my-value");
    }

    [Test]
    public void WriteVariables_VariableList_Name_WithReadOnly_WritesReadonlyProperty()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Name
                {
                    VariableName = new RawExpression("READONLY_VAR"),
                    Value = new RawExpression("locked"),
                    ReadOnly = new BooleanExpression(true),
                },
            ],
        }));

        output.ShouldContain("readonly: true");
    }

    [Test]
    public void WriteVariables_VariableList_Name_WithoutReadOnly_DoesNotWriteReadonly()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Name
                {
                    VariableName = new RawExpression("MY_VAR"),
                    Value = new RawExpression("value"),
                },
            ],
        }));

        output.ShouldNotContain("readonly:");
    }

    [Test]
    public void WriteVariables_VariableList_Group_WritesGroupReference()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Group
                {
                    GroupName = new RawExpression("my-variable-group"),
                },
            ],
        }));

        output.ShouldContain("variables:");
        output.ShouldContain("- group: my-variable-group");
    }

    [Test]
    public void WriteVariables_VariableList_Template_NoParams_WritesTemplatePath()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Template
                {
                    TemplatePath = new RawExpression("templates/vars.yml"),
                },
            ],
        }));

        output.ShouldContain("variables:");
        output.ShouldContain("- template: templates/vars.yml");
    }

    [Test]
    public void WriteVariables_VariableList_Template_WithParams_WritesTemplateWithParameters()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Template
                {
                    TemplatePath = new RawExpression("templates/vars.yml"),
                    Parameters = new Dictionary<string, TextExpression>
                    {
                        ["env"] = new RawExpression("production"),
                    },
                },
            ],
        }));

        output.ShouldContain("- template: templates/vars.yml");
        output.ShouldContain("parameters:");
        output.ShouldContain("env: production");
    }

    [Test]
    public void WriteVariables_VariableList_Mixed_WritesAllTypes()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(new Variables.VariableList
        {
            Values =
            [
                new Variable.Name
                {
                    VariableName = new RawExpression("MY_VAR"),
                    Value = new RawExpression("val"),
                },
                new Variable.Group
                {
                    GroupName = new RawExpression("shared-group"),
                },
                new Variable.Template
                {
                    TemplatePath = new RawExpression("templates/vars.yml"),
                },
            ],
        }));

        output.ShouldContain("- name: MY_VAR");
        output.ShouldContain("- group: shared-group");
        output.ShouldContain("- template: templates/vars.yml");
    }

    [Test]
    public void WriteVariables_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(PipelineWithStepAndVariables(null));
        output.ShouldNotContain("variables:");
    }

    // ── Pool ──────────────────────────────────────────────────────────────────

    [Test]
    public void WritePool_PoolName_WritesSimplePoolProperty()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = new Pool.PoolName
            {
                Name = new RawExpression("my-private-pool"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("pool: my-private-pool");
    }

    [Test]
    public void WritePool_PoolSpec_WithVmImage_WritesPoolSection()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = new Pool.PoolSpec
            {
                VmImage = new RawExpression("ubuntu-latest"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("pool:");
        output.ShouldContain("vmImage: ubuntu-latest");
    }

    [Test]
    public void WritePool_PoolSpec_WithName_WritesPoolNameInSection()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = new Pool.PoolSpec
            {
                Name = new RawExpression("my-pool"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("pool:");
        output.ShouldContain("name: my-pool");
    }

    [Test]
    public void WritePool_PoolSpec_WithDemands_WritesDemandsList()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = new Pool.PoolSpec
            {
                Name = new RawExpression("my-pool"),
                Demands = new[] { "agent.os -equals Windows_NT", "vstest" },
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("demands:");
        output.ShouldContain("- agent.os -equals Windows_NT");
        output.ShouldContain("- vstest");
    }

    [Test]
    public void WritePool_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = null,
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldNotContain("pool");
    }
}
