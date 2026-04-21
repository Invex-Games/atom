using DecSm.Atom.Build.Util.Scope;

namespace DecSm.Atom.Workflows.Writer;

[PublicAPI]
public class StructuredWriter(int indentSize = 2)
{
    public StringBuilder StringBuilder { get; } = new();

    public int Version { get; private set; } = 1;

    public int Indent { get; private set; }

    public int IndentSize { get; init; } = indentSize;

    public void WriteLine(string? text = null) =>
        StringBuilder.AppendLine(text is not null
            ? $"{new(' ', Indent * IndentSize)}{text}"
            : string.Empty);

    public IDisposable WriteSection(string header)
    {
        if (header is { Length: > 0 })
            StringBuilder.AppendLine($"{new(' ', Indent * IndentSize)}{header}");

        Indent++;

        var version = Version;

        return new ActionScope(() =>
        {
            if (Version == version)
                Indent--;
        });
    }

    public void Clear()
    {
        StringBuilder.Clear();
        Indent = 0;
        Version++;
    }

    public override string ToString() =>
        StringBuilder.ToString();
}
