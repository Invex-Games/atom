namespace Invex.Atom.Build.Tests.BuildTests.Products;

[TestFixture]
internal sealed class ProductValidatorTests
{
    private static IReadOnlyList<string> GetErrors(params IReadOnlyList<Product> products) =>
        GetErrors(null, products);

    private static IReadOnlyList<string> GetErrors(MockFileSystem? fileSystem, params IReadOnlyList<Product> products)
    {
        var host = CreateTestHost<ProductsBuild>(fileSystem: fileSystem);
        var build = (ProductsBuild)host.Services.GetRequiredService<IBuildDefinition>();
        build.TestProducts = products;

        return ProductValidator.GetErrors(build,
            fileSystem is null
                ? null
                : host.Services.GetRequiredService<IRootedFileSystem>());
    }

    [Test]
    public void GetErrors_WithValidProducts_ReturnsNone() =>
        GetErrors(new Product("Shared")
                {
                    Paths = ["src/Shared/"],
                },
                new Product("App.A-1_x")
                {
                    Paths = ["src/**/AppA/*.cs", "src/AppA/x.txt"],
                    DependsOn = ["shared"],
                    GitVersionConfig = "src/AppA/GitVersion.yml",
                })
            .ShouldBeEmpty();

    [Test]
    public void GetErrors_ReportsEachProblem() =>
        GetErrors(new Product("AppA")
                {
                    Paths = [],
                    GitVersionConfig = "src/GitVersion.yml",
                },
                new Product("appa")
                {
                    Paths = ["/src/AppA/**", "src\\AppA", "src/[ab]/**", "src/App**/x", "./src"],
                    DependsOn = ["Missing", "appa"],
                    TagPrefix = "appa/v",
                    GitVersionConfig = "src\\GitVersion.yml",
                },
                new Product("-Bad")
                {
                    Paths = ["x/**"],
                    TagPrefix = "",
                })
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

    [TestCase(" gitversion.YML")]
    [TestCase("./GitVersion.yml")]
    [TestCase("src/../.GitVersion.yaml")]
    [TestCase(".\\GitVersion.yml")]
    public void GetErrors_WithRootGitVersionConfig_ReportsIt(string config) =>
        GetErrors(new Product("AppA")
            {
                Paths = ["a/"],
                GitVersionConfig = config,
            })
            .ShouldBe([
                $"The GitVersion configuration of product 'AppA' is '{config.Trim()}', which GitVersion also reads for builds without a product. Give the product its own file, such as 'src/AppA/GitVersion.yml'.",
            ]);

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
                new Product("AppA")
                {
                    Paths = ["src/AppA", "src/AppA.props", "src/Missing", "libs/Submodule"],
                })
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
