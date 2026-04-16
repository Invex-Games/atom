namespace DecSm.Atom.Module.AzureStorage;

/// <summary>
///     Provides an implementation of <see cref="IArtifactProvider" /> for storing and retrieving
///     build artifacts in Azure Blob Storage.
/// </summary>
/// <remarks>
///     This provider uses the Azure Storage SDK to interact with blob containers,
///     allowing for robust artifact management within DecSm.Atom build processes.
/// </remarks>
[PublicAPI]
public sealed class AzureBlobArtifactProvider(
    IBuildInfo buildInfo,
    IParamService paramService,
    ReportService reportService,
    IAtomFileSystem fileSystem,
    IBuildIdProvider buildIdProvider,
    ILogger<AzureBlobArtifactProvider> logger
) : IArtifactProvider
{
    /// <summary>
    ///     Gets a list of required parameters for this artifact provider to function correctly.
    /// </summary>
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer),
        nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];

    /// <summary>
    ///     Stores build artifacts in Azure Blob Storage.
    /// </summary>
    /// <param name="artifactNames">
    ///     A list of artifact names to store. Each name corresponds to a directory in the Atom publish
    ///     directory.
    /// </param>
    /// <param name="buildId">
    ///     Optional. The unique identifier for the build. If not provided, the current build ID from
    ///     <see cref="IBuildIdProvider" /> will be used.
    /// </param>
    /// <param name="slice">
    ///     Optional. A slice identifier to categorize artifacts within a build. If not provided, the build
    ///     slice from <see cref="IBuildInfo" /> will be used.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a build ID is required but not available, or if no files are found in an artifact directory.
    /// </exception>
    public async Task StoreArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? slice = null,
        CancellationToken cancellationToken = default)
    {
        buildId ??= buildIdProvider.BuildId;

        if (buildId is null)
            throw new InvalidOperationException("A run identifier is required to upload artifacts.");

        var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
        var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

        var connectionString =
            paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));

        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = buildInfo.BuildName;

        var serviceClient = new BlobServiceClient(connectionString,
            new()
            {
                Retry =
                {
                    NetworkTimeout = TimeSpan.FromMinutes(3),
                },
            });

        var containerClient = serviceClient.GetBlobContainerClient(container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        slice ??= pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

        logger.LogInformation("Uploading artifacts '{Artifacts}' to container '{Container}' with path '{BlobPath}'",
            artifactNames,
            container.SanitizeForLogging(),
            slice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/*/{slice}"
                : $"{buildName}/{buildIdPath}/*");

        foreach (var artifactName in artifactNames)
        {
            var publishDir = fileSystem.AtomPublishDirectory / artifactName;

            var artifactBlobDir = slice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/{artifactName}/{slice}"
                : $"{buildName}/{buildIdPath}/{artifactName}";

            var files = fileSystem.Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories);

            foreach (var file in files)
                logger.LogDebug("Found file {File} for upload", file);

            if (files.Length == 0)
                throw new InvalidOperationException($"Could not find any files in the directory {publishDir}.");

            logger.LogInformation("Uploading {FileCount} files to {BlobDir}",
                files.Length,
                artifactBlobDir.SanitizeForLogging());

            foreach (var file in files)
            {
                var relativePath = fileSystem.Path.GetRelativePath(publishDir, file);
                var blobPath = $"{artifactBlobDir}/{relativePath}";

                var blobClient = containerClient.GetBlobClient(blobPath);

                await blobClient.UploadAsync(file,
                    new BlobHttpHeaders
                    {
                        ContentType = MimeTypes.GetMimeType(file),
                    },
                    cancellationToken: cancellationToken);
            }

            // Add report data for the artifact - name and url
            reportService.AddReportData(new ArtifactReportData($"{artifactName} - {buildId}",
                $"{containerClient.Uri}/{artifactBlobDir}"));
        }
    }

    /// <summary>
    ///     Retrieves build artifacts from Azure Blob Storage.
    /// </summary>
    /// <param name="artifactNames">A list of artifact names to retrieve.</param>
    /// <param name="buildId">
    ///     Optional. The unique identifier for the build from which to retrieve artifacts. If not provided,
    ///     the current build ID will be used.
    /// </param>
    /// <param name="buildSlice">
    ///     Optional. The slice identifier of the artifacts to retrieve. If not provided, the build slice
    ///     from <see cref="IBuildInfo" /> will be used.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a build ID is required but not available, or if no blobs are found for the specified artifact.
    /// </exception>
    public async Task RetrieveArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default)
    {
        buildId ??= buildIdProvider.BuildId;

        if (buildId is null)
            throw new InvalidOperationException("Build ID is required to retrieve artifacts.");

        var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
        var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

        var connectionString =
            paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));

        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = buildInfo.BuildName;

        var serviceClient = new BlobServiceClient(connectionString,
            new()
            {
                Retry =
                {
                    NetworkTimeout = TimeSpan.FromMinutes(3),
                },
            });

        var containerClient = serviceClient.GetBlobContainerClient(container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        buildSlice ??= pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

        logger.LogInformation("Downloading artifacts '{Artifacts}' from container '{Container}' with path '{BlobPath}'",
            artifactNames,
            container.SanitizeForLogging(),
            buildSlice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/*/{buildSlice}"
                : $"{buildName}/{buildIdPath}/*");

        foreach (var artifactName in artifactNames)
        {
            var artifactDir = fileSystem.AtomArtifactsDirectory / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            // Includes path separator at the end to prevent matching other directories with the same start of the name
            var artifactBlobDir = buildSlice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/{artifactName}/{buildSlice}/"
                : $"{buildName}/{buildIdPath}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(new()
                {
                    Prefix = artifactBlobDir,
                },
                cancellationToken);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync(cancellationToken);
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith(artifactBlobDir))
                    blobName = blobName[artifactBlobDir.Length..];
                else
                    throw new InvalidOperationException(
                        $"Blob name {blobName} does not start with {buildName}/{buildIdPath}.");

                var blobPath = artifactDir / blobName;
                var blobDir = fileSystem.Path.GetDirectoryName(blobPath);

                if (blobDir is null)
                    throw new InvalidOperationException($"Could not get directory name for blob path {blobPath}.");

                if (!fileSystem.Directory.Exists(blobDir))
                    fileSystem.Directory.CreateDirectory(blobDir);

                logger.LogTrace("Writing file {BlobPath}", blobPath);
                await using var fileStream = fileSystem.File.Create(blobPath);
                await blobDownloadInfo.Value.Content.CopyToAsync(fileStream, cancellationToken);
            }

            if (!hasAtLeastOneBlob)
                throw new InvalidOperationException(
                    $"Could not find any blobs in the container {container.SanitizeForLogging()} with the prefix {buildName}/{buildIdPath}/{artifactName}.");
        }
    }

    /// <summary>
    ///     Cleans up artifacts associated with specified run identifiers from Azure Blob Storage.
    /// </summary>
    /// <param name="runIdentifiers">A list of build IDs for which to clean up artifacts.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task Cleanup(IReadOnlyList<string> runIdentifiers, CancellationToken cancellationToken = default)
    {
        var connectionString =
            paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));

        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = buildInfo.BuildName;
        var containerClient = new BlobContainerClient(connectionString, container);

        foreach (var buildId in runIdentifiers)
        {
            var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
            var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

            var blobs = containerClient.GetBlobsByHierarchyAsync(new()
                {
                    Prefix = $"{buildName}/{buildIdPath}/",
                },
                cancellationToken);

            await foreach (var blob in blobs)
            {
                logger.LogInformation("Deleting blob {Blob}", blob.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blob.Blob.Name);

                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    ///     Gets a list of stored run identifiers (build IDs) that have artifacts in Azure Blob Storage.
    /// </summary>
    /// <param name="artifactName">Optional. Filters the run identifiers to those that contain a specific artifact name.</param>
    /// <param name="buildSlice">
    ///     Optional. Filters the run identifiers to those that contain artifacts for a specific build
    ///     slice.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A read-only list of unique build IDs.</returns>
    public async Task<IReadOnlyList<string>> GetStoredRunIdentifiers(
        string? artifactName = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));

        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = buildInfo.BuildName;
        var containerClient = new BlobContainerClient(connectionString, container);

        if (artifactName is { Length: > 0 })
        {
            var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
            var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");

            buildSlice ??=
                pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

            artifactName = buildSlice is { Length: > 0 }
                ? $"{artifactName}/{buildSlice}/"
                : $"{artifactName}/";
        }

        var blobs = containerClient.GetBlobsByHierarchyAsync(new()
            {
                Prefix = $"{buildName}/",
            },
            cancellationToken);

        var buildIds = new List<string>();

        var blobNameRegex = artifactName is { Length: > 0 }
            ? new Regex($"^{Regex.Escape(buildName)}/([^/]+/)+{Regex.Escape(artifactName)}")
            : new($"^{Regex.Escape(buildName)}/([^/]+/)+");

        await foreach (var blob in blobs)
        {
            var blobName = blob.Blob.Name;
            var match = blobNameRegex.Match(blobName);

            if (!match.Success)
                continue;

            var buildId = match
                .Groups[1]
                .Value
                .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Last();

            if (!buildIds.Contains(buildId))
                buildIds.Add(buildId);
        }

        return buildIds;
    }
}
