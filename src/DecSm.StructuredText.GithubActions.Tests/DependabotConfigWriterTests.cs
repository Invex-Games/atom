namespace DecSm.StructuredText.GithubActions.Tests;

[TestFixture]
internal sealed class DependabotConfigWriterTests
{
    private static string WriteConfig(DependabotConfig config)
    {
        var writer = new DependabotConfigWriter();
        writer.WriteConfig(config);

        return writer.TextWriter.ToString();
    }

    private static DependabotUpdate MinimalUpdate(string ecosystem = "nuget") =>
        new()
        {
            PackageEcosystem = ecosystem,
        };

    // ── Version / minimal ─────────────────────────────────────────────────────

    [Test]
    public void WriteConfig_MinimalConfig_WritesVersionLine()
    {
        var config = new DependabotConfig
        {
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);
        output.ShouldStartWith("version: 2");
    }

    [Test]
    public void WriteConfig_MinimalConfig_WritesVersionThenBlankLineThenUpdates()
    {
        var config = new DependabotConfig
        {
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);

        output.ShouldBe(GithubActionWriterHelper.JoinLines("version: 2",
            "",
            "updates:",
            "  - package-ecosystem: nuget",
            ""));
    }

    [Test]
    public void WriteConfig_WithEnableBetaEcosystems_WritesFlag()
    {
        var config = new DependabotConfig
        {
            EnableBetaEcosystems = true,
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);
        output.ShouldContain("enable-beta-ecosystems: true");

        output
            .IndexOf("enable-beta-ecosystems:", StringComparison.Ordinal)
            .ShouldBeLessThan(output.IndexOf("updates:", StringComparison.Ordinal));
    }

    [Test]
    public void WriteConfig_WithoutEnableBetaEcosystems_DoesNotWriteFlag()
    {
        var config = new DependabotConfig
        {
            Updates = [MinimalUpdate()],
        };

        WriteConfig(config)
            .ShouldNotContain("enable-beta-ecosystems");
    }

    // ── Registries ────────────────────────────────────────────────────────────

    [Test]
    public void WriteConfig_WithRegistry_WritesRegistriesSection()
    {
        var config = new DependabotConfig
        {
            Registries = new Dictionary<string, DependabotRegistry>
            {
                ["my-nuget"] = new()
                {
                    Type = RegistryType.NugetFeed,
                    Url = "https://pkgs.dev.azure.com/myorg/_packaging/my-feed/nuget/v3/index.json",
                    Token = "${{ secrets.FEED_TOKEN }}",
                },
            },
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);
        output.ShouldContain("registries:");
        output.ShouldContain("my-nuget:");
        output.ShouldContain("type: nuget-feed");
        output.ShouldContain("url: https://pkgs.dev.azure.com");
        output.ShouldContain("token: ${{ secrets.FEED_TOKEN }}");
    }

    [Test]
    public void WriteConfig_WithRegistryAllFields_WritesAllFields()
    {
        var config = new DependabotConfig
        {
            Registries = new Dictionary<string, DependabotRegistry>
            {
                ["dockerhub"] = new()
                {
                    Type = RegistryType.DockerRegistry,
                    Url = "https://registry.hub.docker.com",
                    Username = "my-user",
                    Password = "${{ secrets.DOCKER_TOKEN }}",
                    ReplacesBase = true,
                },
            },
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);
        output.ShouldContain("username: my-user");
        output.ShouldContain("password: ${{ secrets.DOCKER_TOKEN }}");
        output.ShouldContain("replaces-base: true");
    }

    [Test]
    public void WriteConfig_RegistriesAppearsBeforeUpdates()
    {
        var config = new DependabotConfig
        {
            Registries = new Dictionary<string, DependabotRegistry>
            {
                ["reg"] = new()
                {
                    Type = RegistryType.NpmRegistry,
                    Url = "https://example.com",
                },
            },
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);

        output
            .IndexOf("registries:", StringComparison.Ordinal)
            .ShouldBeLessThan(output.IndexOf("updates:", StringComparison.Ordinal));
    }

    // ── multi-ecosystem-groups ────────────────────────────────────────────────

