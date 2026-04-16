namespace DecSm.Atom.Logging;

/// <summary>
///     An <see cref="IAnsiConsoleOutput" /> wrapper that masks secret values before writing to the underlying output.
/// </summary>
internal sealed class MaskingAnsiConsoleOutput : IAnsiConsoleOutput
{
    private readonly IAnsiConsoleOutput _inner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MaskingAnsiConsoleOutput" /> class.
    /// </summary>
    /// <param name="inner">The inner <see cref="IAnsiConsoleOutput" /> to wrap.</param>
    public MaskingAnsiConsoleOutput(IAnsiConsoleOutput inner)
    {
        _inner = inner;
        Writer = new MaskingTextWriter(inner.Writer);
    }

    /// <inheritdoc />
    public int Width => _inner.Width;

    /// <inheritdoc />
    public int Height => _inner.Height;

    /// <inheritdoc />
    public TextWriter Writer { get; }

    /// <inheritdoc />
    public bool IsTerminal => _inner.IsTerminal;

    /// <inheritdoc />
    public void SetEncoding(Encoding encoding) =>
        _inner.SetEncoding(encoding);

    /// <summary>
    ///     A <see cref="TextWriter" /> that masks secrets before writing to an inner writer.
    /// </summary>
    private sealed class MaskingTextWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MaskingTextWriter" /> class.
        /// </summary>
        /// <param name="innerWriter">The inner <see cref="TextWriter" /> to wrap.</param>
        public MaskingTextWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        /// <inheritdoc />
        public override Encoding Encoding => _innerWriter.Encoding;

        /// <inheritdoc />
        public override void Write(char value) =>
            Write(new string(value, 1));

        /// <summary>
        ///     Masks secrets in the specified string and writes it to the inner writer.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public override void Write(string? value)
        {
            if (value is { Length: > 0 })
                value = ServiceStaticAccessor<IParamService>.Service?.MaskMatchingSecrets(value) ?? value;

            _innerWriter.Write(value);
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            var value = new string(buffer, index, count);
            Write(value);
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value) =>
            WriteAsync(new string(value, 1));

        /// <summary>
        ///     Asynchronously masks secrets in the specified string and writes it to the inner writer.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public override Task WriteAsync(string? value)
        {
            if (value is not { Length: > 0 })
                return _innerWriter.WriteAsync(value);

            var masker = ServiceStaticAccessor<IParamService>.Service;

            if (masker is not null)
                value = masker.MaskMatchingSecrets(value);

            return _innerWriter.WriteAsync(value);
        }

        /// <inheritdoc />
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            var value = new string(buffer, index, count);

            return WriteAsync(value);
        }

        /// <inheritdoc />
        public override void Flush() =>
            _innerWriter.Flush();

        /// <inheritdoc />
        public override Task FlushAsync() =>
            _innerWriter.FlushAsync();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _innerWriter.Dispose();

            base.Dispose(disposing);
        }
    }
}
