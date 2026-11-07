namespace Invex.Atom.Build.Tests.BuildTests.Products;

[TestFixture]
internal sealed class ProductValidatorTests
{
    private static IReadOnlyList<string> GetErrors(params IReadOnlyList<Product> products) =>
        GetErrors(null, _ => products);

    /// <summary>
    ///     Validates products built from the test host's Atom root directory.
    /// </summary>
    private static IReadOnlyList<string> GetErrors(Func<RootedPath, IReadOnlyList<Product>> products) =>
        GetErrors(null, products);

    private static IReadOnlyList<string> GetErrors(
        MockFileSystem? fileSystem,
        Func<RootedPath, IReadOnlyList<Product>> products)
    {
        var host = CreateTestHost<ProductsBuild>(fileSystem: fileSystem);
        var build = (ProductsBuild)host.Services.GetRequiredService<IBuildDefinition>();
        var rootedFileSystem = host.Services.GetRequiredService<IRootedFileSystem>();
        build.TestProducts = products(rootedFileSystem.AtomRootDirectory);

        return ProductValidator.GetErrors(build, rootedFileSystem);
    }

    [Test]
    public void GetErrors_WithValidProducts_ReturnsNone() =>
        GetErrors(root =>
            [
                new Product("Shared")
                {
                    Paths = ["src/Shared/"],
                },
                new Product("App.A-1_x")
                {
                    Paths = ["src/**/AppA/*.cs", "src/AppA/x.txt"],
                    DependsOn = ["shared"],
                    GitVersionConfig = root / "src/AppA/GitVersion.yml",
                },
            ])
            .ShouldBeEmpty();

    [Test]
    public void GetErrors_ReportsEachProblem() =>
        GetErrors(root =>
            [
                new Product("AppA")
                {
                    Paths = [],
                    GitVersionConfig = root / "src/GitVersion.yml",
                },
                new Product("appa")
                {
                    Paths = ["/src/AppA/**", "src\\AppA", "src/[ab]/**", "src/App**/x", "./src"],
                    DependsOn = ["Missing", "appa"],
                    TagPrefix = "appa/v",
                    GitVersionConfig = root / "src/./GitVersion.yml",
                },
                new Product("-Bad")
                {
                    Paths = ["x/**"],
                    TagPrefix = "",
                },
            ])
            .ShouldBe([
                "Product 'AppA' has no paths.",
                "Path '/src/AppA/**' of product 'appa' is not supported: it starts with '/'; paths are relative to the repository root.",
                "Path 'src\\AppA' of product 'appa' is not supported: it contains '\\'; use '/' as the separator.",
                "Path 'src/[ab]/**' of product 'appa' is not supported: it uses glob syntax other than '*' and '**'.",
                "Path 'src/App**/x' of product 'appa' is not supported: it uses '**' inside a path segment; '**' must be a whole segment, such as 'src/**/bin'.",
                "Path './src' of product 'appa' is not supported: it starts with './'; paths are relative to the repository root.",
                "Product 'appa' depends on itself.",
                "Product name '-Bad' is not valid. Use letters, digits, '.', '-' and '_', starting with a letter or digit.",
                "Product '-Bad' has an empty tag prefix.",
                "Products 'AppA', 'appa' share the name 'AppA'.",
                "Products 'AppA', 'appa' share the tag prefix 'appa/v'.",
                "Products 'AppA', 'appa' share the GitVersion configuration 'src/GitVersion.yml'.",
                "Product 'appa' depends on unknown product 'Missing'.",
            ]);

    [Test]
    public void GetErrors_WithTagPrefixStartingAnother_ReportsIt() =>
        GetErrors(new Product("AppA")
                {
                    Paths = ["a/"],
                    TagPrefix = "v",
                },
                new Product("Vision")
                {
                    Paths = ["b/"],
                },
                new Product("AppB")
                {
                    Paths = ["c/"],
                    TagPrefix = "APPA/v",
                },
                new Product("AppC")
                {
                    Paths = ["d/"],
                    TagPrefix = "appa/",
                })
            .ShouldBe([
                "The tag prefix 'v' of product 'AppA' starts the tag prefix 'vision/v' of product 'Vision', so 'AppA' would match 'Vision''s release tags.",
                "The tag prefix 'appa/' of product 'AppC' starts the tag prefix 'APPA/v' of product 'AppB', so 'AppC' would match 'AppB''s release tags.",
            ]);

