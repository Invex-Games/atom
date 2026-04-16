namespace DecSm.Atom.Tests.ClassTests.Params;

[TestFixture]
[NonParallelizable]
public class ParamServiceTests
{
    [SetUp]
    public void Setup()
    {
        _buildDefinition = A.Fake<IBuildDefinition>();
        _args = new(true, []);
        _config = A.Fake<IConfiguration>();
        _console = A.Fake<IAnsiConsole>();
        _logger = A.Fake<ILogger<ParamService>>();

        _vaultProviders = new List<ISecretsProvider>
        {
            A.Fake<ISecretsProvider>(),
        };

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);
    }

    [TearDown]
    public void TearDown() =>
        Environment.SetEnvironmentVariable("test-param", null);

    private IBuildDefinition _buildDefinition;
    private CommandLineArgs _args;
    private IConfiguration _config;
    private IAnsiConsole _console;
    private ILogger<ParamService> _logger;
    private IEnumerable<ISecretsProvider> _vaultProviders;
    private ParamService _paramService;

    private static string TestParam => "TestParam";

    [Test]
    public void GetParam_WithExpression_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam(() => TestParam, "DefaultValue");

        // Assert
        result.ShouldBe("ConfigValue");
    }

    [Test]
    public void GetParam_WithString_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("ConfigValue");
    }

    [Test]
    public void GetParam_WithEnvironmentVariable_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("EnvValue");
    }

    [Test]
    public void GetParam_WithVaultValue_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = true,
            ChainedParams = [],
        };

        var vaultProvider = A.Fake<ISecretsProvider>();
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, [vaultProvider]);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => vaultProvider.GetSecret("test-param"))
            .Returns("VaultValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("VaultValue");
    }

    [Test]
    public void GetParam_WithVaultValueButNotSecret_ReturnsDefaultValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        var vaultProvider = A.Fake<ISecretsProvider>();
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, [vaultProvider]);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => vaultProvider.GetSecret("test-param"))
            .Returns("VaultValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("DefaultValue");
    }

    [Test]
    public void MaskSecrets_WithSecretsInText_MasksSecrets()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = true,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => _vaultProviders
                .First()
                .GetSecret("test-param"))
            .Returns("SecretValue");

        _paramService.GetParam("TestParam", "DefaultValue");

        // Act
        var result = _paramService.MaskMatchingSecrets("This is a SecretValue in the text.");

        // Assert
        result.ShouldBe("This is a ***** in the text.");
    }

    [Test]
    public void MaskSecrets_WithSecretsInTextButNotSecretAttribute_DoesNotMaskSecrets()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => _vaultProviders
                .First()
                .GetSecret("test-param"))
            .Returns("NotSecretValue");

        _paramService.GetParam("TestParam", "DefaultValue");

        // Act
        var result = _paramService.MaskMatchingSecrets("This is a NotSecretValue in the text.");

        // Assert
        result.ShouldBe("This is a NotSecretValue in the text.");
    }

    [Test]
    public void GetParam_WithNoCacheScope_DoesNotCacheValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder().Build();

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        string? result1;
        string? result2;

        using (_paramService.CreateNoCacheScope())
        {
            result1 = _paramService.GetParam("TestParam", "DefaultValue1");
            result2 = _paramService.GetParam("TestParam", "DefaultValue2");
        }

        // Act
        var result3 = _paramService.GetParam("TestParam", "DefaultValue3");
        var result4 = _paramService.GetParam("TestParam", "DefaultValue4");

        // Assert
        result1.ShouldBe("DefaultValue1");
        result2.ShouldBe("DefaultValue2");
        result3.ShouldBe("DefaultValue3");
        result4.ShouldBe("DefaultValue3");
    }

    [Test]
    public void GetParam_WithNoneFilter_IncludesNone()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.None,
            IsSecret = false,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("DefaultValue");
    }

    [Test]
    public void GetParam_WithCommandLineArgsFilter_IncludesCommandLineArgs()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.CommandLineArgs,
            IsSecret = false,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("ArgValue");
    }

    [Test]
    public void GetParam_WithEnvironmentVariablesFilter_IncludesEnvironmentVariables()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.EnvironmentVariables,
            IsSecret = false,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("EnvValue");
    }

    [Test]
    public void GetParam_WithConfigurationFilter_IncludesConfiguration()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.Configuration,
            IsSecret = false,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("ConfigValue");
    }

    [Test]
    public void GetParam_WithVaultFilter_IncludesVault()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.Secrets,
            IsSecret = true,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "NotTestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("VaultValue");
    }

    [Test]
    public void GetParam_WithSecretsFilter_IncludesVault()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.Secrets,
            IsSecret = true,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("VaultValue");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GetParam_BoolType_WithDefault_NoMatch_ReturnsDefault(bool defaultValue)
    {
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        var result = _paramService.GetParam("TestParam", defaultValue);

        result.ShouldBe(defaultValue);
    }

    [TestCase(false)]
    [TestCase(true)]
    [TestCase(null)]
    public void GetParam_NullableBool_WithDefault_NoMatch_ReturnsDefault(bool? defaultValue)
    {
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        var result = _paramService.GetParam("TestParam", defaultValue);

        result.ShouldBe(defaultValue);
    }

    [Test]
    public void CreateOverrideSourcesScope_WhenNested_RestoresPreviousState()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _console, _config, _logger, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Use no-cache scope to ensure each GetParam call resolves from sources instead of cache
        using (_paramService.CreateNoCacheScope())
        {
            // Act & Assert
            // Without override scope, should use CommandLineArgs (highest priority)
            var resultNoScope = _paramService.GetParam("TestParam", "DefaultValue");
            resultNoScope.ShouldBe("ArgValue");

            using (_paramService.CreateOverrideSourcesScope(ParamSource.Configuration))
            {
                // In outer scope, should use Configuration only
                var resultOuterScope = _paramService.GetParam("TestParam", "DefaultValue");
                resultOuterScope.ShouldBe("ConfigValue");

                using (_paramService.CreateOverrideSourcesScope(ParamSource.EnvironmentVariables))
                {
                    // In inner scope, should use EnvironmentVariables only
                    var resultInnerScope = _paramService.GetParam("TestParam", "DefaultValue");
                    resultInnerScope.ShouldBe("EnvValue");
                }

                // After inner scope disposed, should be back to Configuration
                var resultAfterInnerScope = _paramService.GetParam("TestParam", "DefaultValue");
                resultAfterInnerScope.ShouldBe("ConfigValue");
            }

            // After all scopes disposed, should be back to CommandLineArgs
            var resultAfterAllScopes = _paramService.GetParam("TestParam", "DefaultValue");
            resultAfterAllScopes.ShouldBe("ArgValue");
        }
    }

    private class TestSecretsProvider : ISecretsProvider
    {
        public string GetSecret(string secretName) =>
            "VaultValue";
    }
}
