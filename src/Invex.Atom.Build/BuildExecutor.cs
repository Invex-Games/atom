namespace Invex.Atom.Build;

/// <summary>
///     Responsible for executing build targets based on command-line arguments and the build model.
/// </summary>
/// <param name="args">The parsed command-line arguments.</param>
/// <param name="buildModel">The model representing the build structure and state.</param>
/// <param name="paramService">The service for resolving parameters.</param>
/// <param name="variableService">The service for managing workflow variables.</param>
/// <param name="outcomeReporters">The collection of report writers to invoke after execution.</param>
/// <param name="console">The console for writing output.</param>
/// <param name="reportService">The service for collecting report data.</param>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class BuildExecutor(
    CommandLineArgs args,
    BuildModel buildModel,
    IParamService paramService,
    IVariableService variableService,
    IEnumerable<IOutcomeReportWriter> outcomeReporters,
    IAnsiConsole console,
    ReportService reportService,
    ILogger<BuildExecutor> logger
)
{
    /// <summary>
    ///     Executes the build by running the specified targets, and then generates outcome reports.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <exception cref="StepFailedException">Thrown if the build fails.</exception>
    public async Task Execute(CancellationToken cancellationToken)
    {
        var commands = args.Commands;

        if (commands is { Count: 0 })
        {
            logger.LogInformation("No targets specified; execution skipped");

            return;
        }

        console.WriteLine();
        logger.LogInformation("Executing build");

        foreach (var command in commands)
            ValidateTargetParameters(buildModel.GetTarget(command.Name));

        foreach (var command in commands)
            await ExecuteTarget(buildModel.GetTarget(command.Name), cancellationToken);

        foreach (var outcomeReporter in outcomeReporters)
            try
            {
                await outcomeReporter.ReportRunOutcome(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while reporting run outcome");
            }

        if (buildModel.TargetStates.Values.Any(state => state.Status is TargetRunState.Failed or TargetRunState.NotRun))
        {
            Environment.ExitCode = 1;

            throw new StepFailedException("Build failed");
        }
    }

    /// <summary>
    ///     Validates that all required parameters for a given target have been provided.
    /// </summary>
    /// <param name="target">The target to validate.</param>
    private void ValidateTargetParameters(TargetModel target)
    {
        var missingParams = new List<UsedParam>();

        foreach (var requiredParam in target.Params.Where(x => x.Required))
        {
            var defaultValue = requiredParam.Param.DefaultValue is { Length: > 0 }
                ? requiredParam.Param.DefaultValue
                : null;

            string? value;

            if (requiredParam.Param.IsSecret)
            {
                value = paramService.GetParam(requiredParam.Param.Name, defaultValue, x => x);
            }
            else
            {
                using var _ = paramService.CreateNoCacheScope();
                value = paramService.GetParam(requiredParam.Param.Name, defaultValue);
            }

            if (value is { Length: > 0 })
                continue;

            missingParams.Add(requiredParam);
        }

        if (missingParams.Count == 0)
            return;

        foreach (var requiredParam in missingParams)
            logger.LogError("""
                            Missing required parameter '{ParamName}' for target {TargetName}.
                            You can provide it via:
                            Command line:           --{ArgName} <value>,
                            Environment variable:   {EnvVarName},
                            appsettings.json file:  Params:{ConfigParamName},
                            Interactive mode:       -i or --interactive
                            """,
                requiredParam.Param.ArgName,
                target.Name,
                requiredParam.Param.ArgName,
                requiredParam.Param.EnvVarName,
                requiredParam.Param.Name);

        buildModel.GetTargetState(target)
            .Status = TargetRunState.Failed;
    }

    /// <summary>
    ///     Recursively executes a target and its dependencies.
    /// </summary>
    /// <param name="target">The target to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    private async Task ExecuteTarget(TargetModel target, CancellationToken cancellationToken)
    {
        var targetState = buildModel.GetTargetState(target);

        if (targetState.Status is TargetRunState.NotRun
            or TargetRunState.Skipped
            or TargetRunState.Succeeded
            or TargetRunState.Failed)
            return;

        if (targetState.Status is not TargetRunState.PendingRun)
        {
            logger.LogWarning("Skipping target {TargetDefinitionName} due to unexpected state {TargetState}",
                target.Name,
                targetState.Status);

            return;
        }

        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(dependency, cancellationToken);

        if (target.Dependencies.Any(depTarget => buildModel.GetTargetState(depTarget)
                .Status is TargetRunState.Failed))
        {
            targetState.Status = TargetRunState.NotRun;

            logger.LogWarning("Skipping target {TargetDefinitionName} due to failed dependencies", target.Name);

            return;
        }

        foreach (var variable in target.ConsumedVariables)
        {
            await variableService.ReadVariable(variable.TargetName, variable.VariableName, cancellationToken);

            if (paramService.GetParam(variable.VariableName) is not (null or ""))
                continue;

            logger.LogError("Missing required variable '{VariableName}' from target {FromTarget} for target {Target}",
                variable,
                variable.TargetName,
                target.Name);

            targetState.Status = TargetRunState.Failed;

            return;
        }

        targetState.Status = TargetRunState.Running;

        var startTime = Stopwatch.GetTimestamp();

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["Command"] = target.Name,
               }))
        {
            if (args.HasHeadless)
            {
                console.WriteLine($"##[group]{target.Name}");
            }
            else
            {
                console.Write(new Markup($"[underline]{target.Name}[/]"));
                console.WriteLine();
                console.WriteLine();
            }

            if (target.Description is { Length: > 0 })
            {
                console.Write(new Markup($"[dim]{target.Description}[/]"));
                console.WriteLine();
                console.WriteLine();
            }

            try
            {
                foreach (var task in target.Tasks)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException("Build execution was cancelled", cancellationToken);

                    await task(cancellationToken);
                }

                targetState.Status = TargetRunState.Succeeded;
            }
            catch (StepFailedException failedCheckException)
            {
                logger.LogInformation(failedCheckException,
                    "A check failed for target {TargetDefinitionName}",
                    target.Name);

                targetState.Status = TargetRunState.Failed;

                reportService.AddReportData(new TextReportData(failedCheckException.Message)
                {
                    Title = $"Check failed in {target.Name}",
                });

                if (failedCheckException.ReportData is not null)
                    reportService.AddReportData(failedCheckException.ReportData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing target {TargetDefinitionName}", target.Name);

                targetState.Status = TargetRunState.Failed;
            }

            targetState.RunDuration =
                TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency);

            if (args.HasHeadless)
                console.WriteLine("##[endgroup]");
        }
    }
}
