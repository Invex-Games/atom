namespace Invex.Atom.Tool.Tests;

[TestFixture]
internal sealed class RunArgsFilterTests
{
    // A terminal ConsoleAppFilter that captures the context passed to it.
    // We pass null! because CapturingFilter IS the terminus — Next is never called.
    private sealed class CapturingFilter() : ConsoleAppFilter(null!)
    {
        public ConsoleAppContext? CapturedContext { get; private set; }

        public override Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
        {
            CapturedContext = context;

            return Task.CompletedTask;
        }
    }

    private static ConsoleAppContext MakeContext(params string[] args) =>
        // Use positional construction — parameter names differ across generator versions.
        // Signature: (commandName, arguments, rawArguments/runArguments, state, methodName?, commandDepth, escapeIndex)
        new("atom", args, ReadOnlyMemory<string>.Empty, null, null, 0, 0);

    private static async Task<ConsoleAppContext?> InvokeFilter(string[] args)
    {
        var capture = new CapturingFilter();
        var filter = new RunArgsFilter(capture);
        await filter.InvokeAsync(MakeContext(args), CancellationToken.None);

        return capture.CapturedContext;
    }

    [Test]
    public async Task InvokeAsync_NoArguments_PassesContextUnchanged()
    {
        var ctx = await InvokeFilter([]);

        ctx.ShouldNotBeNull();
        ctx.Arguments.ShouldBeEmpty();
    }

    [Test]
    public async Task InvokeAsync_NugetAddCommand_PassesContextWithoutChangingEscapeIndex()
    {
        // nuget-add should be passed through with default EscapeIndex (null / 0)
        var ctx = await InvokeFilter(["nuget-add", "my-feed", "https://example.com"]);

        ctx.ShouldNotBeNull();
        ctx.Arguments.ShouldBe(["nuget-add", "my-feed", "https://example.com"]);
    }

    [Test]
    public async Task InvokeAsync_DirectAtomArgs_SetsEscapeIndexToZero()
    {
        // Args like ["Build", "--verbose"] don't start with -p/--project
        // → all args should be forwarded to Atom → EscapeIndex = 0
        var ctx = await InvokeFilter(["Build", "--verbose"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(0);
    }

    [Test]
    public async Task InvokeAsync_ShortProjectFlag_SetsEscapeIndexToTwo()
    {
        // atom -p _atom Build → EscapeIndex should be 2 (skip "-p" and "_atom")
        var ctx = await InvokeFilter(["-p", "_atom", "Build"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(2);
    }

    [Test]
    public async Task InvokeAsync_LongProjectFlag_SetsEscapeIndexToTwo()
    {
        var ctx = await InvokeFilter(["--project", "_atom", "Build"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(2);
    }

    [Test]
    public async Task InvokeAsync_ShortFileFlag_SetsEscapeIndexToTwo()
    {
        var ctx = await InvokeFilter(["-f", "build.cs", "Build"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(2);
    }

    [Test]
    public async Task InvokeAsync_LongFileFlag_SetsEscapeIndexToTwo()
    {
        var ctx = await InvokeFilter(["--file", "build.cs", "Build"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(2);
    }

    [Test]
    public async Task InvokeAsync_ProjectFlagWithNoValue_SetsEscapeIndexToZero()
    {
        // "-p" alone (Length < 2): context.Arguments.Length is 1, which is NOT >= 2
        // so EscapeIndex should fall through to 0
        var ctx = await InvokeFilter(["-p"]);

        ctx.ShouldNotBeNull();
        ctx.EscapeIndex.ShouldBe(0);
    }

    [Test]
    public async Task InvokeAsync_ProjectFlag_ArgumentsArePreserved()
    {
        var ctx = await InvokeFilter(["--project", "myproj", "Build", "--no-restore"]);

        ctx.ShouldNotBeNull();
        ctx.Arguments.ShouldBe(["--project", "myproj", "Build", "--no-restore"]);
    }
}
