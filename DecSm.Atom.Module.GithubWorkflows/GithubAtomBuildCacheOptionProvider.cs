namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public sealed record UseGithubForAtomBuildCache : ToggleWorkflowOption<UseGithubForAtomBuildCache>
{
    public static void WriteSaveStep(GithubStepWriter writer, WorkflowCacheSaveOption cacheOptions)
    {
        using (writer.WriteSection($"- name: {cacheOptions.StepId}"))
        {
            writer.WriteLine($"id: {cacheOptions.StepId}");
            writer.WriteLine("uses: actions/cache/save@v4");

            var runIfExpression = cacheOptions.RunIf is not null
                ? cacheOptions.RunOnlyIfMatchingNameCacheMissed
                    ? cacheOptions.RunIf.And(WorkflowExpressions
                        .Literal(
                            $"steps.cache-restore-{WorkflowCacheUtil.ConvertNameToId(cacheOptions.Name)}.outputs.cache-hit")
                        .NotEqualTo(true))
                    : cacheOptions.RunIf
                : cacheOptions.RunOnlyIfMatchingNameCacheMissed
                    ? WorkflowExpressions
                        .Literal(
                            $"steps.cache-restore-{WorkflowCacheUtil.ConvertNameToId(cacheOptions.Name)}.outputs.cache-hit")
                        .NotEqualTo(true)
                    : null;

            if (runIfExpression is not null)
                writer.WriteLine($"if: {writer.WorkflowExpressionGenerator.Write(runIfExpression)}");

            using (writer.WriteSection("with:"))
            {
                if (cacheOptions.Paths.Count > 0)
                    using (writer.WriteSection("path: |"))
                    {
                        foreach (var path in cacheOptions.Paths)
                            writer.WriteLine(writer.WorkflowExpressionGenerator.Write(path));
                    }

                writer.WriteLine($"key: {writer.WorkflowExpressionGenerator.Write(cacheOptions.Key)}");
            }
        }
    }

    public static void WriteRestoreStep(GithubStepWriter writer, WorkflowCacheRestoreOption cacheOptions)
    {
        using (writer.WriteSection($"- name: {cacheOptions.StepId}"))
        {
            writer.WriteLine($"id: {cacheOptions.StepId}");
            writer.WriteLine("uses: actions/cache/restore@v4");

            if (cacheOptions.RunIf is not null)
                writer.WriteLine($"if: {writer.WorkflowExpressionGenerator.Write(cacheOptions.RunIf)}");

            using (writer.WriteSection("with:"))
            {
                if (cacheOptions.Paths.Count > 0)
                    using (writer.WriteSection("path: |"))
                    {
                        foreach (var path in cacheOptions.Paths)
                            writer.WriteLine(writer.WorkflowExpressionGenerator.Write(path));
                    }

                writer.WriteLine($"key: {writer.WorkflowExpressionGenerator.Write(cacheOptions.Key)}");
            }
        }
    }
}

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class UseGithubForAtomBuildCacheOptions
{
    extension(WorkflowCacheOptions.Options _)
    {
        [PublicAPI]
        public UseGithubForAtomBuildCache UseGithubActionsCaching =>
            new()
            {
                Value = true,
            };
    }
}
