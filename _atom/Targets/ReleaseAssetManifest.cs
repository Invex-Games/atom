namespace Atom.Targets;

internal static class ReleaseAssetManifest
{
    internal static IReadOnlyList<string> CreateArtifactNames(
        IEnumerable<string> packageProjects,
        string toolProject,
        IEnumerable<string> testProjects) =>
        packageProjects
            .Append(toolProject)
            .Concat(testProjects)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    internal static IReadOnlyList<string> CreateExpectedAssetNames(IEnumerable<string> artifactNames) =>
        artifactNames
            .Select(name => $"{name}.zip")
            .Order(StringComparer.Ordinal)
            .ToArray();

    internal static void ValidateUploadedAssets(
        IEnumerable<string> expectedAssetNames,
        IEnumerable<string> uploadedAssetNames)
    {
        var expected = expectedAssetNames
            .Order(StringComparer.Ordinal)
            .ToArray();

        var uploaded = uploadedAssetNames
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (uploaded.Length !=
            uploaded
                .Distinct(StringComparer.Ordinal)
                .Count())
            throw new StepFailedException("The GitHub release contains duplicate asset names.");

        if (!expected.SequenceEqual(uploaded, StringComparer.Ordinal))
            throw new StepFailedException(
                $"GitHub release asset verification failed. Expected: {string.Join(", ", expected)}. " +
                $"Uploaded: {string.Join(", ", uploaded)}.");
    }
}
