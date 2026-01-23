namespace DecSm.Atom.Module.GithubWorkflows.Generation;

[PublicAPI]
public sealed record GithubStepWriter(
    IWorkflowExpressionGenerator WorkflowExpressionGenerator,
    IAtomFileSystem FileSystem,
    StringBuilder Builder,
    int BaseIndentLevel
)
{
    private const int TabSize = 2;
    private int _indentLevel = BaseIndentLevel;

    /// <summary>
    ///     Writes a line of text to the output with the current indentation.
    /// </summary>
    /// <param name="value">The text to write. If null, an empty line is written.</param>
    public void WriteLine(string? value = null)
    {
        if (_indentLevel > 0)
            Builder.Append(new string(' ', _indentLevel));

        Builder.AppendLine(value);
    }

    /// <summary>
    ///     Writes a section header and returns a disposable scope that manages indentation for the section's content.
    /// </summary>
    /// <param name="header">The header text for the section.</param>
    /// <returns>A disposable object that decreases the indentation level upon disposal.</returns>
    /// <example>
    ///     <code>
    /// using (WriteSection("- checkout: self"))
    ///     WriteLine("fetchDepth: 0");
    ///     </code>
    /// </example>
    public IDisposable WriteSection(string header)
    {
        WriteLine(header);
        _indentLevel += TabSize;

        return new ActionScope(() => _indentLevel -= TabSize);
    }

    public void ResetIndent() =>
        _indentLevel = BaseIndentLevel;

    public override string ToString() =>
        Builder.ToString();
}
