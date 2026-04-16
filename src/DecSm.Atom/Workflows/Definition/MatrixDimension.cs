namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Represents a dimension in a build matrix, defining variations for a workflow job or step.
/// </summary>
/// <param name="Name">The name of the matrix dimension (e.g., "os", "dotnet-version").</param>
/// <remarks>
///     A <c>MatrixDimension</c> is used to run the same job or step across different configurations.
///     For example, a dimension named "os" could have values like "windows-latest" and "ubuntu-latest".
/// </remarks>
[PublicAPI]
public record MatrixDimension(string Name)
{
    /// <summary>
    ///     Gets a read-only list of string values for this dimension.
    /// </summary>
    /// <remarks>
    ///     These values represent the different options for this dimension (e.g., "windows-latest", "ubuntu-latest").
    /// </remarks>
    public IReadOnlyList<string> Values { get; init; } = [];
}
