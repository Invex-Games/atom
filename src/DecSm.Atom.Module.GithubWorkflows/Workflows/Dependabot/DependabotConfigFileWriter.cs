namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot;

/// <summary>
///     Writes Dependabot configuration files in YAML format.
/// </summary>
[PublicAPI]
public sealed class DependabotConfigFileWriter(
    IAtomFileSystem atomFileSystem,
    ILogger<DependabotConfigFileWriter> logger
) : WorkflowFileWriter<DependabotWorkflowType>(atomFileSystem, logger)
{
    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".github";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        var config = workflow
            .Options
            .OfType<DependabotConfigOption>()
            .FirstOrDefault();

        if (config is null)
            throw new InvalidOperationException(
                $"Dependabot workflow '{workflow.Name}' is missing a {nameof(DependabotConfigOption)}.");

        WriteConfig(config.Config);
    }

    private void WriteConfig(DependabotConfig config)
    {
        Writer.WriteLine($"version: {config.Version}");
        Writer.WriteLine();

        if (config.EnableBetaEcosystems is true)
        {
            Writer.WriteLine("enable-beta-ecosystems: true");
            Writer.WriteLine();
        }

        if (config.Registries is { Count: > 0 } registries)
        {
            using (Writer.WriteSection("registries:"))
                foreach (var (name, registry) in registries)
                    WriteRegistry(name, registry);

            Writer.WriteLine();
        }

        if (config.MultiEcosystemGroups is { Count: > 0 } multiEcosystemGroups)
        {
            using (Writer.WriteSection("multi-ecosystem-groups:"))
                foreach (var (name, group) in multiEcosystemGroups)
                    WriteMultiEcosystemGroup(name, group);

            Writer.WriteLine();
        }

        using (Writer.WriteSection("updates:"))
            foreach (var update in config.Updates)
                WriteUpdate(update);
    }

    private void WriteRegistry(string name, DependabotRegistry registry)
    {
        using var _ = Writer.WriteSection($"{name}:");

        Writer.WriteLine($"type: {FormatRegistryType(registry.Type)}");
        Writer.WriteLine($"url: {registry.Url}");

        if (registry.Username is { Length: > 0 } username)
            Writer.WriteLine($"username: {username}");

        if (registry.Password is { Length: > 0 } password)
            Writer.WriteLine($"password: {password}");

        if (registry.Key is { Length: > 0 } key)
            Writer.WriteLine($"key: {key}");

        if (registry.Token is { Length: > 0 } token)
            Writer.WriteLine($"token: {token}");

        if (registry.ReplacesBase is { } replacesBase)
            Writer.WriteLine($"replaces-base: {replacesBase.ToString().ToLowerInvariant()}");

        if (registry.Organization is { Length: > 0 } organization)
            Writer.WriteLine($"organization: {organization}");

        if (registry.Repo is { Length: > 0 } repo)
            Writer.WriteLine($"repo: {repo}");

        if (registry.AuthKey is { Length: > 0 } authKey)
            Writer.WriteLine($"auth-key: {authKey}");

        if (registry.PublicKeyFingerprint is { Length: > 0 } publicKeyFingerprint)
            Writer.WriteLine($"public-key-fingerprint: {publicKeyFingerprint}");

        if (registry.Registry is { Length: > 0 } registryName)
            Writer.WriteLine($"registry: {registryName}");

        if (registry.TenantId is { Length: > 0 } tenantId)
            Writer.WriteLine($"tenant-id: {tenantId}");

        if (registry.ClientId is { Length: > 0 } clientId)
            Writer.WriteLine($"client-id: {clientId}");

        if (registry.JfrogOidcProviderName is { Length: > 0 } jfrogOidcProviderName)
            Writer.WriteLine($"jfrog-oidc-provider-name: {jfrogOidcProviderName}");

        if (registry.IdentityMappingName is { Length: > 0 } identityMappingName)
            Writer.WriteLine($"identity-mapping-name: {identityMappingName}");

        if (registry.Audience is { Length: > 0 } audience)
            Writer.WriteLine($"audience: {audience}");

        if (registry.AwsRegion is { Length: > 0 } awsRegion)
            Writer.WriteLine($"aws-region: {awsRegion}");

        if (registry.AccountId is { Length: > 0 } accountId)
            Writer.WriteLine($"account-id: {accountId}");

        if (registry.RoleName is { Length: > 0 } roleName)
            Writer.WriteLine($"role-name: {roleName}");

        if (registry.Domain is { Length: > 0 } domain)
            Writer.WriteLine($"domain: {domain}");

        if (registry.DomainOwner is { Length: > 0 } domainOwner)
            Writer.WriteLine($"domain-owner: {domainOwner}");
    }

    private void WriteMultiEcosystemGroup(string name, DependabotMultiEcosystemGroup group)
    {
        using var _ = Writer.WriteSection($"{name}:");

        WriteSchedule(group.Schedule);

        if (group.Labels is { Count: > 0 } labels)
            WriteArrayProperty("labels", labels);

        if (group.Assignees is { Count: > 0 } assignees)
            WriteArrayProperty("assignees", assignees);

        if (group.Milestone is { } milestone)
            Writer.WriteLine($"milestone: {milestone}");

        if (group.TargetBranch is { Length: > 0 } targetBranch)
            Writer.WriteLine($"target-branch: {targetBranch}");

        if (group.CommitMessage is { } commitMessage)
            WriteCommitMessage(commitMessage);

        if (group.PullRequestBranchName is { } pullRequestBranchName)
            WritePullRequestBranchName(pullRequestBranchName);

        if (group.OpenPullRequestsLimit is { } openPullRequestsLimit)
            Writer.WriteLine($"open-pull-requests-limit: {openPullRequestsLimit}");

        if (group.UpdateTypes is { Count: > 0 } updateTypes)
            WriteArrayProperty("update-types", updateTypes.Select(FormatGroupUpdateType));

        if (group.DependencyType is { } dependencyType)
            Writer.WriteLine($"dependency-type: {FormatGroupDependencyType(dependencyType)}");

        if (group.ExcludePatterns is { Count: > 0 } excludePatterns)
            WriteArrayProperty("exclude-patterns", excludePatterns);
    }

    private void WriteUpdate(DependabotUpdate update)
    {
        using var _ = Writer.WriteSection($"- package-ecosystem: {update.PackageEcosystem}");

        if (update.Directory is { Length: > 0 } directory)
            Writer.WriteLine($"directory: \"{directory}\"");
        else if (update.Directories is { Count: > 0 } directories)
            WriteArrayProperty("directories", directories.Select(d => $"\"{d}\""));

        if (update.Schedule is { } schedule)
            WriteSchedule(schedule);

        if (update.Allow is { Count: > 0 } allow)
            WriteAllowRules(allow);

        if (update.Assignees is { Count: > 0 } assignees)
            WriteArrayProperty("assignees", assignees);

        if (update.CommitMessage is { } commitMessage)
            WriteCommitMessage(commitMessage);

        if (update.Cooldown is { } cooldown)
            WriteCooldown(cooldown);

        if (update.ExcludePaths is { Count: > 0 } excludePaths)
            WriteArrayProperty("exclude-paths", excludePaths);

        if (update.Groups is { Count: > 0 } groups)
            WriteGroups(groups);

        if (update.Ignore is { Count: > 0 } ignore)
            WriteIgnoreRules(ignore);

        if (update.InsecureExternalCodeExecution is { } insecureExternalCodeExecution)
            Writer.WriteLine(
                $"insecure-external-code-execution: {FormatInsecureExternalCodeExecution(insecureExternalCodeExecution)}");

        if (update.Labels is { Count: > 0 } labels)
            WriteArrayProperty("labels", labels);

        if (update.Milestone is { } milestone)
            Writer.WriteLine($"milestone: {milestone}");

        if (update.Name is { Length: > 0 } name)
            Writer.WriteLine($"name: \"{name}\"");

        if (update.OpenPullRequestsLimit is { } openPullRequestsLimit)
            Writer.WriteLine($"open-pull-requests-limit: {openPullRequestsLimit}");

        if (update.PullRequestBranchName is { } pullRequestBranchName)
            WritePullRequestBranchName(pullRequestBranchName);

        if (update.RebaseStrategy is { } rebaseStrategy)
            Writer.WriteLine($"rebase-strategy: {FormatRebaseStrategy(rebaseStrategy)}");

        if (update.Registries is { } registries)
            WriteRegistriesReference(registries);

        if (update.TargetBranch is { Length: > 0 } targetBranch)
            Writer.WriteLine($"target-branch: {targetBranch}");

        if (update.Vendor is { } vendor)
            Writer.WriteLine($"vendor: {vendor.ToString().ToLowerInvariant()}");

        if (update.VersioningStrategy is { } versioningStrategy)
            Writer.WriteLine($"versioning-strategy: {FormatVersioningStrategy(versioningStrategy)}");

        if (update.Patterns is { Count: > 0 } patterns)
            WriteArrayProperty("patterns", patterns);

        if (update.MultiEcosystemGroup is { Length: > 0 } multiEcosystemGroup)
            Writer.WriteLine($"multi-ecosystem-group: {multiEcosystemGroup}");

        Writer.WriteLine();
    }

    private void WriteSchedule(DependabotSchedule schedule)
    {
        using var _ = Writer.WriteSection("schedule:");

        Writer.WriteLine($"interval: {FormatScheduleInterval(schedule.Interval)}");

        if (schedule.Day is { } day)
            Writer.WriteLine($"day: {FormatScheduleDay(day)}");

        if (schedule.Time is { Length: > 0 } time)
            Writer.WriteLine($"time: \"{time}\"");

        if (schedule.Timezone is { Length: > 0 } timezone)
            Writer.WriteLine($"timezone: {timezone}");

        if (schedule.Cronjob is { Length: > 0 } cronjob)
            Writer.WriteLine($"cronjob: \"{cronjob}\"");
    }

    private void WriteCommitMessage(DependabotCommitMessage commitMessage)
    {
        using var _ = Writer.WriteSection("commit-message:");

        if (commitMessage.Prefix is { Length: > 0 } prefix)
            Writer.WriteLine($"prefix: \"{prefix}\"");

        if (commitMessage.PrefixDevelopment is { Length: > 0 } prefixDevelopment)
            Writer.WriteLine($"prefix-development: \"{prefixDevelopment}\"");

        if (commitMessage.Include is { } include)
            Writer.WriteLine($"include: {FormatCommitMessageInclude(include)}");
    }

    private void WriteCooldown(DependabotCooldown cooldown)
    {
        using var _ = Writer.WriteSection("cooldown:");

        if (cooldown.DefaultDays is { } defaultDays)
            Writer.WriteLine($"default-days: {defaultDays}");

        if (cooldown.SemverMajorDays is { } semverMajorDays)
            Writer.WriteLine($"semver-major-days: {semverMajorDays}");

        if (cooldown.SemverMinorDays is { } semverMinorDays)
            Writer.WriteLine($"semver-minor-days: {semverMinorDays}");

        if (cooldown.SemverPatchDays is { } semverPatchDays)
            Writer.WriteLine($"semver-patch-days: {semverPatchDays}");

        if (cooldown.Include is { Count: > 0 } include)
            WriteArrayProperty("include", include);

        if (cooldown.Exclude is { Count: > 0 } exclude)
            WriteArrayProperty("exclude", exclude);
    }

    private void WriteAllowRules(IReadOnlyList<DependabotAllow> allow)
    {
        using var _ = Writer.WriteSection("allow:");

        foreach (var rule in allow)
            if (rule.DependencyName is { Length: > 0 } dependencyName)
            {
                if (rule.DependencyType is { } dependencyType)
                    using (Writer.WriteSection($"- dependency-name: \"{dependencyName}\""))
                        Writer.WriteLine($"dependency-type: {FormatDependencyType(dependencyType)}");
                else
                    Writer.WriteLine($"- dependency-name: \"{dependencyName}\"");
            }
            else if (rule.DependencyType is { } dependencyType)
            {
                Writer.WriteLine($"- dependency-type: {FormatDependencyType(dependencyType)}");
            }
    }

    private void WriteIgnoreRules(IReadOnlyList<DependabotIgnore> ignore)
    {
        using var _ = Writer.WriteSection("ignore:");

        foreach (var rule in ignore)
        {
            var hasName = rule.DependencyName is { Length: > 0 };
            var hasUpdateTypes = rule.UpdateTypes is { Count: > 0 };
            var hasVersions = rule.Versions is not null;

            if (hasName)
            {
                if (hasUpdateTypes || hasVersions)
                    using (Writer.WriteSection($"- dependency-name: \"{rule.DependencyName}\""))
                    {
                        if (hasUpdateTypes)
                            WriteArrayProperty("update-types", rule.UpdateTypes!.Select(FormatSemverUpdateType));

                        if (hasVersions)
                            WriteVersions(rule.Versions!);
                    }
                else
                    Writer.WriteLine($"- dependency-name: \"{rule.DependencyName}\"");
            }
            else if (hasUpdateTypes)
            {
                using (Writer.WriteSection("- update-types:"))
                    foreach (var updateType in rule.UpdateTypes!)
                        Writer.WriteLine($"- {FormatSemverUpdateType(updateType)}");
            }
        }
    }

    private void WriteVersions(DependabotVersions versions)
    {
        switch (versions)
        {
            case DependabotVersions.Single single:
                Writer.WriteLine($"versions: [ \"{single.Version}\" ]");

                break;
            case DependabotVersions.Multiple multiple:
                WriteArrayProperty("versions", multiple.Versions.Select(v => $"\"{v}\""));

                break;
        }
    }

    private void WriteGroups(IReadOnlyDictionary<string, DependabotGroup> groups)
    {
        using var _ = Writer.WriteSection("groups:");

        foreach (var (name, group) in groups)
        {
            using var __ = Writer.WriteSection($"{name}:");

            if (group.AppliesTo is { } appliesTo)
                Writer.WriteLine($"applies-to: {FormatGroupAppliesTo(appliesTo)}");

            if (group.DependencyType is { } dependencyType)
                Writer.WriteLine($"dependency-type: {FormatGroupDependencyType(dependencyType)}");

            if (group.Patterns is { Count: > 0 } patterns)
                WriteArrayProperty("patterns", patterns.Select(p => $"\"{p}\""));

            if (group.ExcludePatterns is { Count: > 0 } excludePatterns)
                WriteArrayProperty("exclude-patterns", excludePatterns.Select(p => $"\"{p}\""));

            if (group.UpdateTypes is { Count: > 0 } updateTypes)
                WriteArrayProperty("update-types", updateTypes.Select(FormatGroupUpdateType));

            if (group.GroupBy is { } groupBy)
                Writer.WriteLine($"group-by: {FormatGroupBy(groupBy)}");
        }
    }

    private void WritePullRequestBranchName(DependabotPullRequestBranchName pullRequestBranchName)
    {
        using var _ = Writer.WriteSection("pull-request-branch-name:");

        Writer.WriteLine($"separator: \"{FormatBranchNameSeparator(pullRequestBranchName.Separator)}\"");
    }

    private void WriteRegistriesReference(DependabotRegistries registries)
    {
        switch (registries)
        {
            case DependabotRegistries.All:
                Writer.WriteLine("registries: \"*\"");

                break;
            case DependabotRegistries.Named named:
                WriteArrayProperty("registries", named.Names);

                break;
        }
    }

    private void WriteArrayProperty(string key, IEnumerable<string> values)
    {
        var valueList = values.ToList();
        var valuesTotalLength = valueList.Sum(x => x.Length);

        if (valuesTotalLength < 80 && valueList.Count <= 5)
            Writer.WriteLine($"{key}: [ {string.Join(", ", valueList)} ]");
        else
            using (Writer.WriteSection($"{key}:"))
                foreach (var value in valueList)
                    Writer.WriteLine($"- {value}");
    }

    private static string FormatScheduleInterval(ScheduleInterval interval) =>
        interval switch
        {
            ScheduleInterval.Daily => "daily",
            ScheduleInterval.Weekly => "weekly",
            ScheduleInterval.Monthly => "monthly",
            ScheduleInterval.Quarterly => "quarterly",
            ScheduleInterval.Semiannually => "semiannually",
            ScheduleInterval.Yearly => "yearly",
            ScheduleInterval.Cron => "cron",
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
        };

    private static string FormatScheduleDay(ScheduleDay day) =>
        day switch
        {
            ScheduleDay.Monday => "monday",
            ScheduleDay.Tuesday => "tuesday",
            ScheduleDay.Wednesday => "wednesday",
            ScheduleDay.Thursday => "thursday",
            ScheduleDay.Friday => "friday",
            ScheduleDay.Saturday => "saturday",
            ScheduleDay.Sunday => "sunday",
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null),
        };

    private static string FormatDependencyType(DependencyType dependencyType) =>
        dependencyType switch
        {
            DependencyType.Direct => "direct",
            DependencyType.Indirect => "indirect",
            DependencyType.All => "all",
            DependencyType.Production => "production",
            DependencyType.Development => "development",
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyType), dependencyType, null),
        };

    private static string FormatSemverUpdateType(SemverUpdateType updateType) =>
        updateType switch
        {
            SemverUpdateType.VersionUpdateSemverMajor => "version-update:semver-major",
            SemverUpdateType.VersionUpdateSemverMinor => "version-update:semver-minor",
            SemverUpdateType.VersionUpdateSemverPatch => "version-update:semver-patch",
            _ => throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null),
        };

    private static string FormatGroupUpdateType(GroupUpdateType updateType) =>
        updateType switch
        {
            GroupUpdateType.Major => "major",
            GroupUpdateType.Minor => "minor",
            GroupUpdateType.Patch => "patch",
            _ => throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null),
        };

    private static string FormatGroupDependencyType(GroupDependencyType dependencyType) =>
        dependencyType switch
        {
            GroupDependencyType.Development => "development",
            GroupDependencyType.Production => "production",
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyType), dependencyType, null),
        };

    private static string FormatGroupAppliesTo(GroupAppliesTo appliesTo) =>
        appliesTo switch
        {
            GroupAppliesTo.VersionUpdates => "version-updates",
            GroupAppliesTo.SecurityUpdates => "security-updates",
            _ => throw new ArgumentOutOfRangeException(nameof(appliesTo), appliesTo, null),
        };

    private static string FormatGroupBy(GroupBy groupBy) =>
        groupBy switch
        {
            GroupBy.DependencyName => "dependency-name",
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null),
        };

    private static string FormatInsecureExternalCodeExecution(
        InsecureExternalCodeExecution insecureExternalCodeExecution) =>
        insecureExternalCodeExecution switch
        {
            InsecureExternalCodeExecution.Allow => "allow",
            InsecureExternalCodeExecution.Deny => "deny",
            _ => throw new ArgumentOutOfRangeException(nameof(insecureExternalCodeExecution),
                insecureExternalCodeExecution,
                null),
        };

    private static string FormatRebaseStrategy(RebaseStrategy rebaseStrategy) =>
        rebaseStrategy switch
        {
            RebaseStrategy.Auto => "auto",
            RebaseStrategy.Disabled => "disabled",
            _ => throw new ArgumentOutOfRangeException(nameof(rebaseStrategy), rebaseStrategy, null),
        };

    private static string FormatVersioningStrategy(VersioningStrategy versioningStrategy) =>
        versioningStrategy switch
        {
            VersioningStrategy.Auto => "auto",
            VersioningStrategy.Increase => "increase",
            VersioningStrategy.IncreaseIfNecessary => "increase-if-necessary",
            VersioningStrategy.LockfileOnly => "lockfile-only",
            VersioningStrategy.Widen => "widen",
            _ => throw new ArgumentOutOfRangeException(nameof(versioningStrategy), versioningStrategy, null),
        };

    private static string FormatCommitMessageInclude(CommitMessageInclude include) =>
        include switch
        {
            CommitMessageInclude.Scope => "scope",
            _ => throw new ArgumentOutOfRangeException(nameof(include), include, null),
        };

    private static string FormatBranchNameSeparator(BranchNameSeparator separator) =>
        separator switch
        {
            BranchNameSeparator.Hyphen => "-",
            BranchNameSeparator.Underscore => "_",
            BranchNameSeparator.Slash => "/",
            _ => throw new ArgumentOutOfRangeException(nameof(separator), separator, null),
        };

    private static string FormatRegistryType(RegistryType registryType) =>
        registryType switch
        {
            RegistryType.CargoRegistry => "cargo-registry",
            RegistryType.ComposerRepository => "composer-repository",
            RegistryType.DockerRegistry => "docker-registry",
            RegistryType.Git => "git",
            RegistryType.GoproxyServer => "goproxy-server",
            RegistryType.HexOrganization => "hex-organization",
            RegistryType.HexRepository => "hex-repository",
            RegistryType.HelmRegistry => "helm-registry",
            RegistryType.MavenRepository => "maven-repository",
            RegistryType.NpmRegistry => "npm-registry",
            RegistryType.NugetFeed => "nuget-feed",
            RegistryType.PubRepository => "pub-repository",
            RegistryType.PythonIndex => "python-index",
            RegistryType.RubygemsServer => "rubygems-server",
            RegistryType.TerraformRegistry => "terraform-registry",
            _ => throw new ArgumentOutOfRangeException(nameof(registryType), registryType, null),
        };
}
