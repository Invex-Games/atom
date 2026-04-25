namespace DecSm.Atom.StructuredText;

[PublicAPI]
public sealed class StructuredTextBuilder(int indentSize = 2)
{
    public StringBuilder StringBuilder { get; } = new();

    public int Version { get; private set; } = 1;

    public int Indent { get; private set; }

    public int IndentSize { get; set; } = indentSize;

    public void Write(string? text = null, bool indent = true) =>
        StringBuilder.Append(text is not null
            ? $"{new(' ', indent ? Indent * IndentSize : 0)}{text}"
            : string.Empty);

    public void WriteLine(string? text = null) =>
        StringBuilder.AppendLine(text is not null
            ? $"{new(' ', Indent * IndentSize)}{text}"
            : string.Empty);

    public IDisposable WriteSection(string? text = null)
    {
        if (text is { Length: > 0 })
            StringBuilder.AppendLine($"{new(' ', Indent * IndentSize)}{text}");

        Indent++;

        var version = Version;

        return new ActionScope(() =>
        {
            if (Version == version)
                Indent--;
        });
    }

    public void Reset(int indentSize = 2)
    {
        StringBuilder.Clear();
        Indent = 0;
        Version++;
        IndentSize = indentSize;
    }

    public override string ToString() =>
        StringBuilder.ToString();

    private sealed record ActionScope(Action? OnDispose = null) : IDisposable
    {
        /// <summary>
        ///     Executes the disposal action if it is not null.
        /// </summary>
        public void Dispose() =>
            OnDispose?.Invoke();
    }
}
