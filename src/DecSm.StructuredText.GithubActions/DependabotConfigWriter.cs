namespace DecSm.StructuredText.GithubActions;

[PublicAPI]
public sealed class DependabotConfigWriter
{
    public StructuredTextWriter TextWriter { get; init; } = new();

    public void WriteConfig(DependabotConfig config)
    {
        TextWriter.WriteLine($"version: {config.Version}");
        TextWriter.WriteLine();

        if (config.EnableBetaEcosystems is true)
        {
            TextWriter.WriteLine("enable-beta-ecosystems: true");
            TextWriter.WriteLine();
        }

        if (config.Registries is { Count: > 0 } registries)
        {
            using (TextWriter.WriteSection("registries:"))
                foreach (var (name, registry) in registries)
                    WriteRegistry(name, registry);

            TextWriter.WriteLine();
        }

        if (config.MultiEcosystemGroups is { Count: > 0 } multiEcosystemGroups)
        {
            using (TextWriter.WriteSection("multi-ecosystem-groups:"))
                foreach (var (name, group) in multiEcosystemGroups)
                    WriteMultiEcosystemGroup(name, group);

            TextWriter.WriteLine();
        }

        using (TextWriter.WriteSection("updates:"))
            foreach (var update in config.Updates)
                WriteUpdate(update);
    }

    public void WriteRegistry(string name, DependabotRegistry registry)
    {
        using var _ = TextWriter.WriteSection($"{name}:");

        TextWriter.WriteLine($"type: {FormatRegistryType(registry.Type)}");
        TextWriter.WriteLine($"url: {registry.Url}");

        if (registry.Username is { Length: > 0 } username)
            TextWriter.WriteLine($"username: {username}");

        if (registry.Password is { Length: > 0 } password)
            TextWriter.WriteLine($"password: {password}");

        if (registry.Key is { Length: > 0 } key)
            TextWriter.WriteLine($"key: {key}");

        if (registry.Token is { Length: > 0 } token)
            TextWriter.WriteLine($"token: {token}");

        if (registry.ReplacesBase is { } replacesBase)
            TextWriter.WriteLine($"replaces-base: {replacesBase.ToString().ToLowerInvariant()}");

        if (registry.Organization is { Length: > 0 } organization)
            TextWriter.WriteLine($"organization: {organization}");

        if (registry.Repo is { Length: > 0 } repo)
            TextWriter.WriteLine($"repo: {repo}");

        if (registry.AuthKey is { Length: > 0 } authKey)
            TextWriter.WriteLine($"auth-key: {authKey}");

        if (registry.PublicKeyFingerprint is { Length: > 0 } publicKeyFingerprint)
            TextWriter.WriteLine($"public-key-fingerprint: {publicKeyFingerprint}");

        if (registry.Registry is { Length: > 0 } registryName)
            TextWriter.WriteLine($"registry: {registryName}");

        if (registry.TenantId is { Length: > 0 } tenantId)
            TextWriter.WriteLine($"tenant-id: {tenantId}");

        if (registry.ClientId is { Length: > 0 } clientId)
            TextWriter.WriteLine($"client-id: {clientId}");

        if (registry.JfrogOidcProviderName is { Length: > 0 } jfrogOidcProviderName)
            TextWriter.WriteLine($"jfrog-oidc-provider-name: {jfrogOidcProviderName}");

        if (registry.IdentityMappingName is { Length: > 0 } identityMappingName)
            TextWriter.WriteLine($"identity-mapping-name: {identityMappingName}");

        if (registry.Audience is { Length: > 0 } audience)
            TextWriter.WriteLine($"audience: {audience}");

        if (registry.AwsRegion is { Length: > 0 } awsRegion)
            TextWriter.WriteLine($"aws-region: {awsRegion}");

        if (registry.AccountId is { Length: > 0 } accountId)
            TextWriter.WriteLine($"account-id: {accountId}");

        if (registry.RoleName is { Length: > 0 } roleName)
            TextWriter.WriteLine($"role-name: {roleName}");

        if (registry.Domain is { Length: > 0 } domain)
            TextWriter.WriteLine($"domain: {domain}");

        if (registry.DomainOwner is { Length: > 0 } domainOwner)
            TextWriter.WriteLine($"domain-owner: {domainOwner}");
    }

