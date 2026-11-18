namespace Invex.Atom.Build.Tests.BuildTests.Products;

[TestFixture]
[NonParallelizable]
internal sealed class ProductsTests
{
    private static readonly string[] BaseRefVariables =
        ["GITHUB_BASE_REF", "SYSTEM_PULLREQUEST_TARGETBRANCHNAME", "SYSTEM_PULLREQUEST_TARGETBRANCH"];

    private Dictionary<string, string?> _savedEnvironment = [];

    private static string Root =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom"
            : "/Atom";

    [SetUp]
    public void SetUp()
    {
        // CI sets some of these for real, so clear them for every test
        _savedEnvironment = BaseRefVariables.ToDictionary(x => x, Environment.GetEnvironmentVariable);

        foreach (var name in BaseRefVariables)
            Environment.SetEnvironmentVariable(name, null);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var (name, value) in _savedEnvironment)
            Environment.SetEnvironmentVariable(name, value);

        Environment.ExitCode = 0;
    }

    private static (IHost Host, ProductsBuild Build) CreateHost(
        string? product = null,
        MockFileSystem? fileSystem = null,
        IProcessRunner? processRunner = null,
        IVariableProvider? variableProvider = null,
        string target = nameof(IProductsTestTargets.ProductInfoTarget),
        TestConsole? console = null,
        bool interactive = false)
    {
        var host = CreateTestHost<ProductsBuild>(console,
            fileSystem: fileSystem,
            commandLineArgs: new(true,
                interactive
                    ? [new CommandArg(target), new InteractiveArg()]
                    : [new CommandArg(target)]),
            configure: builder =>
            {
                if (product is not null)
                    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Params:product"] = product,
                    });

                if (processRunner is not null)
                    builder.Services.AddSingleton(processRunner);

                if (variableProvider is not null)
                    builder.Services.AddSingleton(variableProvider);
            });

        return (host, (ProductsBuild)host.Services.GetRequiredService<IBuildDefinition>());
    }

    [Test]
    public void GetProductPaths_IncludesDependenciesAndGlobalPaths()
    {
        var (_, build) = CreateHost();
        IProductsDefinition products = build;

        products
            .GetProductPaths(products.GetProduct("AppA"))
            .ShouldBe(["src/AppA/**", "src/Shared/**", .. IProductsDefinition.DefaultGlobalPaths]);

        products
            .GetProductPaths(products.GetProduct("appb"), false)
            .ShouldBe(["src/AppB/**", "docs/appb/*.md", "src/Shared/**"]);
    }

    [Test]
    public void GetProductPaths_WithUnknownDependency_Throws()
    {
        var (_, build) = CreateHost();

        build.TestProducts =
        [
            new("AppA")
            {
                Paths = ["src/AppA/**"],
                DependsOn = ["Missing"],
            },
        ];

        IProductsDefinition products = build;

        Should
            .Throw<InvalidOperationException>(() => products.GetProductPaths(products.GetProduct("AppA")))
            .Message
            .ShouldBe("Product 'AppA' depends on unknown product 'Missing'.");
    }

    [Test]
    public void GetProductPaths_WithCycle_Throws()
    {
        var (_, build) = CreateHost();

        build.TestProducts =
        [
            new("A")
            {
                Paths = ["a/**"],
                DependsOn = ["B"],
            },
            new("B")
            {
                Paths = ["b/**"],
                DependsOn = ["a"],
            },
        ];

        IProductsDefinition products = build;

        Should
            .Throw<InvalidOperationException>(() => products.GetProductPaths(products.GetProduct("A")))
            .Message
            .ShouldBe("Product dependencies form a cycle: A -> B -> A.");
    }

    [TestCase(new[] { "src/AppA/Program.cs" }, new[] { "AppA" })]
    [TestCase(new[] { @"src\AppA\Program.cs" }, new[] { "AppA" })]
    [TestCase(new[] { "src/Shared/Lib.cs" }, new[] { "Shared", "AppA", "AppB" })]
    [TestCase(new[] { "docs/appb/readme.md" }, new[] { "AppB" })]
    [TestCase(new[] { "docs/appb/nested/readme.md" }, new string[0])]
    [TestCase(new[] { "README.md", "src/AppAB/x.cs" }, new string[0])]
    [TestCase(new[] { "global.json" }, new[] { "Shared", "AppA", "AppB" })]
    [TestCase(new[] { "_atom/Build.cs" }, new[] { "Shared", "AppA", "AppB" })]
    [TestCase(new[] { "src/AppA/x.cs", "src/AppB/y.cs" }, new[] { "AppA", "AppB" })]
    [TestCase(new string[0], new string[0])]
    public void GetAffectedProducts_MatchesChangedFiles(string[] changedFiles, string[] expected)
    {
        var (_, build) = CreateHost();

        ((IProductsDefinition)build)
            .GetAffectedProducts(changedFiles)
            .Select(x => x.Name)
            .ShouldBe(expected);
    }

    [Test]
    public void CurrentProduct_WithoutProductParam_IsNull()
    {
        var (host, build) = CreateHost();

        ((IProductsDefinition)build).CurrentProduct.ShouldBeNull();

        host
            .Services
            .GetRequiredService<IProductContext>()
            .Current
            .ShouldBeNull();
    }

    [Test]
    public void CurrentProduct_WithProductParam_MatchesIgnoringCase()
    {
        var (host, build) = CreateHost("appa");

        ((IProductsDefinition)build)
            .CurrentProduct
            .ShouldNotBeNull()
            .Name
            .ShouldBe("AppA");

        host
            .Services
            .GetRequiredService<IProductContext>()
            .Current
            .ShouldNotBeNull()
            .Name
            .ShouldBe("AppA");
    }

    [Test]
    public void CurrentProduct_WithUnknownProduct_ThrowsListingKnownProducts()
    {
        var (_, build) = CreateHost("AppC");

        Should
            .Throw<InvalidOperationException>(() => ((IProductsDefinition)build).CurrentProduct)
            .Message
            .ShouldBe("Unknown product 'AppC'. Known products: Shared, AppA, AppB.");
    }

    [Test]
    public async Task BuildInfo_WithoutProduct_UsesRepositoryDefaults()
    {
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "Directory.Build.props"),
            new("<Project><PropertyGroup><Version>2.0.0</Version></PropertyGroup></Project>"));

        var (host, build) = CreateHost(fileSystem: fileSystem);

        await host.RunAsync();

        build.ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("Atom"),
            b => b.BuildVersionResult.ShouldBe(SemVer.Parse("2.0.0")),
            b => b.ReleaseTagResult.ShouldBe("v1.2.0"));
    }

    [Test]
    public async Task BuildInfo_Interactive_WithEmptyProductAnswer_UsesRepositoryDefaults()
    {
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "Directory.Build.props"),
            new("<Project><PropertyGroup><Version>2.0.0</Version></PropertyGroup></Project>"));

        // An empty answer to the product prompt must be accepted and select no product
        using var console = new TestConsole();
        console.Interactive();

        for (var i = 0; i < 8; i++)
            console.Input.PushTextWithEnter(string.Empty);

        var (host, build) = CreateHost(fileSystem: fileSystem, console: console, interactive: true);

        await host.RunAsync();

        build.ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("Atom"),
            b => b.BuildVersionResult.ShouldBe(SemVer.Parse("2.0.0")),
            b => b.ReleaseTagResult.ShouldBe("v1.2.0"));
    }

    [Test]
    public async Task BuildInfo_WithProduct_UsesProductDefaults()
    {
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "Directory.Build.props"),
            new("<Project><PropertyGroup><Version>2.0.0</Version></PropertyGroup></Project>"));
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "src", "AppA", "Directory.Build.props"),
            new("<Project><PropertyGroup><VersionPrefix>3.4.5</VersionPrefix></PropertyGroup></Project>"));

        var (host, build) = CreateHost("AppA", fileSystem);

        await host.RunAsync();

        build.ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("AppA"),
            b => b.BuildVersionResult.ShouldBe(SemVer.Parse("3.4.5")),
            b => b.ReleaseTagResult.ShouldBe("appa/v1.2.0"));
    }

    [Test]
    public async Task BuildInfo_WithProductWithoutVersion_FallsBackToRoot()
    {
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "Directory.Build.props"),
            new("<Project><PropertyGroup><Version>2.0.0</Version></PropertyGroup></Project>"));
        fileSystem.AddFile(fileSystem.Path.Combine(Root, "src", "AppA", "Directory.Build.props"),
            new("<Project><Import Project=\"../../Directory.Build.props\" /></Project>"));

        var (host, build) = CreateHost("AppA", fileSystem);

        await host.RunAsync();

        build.BuildVersionResult.ShouldBe(SemVer.Parse("2.0.0"));
    }

    [Test]
    public async Task BuildInfo_WithProductAndExplicitParams_UsesParams()
    {
        var (host, build) = CreateHost("AppB");

        host
            .Services
            .GetRequiredService<IConfiguration>()["Params:build-name"] = "Custom";

        await host.RunAsync();

        build.ShouldSatisfyAllConditions(b => b.BuildNameResult.ShouldBe("Custom"),
            b => b.ReleaseTagResult.ShouldBe("app-b-1.2.0"));
    }

    [Test]
    public void ReleaseTagHelpers_WithProduct_UseProductPrefix()
    {
        var (_, build) = CreateHost("AppA");
        IReleaseTagHelper helper = build;

        helper.ShouldSatisfyAllConditions(h => h.ReleaseTagPrefix.ShouldBe("appa/v"),
            h => h.ReleaseTagGlob.ShouldBe("appa/v*"),
            h => h
                .TryParseReleaseTag("appa/v1.2.0", out _)
                .ShouldBeTrue(),
            h => h
                .TryParseReleaseTag("v1.2.0", out _)
                .ShouldBeFalse());
    }

    [Test]
    public async Task GetReleaseTagCommit_WithProduct_UsesProductPathspecs()
    {
        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                return Task.FromResult(new ProcessRunResult(options,
                    0,
                    options.Args.StartsWith("rev-parse", StringComparison.Ordinal)
                        ? "false"
                        : "abc123",
                    string.Empty));
            });

        var (_, build) = CreateHost("AppA", processRunner: processRunner);

        var commit = await ((IReleaseTagHelper)build).GetReleaseTagCommit();

        commit.ShouldBe("abc123");

        calls[^1]
            .Args
            .ShouldBe(
                "log -1 --first-parent --no-show-signature --format=%H -- :(glob)src/AppA/** :(glob)src/Shared/** :(glob)global.json :(glob)Directory.Build.* :(glob)Directory.Packages.* :(glob)_atom/** :(glob).github/workflows/** :(glob).devops/workflows/**");
    }

    [TestCase(null)]
    [TestCase("AppA")]
    public async Task GetReleaseTagCommit_ForAnotherProduct_UsesThatProductsPathspecs(string? currentProduct)
    {
        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                return Task.FromResult(new ProcessRunResult(options,
                    0,
                    options.Args.StartsWith("rev-parse", StringComparison.Ordinal)
                        ? "false"
                        : "abc123",
                    string.Empty));
            });

        var (_, build) = CreateHost(currentProduct, processRunner: processRunner);
        IProductsDefinition products = build;

        var commit = await ((IReleaseTagHelper)build).GetReleaseTagCommit(products.GetProduct("AppB"));

        commit.ShouldBe("abc123");

        calls
            .Select(x => x.Args)
            .ShouldBe([
                "rev-parse --is-shallow-repository",
                "log -1 --first-parent --no-show-signature --format=%H -- :(glob)src/AppB/** :(glob)docs/appb/*.md :(glob)src/Shared/** :(glob)global.json :(glob)Directory.Build.* :(glob)Directory.Packages.* :(glob)_atom/** :(glob).github/workflows/** :(glob).devops/workflows/**",
            ]);
    }

    [Test]
    public async Task GetReleaseTagCommit_WithoutProduct_UsesHead()
    {
        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                return Task.FromResult(new ProcessRunResult(options, 0, "abc123", string.Empty));
            });

        var (_, build) = CreateHost(processRunner: processRunner);

        await ((IReleaseTagHelper)build).GetReleaseTagCommit();

        calls
            .ShouldHaveSingleItem()
            .Args
            .ShouldBe("log -1 --no-show-signature --format=%H");
    }

    [Test]
    public async Task DetectAffectedProducts_ForPullRequest_WritesAffectedProducts()
    {
        Environment.SetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH", "refs/heads/main");

        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                return Task.FromResult(new ProcessRunResult(options,
                    0,
                    "src/AppB/Program.cs\0README.md\0",
                    string.Empty));
            });

        var variables = new CapturingVariableProvider();

        var (host, _) = CreateHost(processRunner: processRunner,
            variableProvider: variables,
            target: nameof(IProductsDefinition.DetectAffectedProducts));

        await host.RunAsync();

        calls
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(c => c.Name.ShouldBe("git"),
                c => c.Args.ShouldBe("diff --name-only --no-renames --relative -z origin/main...HEAD"));

        variables.Variables["affected-products"]
            .ShouldBe(",appb,");
    }

    [Test]
    public async Task DetectAffectedProducts_WithNoAffectedProducts_WritesComma()
    {
        Environment.SetEnvironmentVariable("GITHUB_BASE_REF", "main");

        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
                Task.FromResult(new ProcessRunResult(options, 0, "README.md", string.Empty)));

        var variables = new CapturingVariableProvider();

        var (host, _) = CreateHost(processRunner: processRunner,
            variableProvider: variables,
            target: nameof(IProductsDefinition.DetectAffectedProducts));

        await host.RunAsync();

        variables.Variables["affected-products"]
            .ShouldBe(",");
    }

    [Test]
    public async Task DetectAffectedProducts_WhenGitFails_MarksEveryProductAffected()
    {
        Environment.SetEnvironmentVariable("GITHUB_BASE_REF", "main");

        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
                Task.FromResult(new ProcessRunResult(options, 128, string.Empty, "fatal: bad revision")));

        var variables = new CapturingVariableProvider();

        var (host, _) = CreateHost(processRunner: processRunner,
            variableProvider: variables,
            target: nameof(IProductsDefinition.DetectAffectedProducts));

        await host.RunAsync();

        variables.Variables["affected-products"]
            .ShouldBe(",shared,appa,appb,");
    }

    [Test]
    public async Task DetectAffectedProducts_WithoutBaseRef_MarksEveryProductAffectedWithoutGit()
    {
        var processRunner = A.Fake<IProcessRunner>();
        var variables = new CapturingVariableProvider();

        var (host, _) = CreateHost(processRunner: processRunner,
            variableProvider: variables,
            target: nameof(IProductsDefinition.DetectAffectedProducts));

        await host.RunAsync();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        variables.Variables["affected-products"]
            .ShouldBe(",shared,appa,appb,");
    }

    [Test]
    public async Task ValidateBuild_WithInvalidProducts_Fails()
    {
        using var console = new TestConsole();
        console.Profile.Width = 1000;
        var (host, build) = CreateHost(target: nameof(IValidateBuild.ValidateBuild), console: console);

        build.TestProducts =
        [
            new("AppA")
            {
                Paths = [],
            },
            new("appa")
            {
                Paths = ["src/[ab]/**"],
                DependsOn = ["Missing"],
            },
        ];

        Environment.ExitCode = 0;

        await host.RunAsync();

        Environment.ExitCode.ShouldBe(1);

        console.Output.ShouldSatisfyAllConditions(o => o.ShouldContain("Product 'AppA' has no paths."),
            o => o.ShouldContain("Product 'appa' depends on unknown product 'Missing'."));
    }
}
