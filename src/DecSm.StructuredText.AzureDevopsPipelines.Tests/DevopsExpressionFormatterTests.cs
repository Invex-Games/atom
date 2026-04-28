namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsExpressionFormatterTests
{
    private readonly DevopsExpressionFormatter _formatter = new();

    // A custom unknown expression type for testing the default case
    private sealed record UnknownExpression : TextExpression;

    // ── Values ────────────────────────────────────────────────────────────────

    [Test]
    public void Format_Null_ReturnsNull() =>
        _formatter
            .Format(null)
            .ShouldBeNull();

    [Test]
    public void Format_RawExpression_ReturnsRawValue() =>
        _formatter
            .Format(new RawExpression("raw-text"))
            .ShouldBe("raw-text");

    [Test]
    public void Format_BooleanExpression_True_ReturnsTrue() =>
        _formatter
            .Format(new BooleanExpression(true))
            .ShouldBe("true");

    [Test]
    public void Format_BooleanExpression_False_ReturnsFalse() =>
        _formatter
            .Format(new BooleanExpression(false))
            .ShouldBe("false");

    [Test]
    public void Format_NullExpression_ReturnsEmptyString() =>
        _formatter
            .Format(new NullExpression())
            .ShouldBe(string.Empty);

    [Test]
    public void Format_NumberExpression_Integer_ReturnsInvariantString() =>
        _formatter
            .Format(new NumberExpression(42))
            .ShouldBe("42");

    [Test]
    public void Format_NumberExpression_Decimal_ReturnsInvariantString() =>
        _formatter
            .Format(new NumberExpression(3.14))
            .ShouldBe("3.14");

    [Test]
    public void Format_StringExpression_WrapsInSingleQuotes() =>
        _formatter
            .Format(new StringExpression("hello world"))
            .ShouldBe("'hello world'");

    [Test]
    public void Format_StringExpression_Empty_WrapsInSingleQuotes() =>
        _formatter
            .Format(new StringExpression(string.Empty))
            .ShouldBe("''");

    // ── DevOps-specific expressions ───────────────────────────────────────────

    [Test]
    public void Format_DevopsMacroExpression_WrapsBrackets() =>
        _formatter
            .Format(new DevopsMacroExpression(new RawExpression("myVar")))
            .ShouldBe("$(myVar)");

    [Test]
    public void Format_DevopsMacroExpression_NestedStringExpression_WrapsInMacroSyntaxWithQuotes() =>
        _formatter
            .Format(new DevopsMacroExpression(new StringExpression("myVar")))
            .ShouldBe("$('myVar')");

    [Test]
    public void Format_DevopsRuntimeExpression_WrapsInSquareBrackets() =>
        _formatter
            .Format(new DevopsRuntimeExpression(new RawExpression("variables.myVar")))
            .ShouldBe("$[ variables.myVar ]");

    [Test]
    public void Format_DevopsRuntimeExpression_NestedStringExpression() =>
        _formatter
            .Format(new DevopsRuntimeExpression(new StringExpression("value")))
            .ShouldBe("$[ 'value' ]");

    // ── Compile-time template expression ─────────────────────────────────────

    [Test]
    public void Format_EvaluateExpression_WrapsInDoubleCurlyBraces() =>
        _formatter
            .Format(new EvaluateExpression(new RawExpression("parameters.name")))
            .ShouldBe("${{ parameters.name }}");

    [Test]
    public void Format_EvaluateExpression_NestedStringExpression() =>
        _formatter
            .Format(new EvaluateExpression(new StringExpression("val")))
            .ShouldBe("${{ 'val' }}");

    // ── Accessors ─────────────────────────────────────────────────────────────

    [Test]
    public void Format_IndexAccessExpression_WrapsIndexInSquareBrackets() =>
        _formatter
            .Format(new IndexAccessExpression(new RawExpression("arr"), new RawExpression("0")))
            .ShouldBe("arr[0]");

    [Test]
    public void Format_IndexAccessExpression_NestedExpressions() =>
        _formatter
            .Format(new IndexAccessExpression(new RawExpression("matrix"), new StringExpression("config")))
            .ShouldBe("matrix['config']");

    [Test]
    public void Format_PropertyAccessExpression_JoinsWithDot() =>
        _formatter
            .Format(new PropertyAccessExpression(new RawExpression("obj"), new RawExpression("prop")))
            .ShouldBe("obj.prop");

    [Test]
    public void Format_PropertyAccessExpression_NestedExpressions() =>
        _formatter
            .Format(new PropertyAccessExpression(new RawExpression("dependencies"), new RawExpression("myJob")))
            .ShouldBe("dependencies.myJob");

    // ── Logic operators ───────────────────────────────────────────────────────

    [Test]
    public void Format_NotExpression_WrapsInNotFunction() =>
        _formatter
            .Format(new NotExpression(new RawExpression("condition")))
            .ShouldBe("not(condition)");

    [Test]
    public void Format_AndExpression_NoElements_ReturnsEmptyString() =>
        _formatter
            .Format(new AndExpression())
            .ShouldBe(string.Empty);

    [Test]
    public void Format_AndExpression_OneElement_ReturnsFormatted() =>
        _formatter
            .Format(new AndExpression(new RawExpression("a")))
            .ShouldBe("a");

    [Test]
    public void Format_AndExpression_TwoElements_WrapsInAndFunction() =>
        _formatter
            .Format(new AndExpression(new RawExpression("a"), new RawExpression("b")))
            .ShouldBe("and(a, b)");

    [Test]
    public void Format_AndExpression_ThreeElements_NestedAndFunction() =>
        _formatter
            .Format(new AndExpression(new RawExpression("a"), new RawExpression("b"), new RawExpression("c")))
            .ShouldBe("and(and(a, b), c)");

    [Test]
    public void Format_OrExpression_NoElements_ReturnsEmptyString() =>
        _formatter
            .Format(new OrExpression())
            .ShouldBe(string.Empty);

    [Test]
    public void Format_OrExpression_OneElement_ReturnsFormatted() =>
        _formatter
            .Format(new OrExpression(new RawExpression("a")))
            .ShouldBe("a");

    [Test]
    public void Format_OrExpression_TwoElements_WrapsInOrFunction() =>
        _formatter
            .Format(new OrExpression(new RawExpression("a"), new RawExpression("b")))
            .ShouldBe("or(a, b)");

    [Test]
    public void Format_OrExpression_ThreeElements_NestedOrFunction() =>
        _formatter
            .Format(new OrExpression(new RawExpression("a"), new RawExpression("b"), new RawExpression("c")))
            .ShouldBe("or(or(a, b), c)");

    [Test]
    public void Format_EqualExpression_UsesEqFunction() =>
        _formatter
            .Format(new EqualExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("eq(left, right)");

    [Test]
    public void Format_NotEqualExpression_UsesNeFunction() =>
        _formatter
            .Format(new NotEqualExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("ne(left, right)");

    [Test]
    public void Format_LessThanExpression_UsesLtFunction() =>
        _formatter
            .Format(new LessThanExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("lt(left, right)");

    [Test]
    public void Format_LessThanOrEqualToExpression_UsesLeFunction() =>
        _formatter
            .Format(new LessThanOrEqualToExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("le(left, right)");

    [Test]
    public void Format_GreaterThanExpression_UsesGtFunction() =>
        _formatter
            .Format(new GreaterThanExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("gt(left, right)");

    [Test]
    public void Format_GreaterThanOrEqualToExpression_UsesGeFunction() =>
        _formatter
            .Format(new GreaterThanOrEqualToExpression(new RawExpression("left"), new RawExpression("right")))
            .ShouldBe("ge(left, right)");

    // ── Functions ─────────────────────────────────────────────────────────────

    [Test]
    public void Format_ContainsExpression_UsesContainsFunction() =>
        _formatter
            .Format(new ContainsExpression(new RawExpression("source"), new RawExpression("pattern")))
            .ShouldBe("contains(source, pattern)");

    [Test]
    public void Format_CoalesceExpression_NoElements_ReturnsEmptyString() =>
        _formatter
            .Format(new CoalesceExpression())
            .ShouldBe(string.Empty);

    [Test]
    public void Format_CoalesceExpression_OneElement_ReturnsFormatted() =>
        _formatter
            .Format(new CoalesceExpression(new RawExpression("a")))
            .ShouldBe("a");

    [Test]
    public void Format_CoalesceExpression_TwoElements_WrapsInCoalesceFunction() =>
        _formatter
            .Format(new CoalesceExpression(new RawExpression("a"), new RawExpression("b")))
            .ShouldBe("coalesce(a, b)");

    [Test]
    public void Format_CoalesceExpression_ThreeElements_NestedCoalesceFunction() =>
        _formatter
            .Format(new CoalesceExpression(new RawExpression("a"), new RawExpression("b"), new RawExpression("c")))
            .ShouldBe("coalesce(coalesce(a, b), c)");

    [Test]
    public void Format_StartsWithExpression_UsesStartsWithFunction() =>
        _formatter
            .Format(new StartsWithExpression(new RawExpression("source"), new RawExpression("pattern")))
            .ShouldBe("startsWith(source, pattern)");

    [Test]
    public void Format_EndsWithExpression_UsesEndsWithFunction() =>
        _formatter
            .Format(new EndsWithExpression(new RawExpression("source"), new RawExpression("pattern")))
            .ShouldBe("endsWith(source, pattern)");

    [Test]
    public void Format_FormatExpression_ZeroArgs_ReturnsSource() =>
        _formatter
            .Format(new FormatExpression(new RawExpression("source")))
            .ShouldBe("source");

    [Test]
    public void Format_FormatExpression_OneArg_UsesFormatFunction() =>
        _formatter
            .Format(new FormatExpression(new StringExpression("{0} world"), new StringExpression("hello")))
            .ShouldBe("format('{0} world', 'hello')");

    [Test]
    public void Format_FormatExpression_TwoArgs_UsesFormatFunctionWithAllArgs() =>
        _formatter
            .Format(new FormatExpression(new StringExpression("{0}-{1}"),
                new RawExpression("a"),
                new RawExpression("b")))
            .ShouldBe("format('{0}-{1}', a, b)");

    [Test]
    public void Format_JoinExpression_NoSeparator_UsesJoinFunctionWithSourceOnly() =>
        _formatter
            .Format(new JoinExpression(new RawExpression("myArray"), null))
            .ShouldBe("join(myArray)");

    [Test]
    public void Format_JoinExpression_WithSeparator_UsesJoinFunctionWithSeparator() =>
        _formatter
            .Format(new JoinExpression(new RawExpression("myArray"), new StringExpression(", ")))
            .ShouldBe("join(myArray, ', ')");

    [Test]
    public void Format_ToJsonExpression_UsesConvertToJsonFunction() =>
        _formatter
            .Format(new ToJsonExpression(new RawExpression("myObject")))
            .ShouldBe("convertToJson(myObject)");

    [Test]
    public void Format_HashFilesExpression_UsesHashFilesFunction() =>
        _formatter
            .Format(new HashFilesExpression(new RawExpression("**/package-lock.json")))
            .ShouldBe("hashFiles(**/package-lock.json)");

    // ── Workflow expressions ───────────────────────────────────────────────────

    [Test]
    public void Format_TargetOutputExpression_WithOutputName_UsesDependenciesOutputsFormat() =>
        _formatter
            .Format(new TargetOutputExpression
            {
                TargetName = "BuildJob",
                OutputName = "ArtifactPath",
            })
            .ShouldBe("dependencies.BuildJob.outputs['BuildJob.ArtifactPath']");

    [Test]
    public void Format_TargetOutputExpression_EmptyOutputName_UsesDependenciesOutputsWithoutKey() =>
        _formatter
            .Format(new TargetOutputExpression
            {
                TargetName = "BuildJob",
                OutputName = string.Empty,
            })
            .ShouldBe("dependencies.BuildJob.outputs");

    [Test]
    public void Format_TargetOutputExpression_NullOutputName_UsesDependenciesOutputsWithoutKey() =>
        _formatter
            .Format(new TargetOutputExpression
            {
                TargetName = "BuildJob",
                OutputName = null,
            })
            .ShouldBe("dependencies.BuildJob.outputs");

    [Test]
    public void Format_TargetOutcomeExpression_UsesDependenciesResult() =>
        _formatter
            .Format(new TargetOutcomeExpression
            {
                Target = "TestJob",
            })
            .ShouldBe("dependencies.TestJob.result");

    [Test]
    public void Format_TargetOutcomeTypeExpression_Success_ReturnsSucceeded() =>
        _formatter
            .Format(new TargetOutcomeTypeExpression
            {
                Type = TargetOutcomeTypeExpression.OutcomeType.Success,
            })
            .ShouldBe("'Succeeded'");

    [Test]
    public void Format_TargetOutcomeTypeExpression_Failure_ReturnsFailed() =>
        _formatter
            .Format(new TargetOutcomeTypeExpression
            {
                Type = TargetOutcomeTypeExpression.OutcomeType.Failure,
            })
            .ShouldBe("'Failed'");

    [Test]
    public void Format_TargetOutcomeTypeExpression_Cancelled_ReturnsCanceled() =>
        _formatter
            .Format(new TargetOutcomeTypeExpression
            {
                Type = TargetOutcomeTypeExpression.OutcomeType.Cancelled,
            })
            .ShouldBe("'Canceled'");

    [Test]
    public void Format_StepOutputExpression_UsesStepsOutputsFormat() =>
        _formatter
            .Format(new StepOutputExpression
            {
                StepName = "myStep",
                OutputName = "myOutput",
            })
            .ShouldBe("steps.myStep.outputs['myOutput']");

    [Test]
    public void Format_StepOutcomeExpression_UsesStepsOutcomeFormat() =>
        _formatter
            .Format(new StepOutcomeExpression
            {
                StepName = "myStep",
            })
            .ShouldBe("steps.myStep.outcome");

    [Test]
    public void Format_StepOutcomeTypeExpression_Success_ReturnsSucceeded() =>
        _formatter
            .Format(new StepOutcomeTypeExpression
            {
                Type = StepOutcomeTypeExpression.OutcomeType.Success,
            })
            .ShouldBe("'Succeeded'");

    [Test]
    public void Format_StepOutcomeTypeExpression_Failure_ReturnsFailed() =>
        _formatter
            .Format(new StepOutcomeTypeExpression
            {
                Type = StepOutcomeTypeExpression.OutcomeType.Failure,
            })
            .ShouldBe("'Failed'");

    [Test]
    public void Format_StepOutcomeTypeExpression_Cancelled_ReturnsCanceled() =>
        _formatter
            .Format(new StepOutcomeTypeExpression
            {
                Type = StepOutcomeTypeExpression.OutcomeType.Cancelled,
            })
            .ShouldBe("'Canceled'");

    [Test]
    public void Format_StepOutcomeTypeExpression_Skipped_ReturnsSkipped() =>
        _formatter
            .Format(new StepOutcomeTypeExpression
            {
                Type = StepOutcomeTypeExpression.OutcomeType.Skipped,
            })
            .ShouldBe("'Skipped'");

    // ── WorkflowExpression<T> overload ────────────────────────────────────────

    [Test]
    public void Format_WorkflowExpression_Null_ReturnsNull()
    {
        WorkflowExpression<string>? typed = null;

        _formatter
            .Format(typed)
            .ShouldBeNull();
    }

    [Test]
    public void Format_WorkflowExpression_WithValue_DelegatesToFormat()
    {
        WorkflowExpression<string>? typed = new RawExpression("workflow-value");

        _formatter
            .Format(typed)
            .ShouldBe("workflow-value");
    }

    // ── Error cases ───────────────────────────────────────────────────────────

    [Test]
    public void Format_UnknownExpression_ThrowsArgumentOutOfRangeException() =>
        Should.Throw<ArgumentOutOfRangeException>(() => _formatter.Format(new UnknownExpression()));

    // ── Nested / combined expressions ─────────────────────────────────────────

    [Test]
    public void Format_NestedExpressions_ComposedCorrectly()
    {
        // eq(dependencies.MyJob.result, 'Succeeded')
        var expr = new EqualExpression(new TargetOutcomeExpression
            {
                Target = "MyJob",
            },
            new TargetOutcomeTypeExpression
            {
                Type = TargetOutcomeTypeExpression.OutcomeType.Success,
            });

        _formatter
            .Format(expr)
            .ShouldBe("eq(dependencies.MyJob.result, 'Succeeded')");
    }

    [Test]
    public void Format_DevopsMacroExpression_WithNestedRuntimeExpression_FormatsInsideMacro()
    {
        // $(variables['myVar']) would be expressed via nested expressions:
        var expr = new DevopsMacroExpression(new IndexAccessExpression(new RawExpression("variables"),
            new StringExpression("myVar")));

        _formatter
            .Format(expr)
            .ShouldBe("$(variables['myVar'])");
    }

    [Test]
    public void Format_EvaluateExpression_WithAndExpression_FormatsCorrectly()
    {
        var expr = new EvaluateExpression(new AndExpression(new RawExpression("eq(variables.A, 'true')"),
            new RawExpression("eq(variables.B, 'true')")));

        _formatter
            .Format(expr)
            .ShouldBe("${{ and(eq(variables.A, 'true'), eq(variables.B, 'true')) }}");
    }
}