    private void WriteMultiEcosystemGroup(string name, DependabotMultiEcosystemGroup group)
    {
        using var _ = TextWriter.WriteSection($"{name}:");

        WriteSchedule(group.Schedule);

        if (group.Labels is { Count: > 0 } labels)
            WriteArrayProperty("labels", labels);

        if (group.Assignees is { Count: > 0 } assignees)
            WriteArrayProperty("assignees", assignees);

        if (group.Milestone is { } milestone)
            TextWriter.WriteLine($"milestone: {milestone}");

        if (group.TargetBranch is { Length: > 0 } targetBranch)
            TextWriter.WriteLine($"target-branch: {targetBranch}");

        if (group.CommitMessage is { } commitMessage)
            WriteCommitMessage(commitMessage);

        if (group.PullRequestBranchName is { } pullRequestBranchName)
            WritePullRequestBranchName(pullRequestBranchName);

        if (group.OpenPullRequestsLimit is { } openPullRequestsLimit)
            TextWriter.WriteLine($"open-pull-requests-limit: {openPullRequestsLimit}");

        if (group.UpdateTypes is { Count: > 0 } updateTypes)
            WriteArrayProperty("update-types", updateTypes.Select(FormatGroupUpdateType));

        if (group.DependencyType is { } dependencyType)
            TextWriter.WriteLine($"dependency-type: {FormatGroupDependencyType(dependencyType)}");

        if (group.ExcludePatterns is { Count: > 0 } excludePatterns)
            WriteArrayProperty("exclude-patterns", excludePatterns);
    }

    private void WriteUpdate(DependabotUpdate update)
    {
        using var _ = TextWriter.WriteSection($"- package-ecosystem: {update.PackageEcosystem}");

        if (update.Directory is { Length: > 0 } directory)
            TextWriter.WriteLine($"directory: \"{directory}\"");
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
            TextWriter.WriteLine(
                $"insecure-external-code-execution: {FormatInsecureExternalCodeExecution(insecureExternalCodeExecution)}");

        if (update.Labels is { Count: > 0 } labels)
            WriteArrayProperty("labels", labels);

        if (update.Milestone is { } milestone)
            TextWriter.WriteLine($"milestone: {milestone}");

        if (update.Name is { Length: > 0 } name)
            TextWriter.WriteLine($"name: \"{name}\"");

        if (update.OpenPullRequestsLimit is { } openPullRequestsLimit)
            TextWriter.WriteLine($"open-pull-requests-limit: {openPullRequestsLimit}");

        if (update.PullRequestBranchName is { } pullRequestBranchName)
            WritePullRequestBranchName(pullRequestBranchName);

        if (update.RebaseStrategy is { } rebaseStrategy)
            TextWriter.WriteLine($"rebase-strategy: {FormatRebaseStrategy(rebaseStrategy)}");

        if (update.Registries is { } registries)
            WriteRegistriesReference(registries);

        if (update.TargetBranch is { Length: > 0 } targetBranch)
            TextWriter.WriteLine($"target-branch: {targetBranch}");

        if (update.Vendor is { } vendor)
            TextWriter.WriteLine($"vendor: {vendor.ToString().ToLowerInvariant()}");

        if (update.VersioningStrategy is { } versioningStrategy)
            TextWriter.WriteLine($"versioning-strategy: {FormatVersioningStrategy(versioningStrategy)}");

        if (update.Patterns is { Count: > 0 } patterns)
            WriteArrayProperty("patterns", patterns);

        if (update.MultiEcosystemGroup is { Length: > 0 } multiEcosystemGroup)
            TextWriter.WriteLine($"multi-ecosystem-group: {multiEcosystemGroup}");

        TextWriter.WriteLine();
    }

    private void WriteSchedule(DependabotSchedule schedule)
    {
        using var _ = TextWriter.WriteSection("schedule:");

        TextWriter.WriteLine($"interval: {FormatScheduleInterval(schedule.Interval)}");

        if (schedule.Day is { } day)
            TextWriter.WriteLine($"day: {FormatScheduleDay(day)}");

        if (schedule.Time is { Length: > 0 } time)
            TextWriter.WriteLine($"time: \"{time}\"");

        if (schedule.Timezone is { Length: > 0 } timezone)
            TextWriter.WriteLine($"timezone: {timezone}");

        if (schedule.Cronjob is { Length: > 0 } cronjob)
            TextWriter.WriteLine($"cronjob: \"{cronjob}\"");
    }

    private void WriteCommitMessage(DependabotCommitMessage commitMessage)
    {
        using var _ = TextWriter.WriteSection("commit-message:");

        if (commitMessage.Prefix is { Length: > 0 } prefix)
            TextWriter.WriteLine($"prefix: \"{prefix}\"");

        if (commitMessage.PrefixDevelopment is { Length: > 0 } prefixDevelopment)
            TextWriter.WriteLine($"prefix-development: \"{prefixDevelopment}\"");

        if (commitMessage.Include is { } include)
            TextWriter.WriteLine($"include: {FormatCommitMessageInclude(include)}");
    }

