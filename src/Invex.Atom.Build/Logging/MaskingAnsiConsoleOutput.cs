namespace Invex.Atom.Build.Logging;

/// <summary>
///     An <see cref="IAnsiConsoleOutput" /> wrapper that masks secret values before writing to the underlying output.
/// </summary>
internal sealed class MaskingAnsiConsoleOutput(IAnsiConsoleOutput inner, IServiceProvider serviceProvider)
    : IAnsiConsoleOutput
{
    /// <inheritdoc />
    public int Width => inner.Width;

    /// <inheritdoc />
    public int Height => inner.Height;

    /// <inheritdoc />
    public TextWriter Writer => field ??= new MaskingTextWriter(inner.Writer, serviceProvider);

    /// <inheritdoc />
    public bool IsTerminal => inner.IsTerminal;

    /// <inheritdoc />
    public void SetEncoding(Encoding encoding) =>
        inner.SetEncoding(encoding);

    /// <summary>
    ///     A <see cref="TextWriter" /> that masks secrets before writing to an inner writer.
    /// </summary>
    private sealed class MaskingTextWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MaskingTextWriter" /> class.
        /// </summary>
        /// <param name="innerWriter">The inner <see cref="TextWriter" /> to wrap.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /> instance.</param>
        public MaskingTextWriter(TextWriter innerWriter, IServiceProvider serviceProvider)
        {
            _innerWriter = innerWriter;
            _serviceProvider = serviceProvider;
        }

        private IParamService ParamService => field ??= _serviceProvider.GetRequiredService<IParamService>();

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
                value = ParamService.MaskMatchingSecrets(value);

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
        public override Task WriteAsync(string? value) =>
            value switch
            {
                null or { Length: 0 } => _innerWriter.WriteAsync(value),
                _ => _innerWriter.WriteAsync(ParamService.MaskMatchingSecrets(value)),
            };

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