    [TestCase("gitversion.YML", "gitversion.YML")]
    [TestCase("./GitVersion.yml", "GitVersion.yml")]
    [TestCase("src/../.GitVersion.yaml", ".GitVersion.yaml")]
    public void GetErrors_WithRootGitVersionConfig_ReportsIt(string config, string expectedConfig) =>
        GetErrors(root =>
            [
                new Product("AppA")
                {
                    Paths = ["a/"],
                    GitVersionConfig = root / config,
                },
            ])
            .ShouldBe([
                $"The GitVersion configuration of product 'AppA' is '{expectedConfig}', which GitVersion also reads for builds without a product. Give the product its own file, such as 'src/AppA/GitVersion.yml'.",
            ]);

    [TestCase("../GitVersion.yml")]
    [TestCase("../AtomOther/GitVersion.yml")]
    [TestCase("src/../../other/GitVersion.yml")]
    [TestCase(".")]
    public void GetErrors_WithGitVersionConfigNotBelowAtomRoot_ReportsIt(string config)
    {
        RootedPath? atomRoot = null;

        GetErrors(root =>
            [
                new Product("AppA")
                {
                    Paths = ["a/"],
                    GitVersionConfig = (atomRoot = root) / config,
                },
            ])
            .ShouldBe([
                $"The GitVersion configuration of product 'AppA' is '{atomRoot! / config}', which is not below the Atom root directory '{atomRoot}'. Use a file below it, such as 'src/AppA/GitVersion.yml'.",
            ]);
    }

    [TestCase("AppA\n")]
    [TestCase("App A")]
    [TestCase("_AppA")]
    public void GetErrors_WithInvalidName_ReportsIt(string name) =>
        GetErrors(new Product(name)
            {
                Paths = ["a/"],
            })
            .ShouldContain(
                $"Product name '{name}' is not valid. Use letters, digits, '.', '-' and '_', starting with a letter or digit.");

    [TestCase("./src/versions.yml")]
    [TestCase("src/AppB/../versions.yml")]
    [TestCase("SRC/versions.yml")]
    public void GetErrors_WithSameGitVersionConfigSpelledDifferently_ReportsIt(string otherConfig) =>
        GetErrors(root =>
            [
                new Product("AppA")
                {
                    Paths = ["a/"],
                    GitVersionConfig = root / "src/versions.yml",
                },
                new Product("AppB")
                {
                    Paths = ["b/"],
                    GitVersionConfig = root / otherConfig,
                },
            ])
            .ShouldBe(["Products 'AppA', 'AppB' share the GitVersion configuration 'src/versions.yml'."]);

    [TestCase(".")]
    [TestCase("src/../x/")]
    public void GetErrors_WithDotSegments_ReportsThem(string path) =>
        GetErrors(new Product("AppA")
            {
                Paths = [path],
            })
            .ShouldBe([
                $"Path '{path}' of product 'AppA' is not supported: it contains a '.' or '..' segment; write the path from the repository root without them.",
            ]);

    [Test]
    public void GetErrors_WithFolderWithoutTrailingSlash_ReportsIt()
    {
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var root = fileSystem.Path.GetDirectoryName(fileSystem.Directory.GetCurrentDirectory())!;
        fileSystem.AddFile(fileSystem.Path.Combine(root, "src", "AppA", "Program.cs"), new(string.Empty));
        fileSystem.AddFile(fileSystem.Path.Combine(root, "src", "AppA.props"), new(string.Empty));
        fileSystem.AddFile(fileSystem.Path.Combine(root, "libs", "Submodule", ".git"), new("gitdir: ../../.git/modules/x"));

        GetErrors(fileSystem,
                _ =>
                [
                    new Product("AppA")
                    {
                        Paths = ["src/AppA", "src/AppA.props", "src/Missing", "libs/Submodule"],
                    },
                ])
            .ShouldBe([
                "Path 'src/AppA' of product 'AppA' names a folder; add a trailing '/', such as 'src/AppA/', to include everything below it.",
            ]);
    }

    [Test]
    public void GetErrors_WithCycle_ReportsIt() =>
        GetErrors(new Product("A")
                {
                    Paths = ["a/**"],
                    DependsOn = ["B"],
                },
                new Product("B")
                {
                    Paths = ["b/**"],
                    DependsOn = ["A"],
                })
            .ShouldBe(["Product dependencies form a cycle: A -> B -> A.", "Product dependencies form a cycle: B -> A -> B."]);
}