    private void WriteCooldown(DependabotCooldown cooldown)
    {
        using var _ = TextWriter.WriteSection("cooldown:");

        if (cooldown.DefaultDays is { } defaultDays)
            TextWriter.WriteLine($"default-days: {defaultDays}");

        if (cooldown.SemverMajorDays is { } semverMajorDays)
            TextWriter.WriteLine($"semver-major-days: {semverMajorDays}");

        if (cooldown.SemverMinorDays is { } semverMinorDays)
            TextWriter.WriteLine($"semver-minor-days: {semverMinorDays}");

        if (cooldown.SemverPatchDays is { } semverPatchDays)
            TextWriter.WriteLine($"semver-patch-days: {semverPatchDays}");

        if (cooldown.Include is { Count: > 0 } include)
            WriteArrayProperty("include", include);

        if (cooldown.Exclude is { Count: > 0 } exclude)
            WriteArrayProperty("exclude", exclude);
    }

    private void WriteAllowRules(IReadOnlyList<DependabotAllow> allow)
    {
        using var _ = TextWriter.WriteSection("allow:");

        foreach (var rule in allow)
            if (rule.DependencyName is { Length: > 0 } dependencyName)
            {
                if (rule.DependencyType is { } dependencyType)
                    using (TextWriter.WriteSection($"- dependency-name: \"{dependencyName}\""))
                        TextWriter.WriteLine($"dependency-type: {FormatDependencyType(dependencyType)}");
                else
                    TextWriter.WriteLine($"- dependency-name: \"{dependencyName}\"");
            }
            else if (rule.DependencyType is { } dependencyType)
            {
                TextWriter.WriteLine($"- dependency-type: {FormatDependencyType(dependencyType)}");
            }
    }

    private void WriteIgnoreRules(IReadOnlyList<DependabotIgnore> ignore)
    {
        using var _ = TextWriter.WriteSection("ignore:");

        foreach (var rule in ignore)
        {
            var hasName = rule.DependencyName is { Length: > 0 };
            var hasUpdateTypes = rule.UpdateTypes is { Count: > 0 };
            var hasVersions = rule.Versions is not null;

            if (hasName)
            {
                if (hasUpdateTypes || hasVersions)
                    using (TextWriter.WriteSection($"- dependency-name: \"{rule.DependencyName}\""))
                    {
                        if (hasUpdateTypes)
                            WriteArrayProperty("update-types", rule.UpdateTypes!.Select(FormatSemverUpdateType));

                        if (hasVersions)
                            WriteVersions(rule.Versions!);
                    }
                else
                    TextWriter.WriteLine($"- dependency-name: \"{rule.DependencyName}\"");
            }
            else if (hasUpdateTypes)
            {
                using (TextWriter.WriteSection("- update-types:"))
                    foreach (var updateType in rule.UpdateTypes!)
                        TextWriter.WriteLine($"- {FormatSemverUpdateType(updateType)}");
            }
        }
    }

    private void WriteVersions(DependabotVersions versions)
    {
        switch (versions)
        {
            case DependabotVersions.Single single:
                TextWriter.WriteLine($"versions: [ \"{single.Version}\" ]");

                break;
            case DependabotVersions.Multiple multiple:
                WriteArrayProperty("versions", multiple.Versions.Select(v => $"\"{v}\""));

                break;
        }
    }

    private void WriteGroups(IReadOnlyDictionary<string, DependabotGroup> groups)
    {
        using var _ = TextWriter.WriteSection("groups:");

        foreach (var (name, group) in groups)
        {
            using var __ = TextWriter.WriteSection($"{name}:");

            if (group.AppliesTo is { } appliesTo)
                TextWriter.WriteLine($"applies-to: {FormatGroupAppliesTo(appliesTo)}");

            if (group.DependencyType is { } dependencyType)
                TextWriter.WriteLine($"dependency-type: {FormatGroupDependencyType(dependencyType)}");

            if (group.Patterns is { Count: > 0 } patterns)
                WriteArrayProperty("patterns", patterns.Select(p => $"\"{p}\""));

            if (group.ExcludePatterns is { Count: > 0 } excludePatterns)
                WriteArrayProperty("exclude-patterns", excludePatterns.Select(p => $"\"{p}\""));

            if (group.UpdateTypes is { Count: > 0 } updateTypes)
                WriteArrayProperty("update-types", updateTypes.Select(FormatGroupUpdateType));

            if (group.GroupBy is { } groupBy)
                TextWriter.WriteLine($"group-by: {FormatGroupBy(groupBy)}");
        }
    }

    private void WritePullRequestBranchName(DependabotPullRequestBranchName pullRequestBranchName)
    {
        using var _ = TextWriter.WriteSection("pull-request-branch-name:");

        TextWriter.WriteLine($"separator: \"{FormatBranchNameSeparator(pullRequestBranchName.Separator)}\"");
    }

    private void WriteRegistriesReference(DependabotRegistries registries)
    {
        switch (registries)
        {
            case DependabotRegistries.All:
                TextWriter.WriteLine("registries: \"*\"");

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
            TextWriter.WriteLine($"{key}: [ {string.Join(", ", valueList)} ]");
        else
            using (TextWriter.WriteSection($"{key}:"))
                foreach (var value in valueList)
                    TextWriter.WriteLine($"- {value}");
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