    [Test]
    public void WriteConfig_WithMultiEcosystemGroup_WritesSection()
    {
        var config = new DependabotConfig
        {
            MultiEcosystemGroups = new Dictionary<string, DependabotMultiEcosystemGroup>
            {
                ["angular"] = new()
                {
                    Schedule = new()
                    {
                        Interval = ScheduleInterval.Weekly,
                    },
                },
            },
            Updates = [MinimalUpdate()],
        };

        var output = WriteConfig(config);
        output.ShouldContain("multi-ecosystem-groups:");
        output.ShouldContain("angular:");
    }

    // ── updates: package-ecosystem ────────────────────────────────────────────

    [Test]
    public void WriteUpdate_MultipleEcosystems_WritesAll()
    {
        var config = new DependabotConfig
        {
            Updates = [MinimalUpdate(), MinimalUpdate("npm")],
        };

        var output = WriteConfig(config);
        output.ShouldContain("package-ecosystem: nuget");
        output.ShouldContain("package-ecosystem: npm");
    }

    // ── updates: directory / directories ─────────────────────────────────────

    [Test]
    public void WriteUpdate_WithDirectory_WritesQuotedDirectory()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Directory = "/",
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("directory: \"/\"");
    }

    [Test]
    public void WriteUpdate_WithDirectories_WritesDirectoriesArray()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Directories = ["/src", "/tests"],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("directories:");
        output.ShouldContain("\"/src\"");
        output.ShouldContain("\"/tests\"");
    }

    // ── updates: schedule ─────────────────────────────────────────────────────

    [Test]
    public void WriteUpdate_WithDailySchedule_WritesScheduleSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Schedule = new()
                    {
                        Interval = ScheduleInterval.Daily,
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("schedule:");
        output.ShouldContain("interval: daily");
    }

    [Test]
    public void WriteUpdate_WithWeeklySchedule_WritesScheduleWithDay()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Schedule = new()
                    {
                        Interval = ScheduleInterval.Weekly,
                        Day = ScheduleDay.Monday,
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("interval: weekly");
        output.ShouldContain("day: monday");
    }

    [Test]
    public void WriteUpdate_WithScheduleTime_WritesTimeAndTimezone()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Schedule = new()
                    {
                        Interval = ScheduleInterval.Daily,
                        Time = "09:00",
                        Timezone = "Europe/London",
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("time: \"09:00\"");
        output.ShouldContain("timezone: Europe/London");
    }

    [Test]
    public void WriteUpdate_WithScheduleAllIntervals_FormatsCorrectly()
    {
        foreach (var (interval, expected) in new[]
                 {
                     (ScheduleInterval.Daily, "daily"),
                     (ScheduleInterval.Weekly, "weekly"),
                     (ScheduleInterval.Monthly, "monthly"),
                     (ScheduleInterval.Quarterly, "quarterly"),
                     (ScheduleInterval.Semiannually, "semiannually"),
                     (ScheduleInterval.Yearly, "yearly"),
                     (ScheduleInterval.Cron, "cron"),
                 })
        {
            var output = WriteConfig(new()
            {
                Updates =
                [
                    MinimalUpdate() with
                    {
                        Schedule = new()
                        {
                            Interval = interval,
                        },
                    },
                ],
            });

            output.ShouldContain($"interval: {expected}");
        }
    }

    // ── updates: allow ────────────────────────────────────────────────────────

    [Test]
    public void WriteUpdate_WithAllowByName_WritesAllowSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Allow =
                    [
                        new()
                        {
                            DependencyName = "Newtonsoft.Json",
                            DependencyType = null,
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("allow:");
        output.ShouldContain("- dependency-name: \"Newtonsoft.Json\"");
    }

    [Test]
    public void WriteUpdate_WithAllowByType_WritesAllowSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Allow =
                    [
                        new()
                        {
                            DependencyName = null,
                            DependencyType = DependencyType.Direct,
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("allow:");
        output.ShouldContain("dependency-type: direct");
    }

    [Test]
    public void WriteUpdate_WithAllowNameAndType_WritesBothFields()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Allow =
                    [
                        new()
                        {
                            DependencyName = "angular*",
                            DependencyType = DependencyType.Production,
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("dependency-name: \"angular*\"");
        output.ShouldContain("dependency-type: production");
    }

    // ── updates: ignore ───────────────────────────────────────────────────────

    [Test]
    public void WriteUpdate_WithIgnoreByName_WritesIgnoreSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Ignore =
                    [
                        new()
                        {
                            DependencyName = "aws-sdk",
                            UpdateTypes = null,
                            Versions = null,
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("ignore:");
        output.ShouldContain("dependency-name: \"aws-sdk\"");
    }

    [Test]
    public void WriteUpdate_WithIgnoreUpdateTypes_WritesUpdateTypes()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Ignore =
                    [
                        new()
                        {
                            DependencyName = "my-dep",
                            UpdateTypes = [SemverUpdateType.VersionUpdateSemverMajor],
                            Versions = null,
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("version-update:semver-major");
    }

    [Test]
    public void WriteUpdate_WithIgnoreSingleVersion_WritesVersionArray()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Ignore =
                    [
                        new()
                        {
                            DependencyName = "dep",
                            UpdateTypes = null,
                            Versions = new DependabotVersions.Single("1.0.0"),
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("versions: [ \"1.0.0\" ]");
    }

    [Test]
    public void WriteUpdate_WithIgnoreMultipleVersions_WritesVersionsArray()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Ignore =
                    [
                        new()
                        {
                            DependencyName = "dep",
                            UpdateTypes = null,
                            Versions = new DependabotVersions.Multiple([">=2.0.0", "<3.0.0"]),
                        },
                    ],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("versions:");
        output.ShouldContain(">=2.0.0");
        output.ShouldContain("<3.0.0");
    }

    // ── updates: groups ───────────────────────────────────────────────────────

    [Test]
    public void WriteUpdate_WithGroups_WritesGroupsSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Groups = new Dictionary<string, DependabotGroup>
                    {
                        ["angular"] = new DependabotGroup.FromPatterns
                        {
                            Patterns = ["angular*"],
                        },
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("groups:");
        output.ShouldContain("angular:");
        output.ShouldContain("patterns:");
        output.ShouldContain("\"angular*\"");
    }

    [Test]
    public void WriteUpdate_WithGroupAppliesTo_WritesAppliesToField()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Groups = new Dictionary<string, DependabotGroup>
                    {
                        ["security"] = new DependabotGroup.FromType
                        {
                            DependencyType = GroupDependencyType.Development,
                            AppliesTo = GroupAppliesTo.SecurityUpdates,
                        },
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("applies-to: security-updates");
        output.ShouldContain("dependency-type: development");
    }

    [Test]
    public void WriteUpdate_WithGroupUpdateTypes_WritesUpdateTypesField()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Groups = new Dictionary<string, DependabotGroup>
                    {
                        ["minor-patch"] = new DependabotGroup.FromUpdateTypes
                        {
                            UpdateTypes = [GroupUpdateType.Minor, GroupUpdateType.Patch],
                        },
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("update-types:");
        output.ShouldContain("minor");
        output.ShouldContain("patch");
    }

    [Test]
    public void WriteUpdate_WithGroupBy_WritesGroupByField()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Groups = new Dictionary<string, DependabotGroup>
                    {
                        ["by-name"] = new DependabotGroup.FromGroupBy
                        {
                            GroupBy = GroupBy.DependencyName,
                        },
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("group-by: dependency-name");
    }

    // ── updates: misc properties ──────────────────────────────────────────────

    [Test]
    public void WriteUpdate_WithLabels_WritesLabelsArray()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Labels = ["dependencies", "automerge"],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("labels:");
        output.ShouldContain("dependencies");
        output.ShouldContain("automerge");
    }

    [Test]
    public void WriteUpdate_WithMilestone_WritesMilestone()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Milestone = 42,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("milestone: 42");
    }

    [Test]
    public void WriteUpdate_WithOpenPullRequestsLimit_WritesLimit()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    OpenPullRequestsLimit = 5,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("open-pull-requests-limit: 5");
    }

    [Test]
    public void WriteUpdate_WithTargetBranch_WritesTargetBranch()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    TargetBranch = "develop",
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("target-branch: develop");
    }

    [Test]
    public void WriteUpdate_WithVendorTrue_WritesVendor()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Vendor = true,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("vendor: true");
    }

    [Test]
    public void WriteUpdate_WithRebaseStrategyDisabled_WritesDisabled()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    RebaseStrategy = RebaseStrategy.Disabled,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("rebase-strategy: disabled");
    }

    [Test]
    public void WriteUpdate_WithRebaseStrategyAuto_WritesAuto()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    RebaseStrategy = RebaseStrategy.Auto,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("rebase-strategy: auto");
    }

    [Test]
    public void WriteUpdate_WithVersioningStrategy_WritesVersioningStrategy()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    VersioningStrategy = VersioningStrategy.Increase,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("versioning-strategy: increase");
    }

    [Test]
    public void WriteUpdate_WithRegistriesAll_WritesStarWildcard()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Registries = new DependabotRegistries.All(),
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("registries: \"*\"");
    }

    [Test]
    public void WriteUpdate_WithRegistriesNamed_WritesRegistryNames()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Registries = new DependabotRegistries.Named("my-reg1", "my-reg2"),
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("registries:");
        output.ShouldContain("my-reg1");
        output.ShouldContain("my-reg2");
    }

    [Test]
    public void WriteUpdate_WithCommitMessage_WritesCommitMessageSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    CommitMessage = new()
                    {
                        Prefix = "dep",
                        PrefixDevelopment = null,
                        Include = CommitMessageInclude.Scope,
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("commit-message:");
        output.ShouldContain("prefix: \"dep\"");
        output.ShouldContain("include: scope");
    }

    [Test]
    public void WriteUpdate_WithName_WritesQuotedName()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Name = "My Update Config",
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("name: \"My Update Config\"");
    }

    [Test]
    public void WriteUpdate_WithAssignees_WritesAssignees()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Assignees = ["user1", "user2"],
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("assignees:");
        output.ShouldContain("user1");
        output.ShouldContain("user2");
    }

    [Test]
    public void WriteUpdate_WithCooldown_WritesCooldownSection()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    Cooldown = new()
                    {
                        DefaultDays = 7,
                        SemverMajorDays = 30,
                    },
                },
            ],
        };

        var output = WriteConfig(config);
        output.ShouldContain("cooldown:");
        output.ShouldContain("default-days: 7");
        output.ShouldContain("semver-major-days: 30");
    }

    [Test]
    public void WriteUpdate_WithMultiEcosystemGroup_WritesField()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    MultiEcosystemGroup = "my-group",
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("multi-ecosystem-group: my-group");
    }

    [Test]
    public void WriteUpdate_WithInsecureExternalCodeExecutionAllow_WritesAllow()
    {
        var config = new DependabotConfig
        {
            Updates =
            [
                MinimalUpdate() with
                {
                    InsecureExternalCodeExecution = InsecureExternalCodeExecution.Allow,
                },
            ],
        };

        WriteConfig(config)
            .ShouldContain("insecure-external-code-execution: allow");
    }

    // ── registry types ────────────────────────────────────────────────────────

    [Test]
    public void WriteRegistry_AllRegistryTypes_FormatCorrectly()
    {
        foreach (var (type, expected) in new[]
                 {
                     (RegistryType.CargoRegistry, "cargo-registry"),
                     (RegistryType.ComposerRepository, "composer-repository"),
                     (RegistryType.DockerRegistry, "docker-registry"),
                     (RegistryType.Git, "git"),
                     (RegistryType.GoproxyServer, "goproxy-server"),
                     (RegistryType.HexOrganization, "hex-organization"),
                     (RegistryType.HexRepository, "hex-repository"),
                     (RegistryType.HelmRegistry, "helm-registry"),
                     (RegistryType.MavenRepository, "maven-repository"),
                     (RegistryType.NpmRegistry, "npm-registry"),
                     (RegistryType.NugetFeed, "nuget-feed"),
                     (RegistryType.PubRepository, "pub-repository"),
                     (RegistryType.PythonIndex, "python-index"),
                     (RegistryType.RubygemsServer, "rubygems-server"),
                     (RegistryType.TerraformRegistry, "terraform-registry"),
                 })
        {
            var writer = new DependabotConfigWriter();

            writer.WriteRegistry("reg",
                new()
                {
                    Type = type,
                    Url = "https://example.com",
                });

            writer
                .TextWriter
                .ToString()
                .ShouldContain($"type: {expected}");
        }
    }
}
