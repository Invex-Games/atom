namespace DecSm.Atom.Module.Dotnet.Util;

[PublicAPI]
public static class TransformProjectVersionScope
{
    public static TransformFileScope Create(RootedPath file, SemVer version) =>
        TransformFileScope.Create(file, text => MsBuildUtil.SetVersionInfo(text, version));

    public static async Task<TransformFileScope> CreateAsync(
        RootedPath file,
        SemVer version,
        CancellationToken cancellationToken = default) =>
        await TransformFileScope.CreateAsync(file,
            text => MsBuildUtil.SetVersionInfo(text, version),
            cancellationToken);

    public static TransformMultiFileScope Create(IEnumerable<RootedPath> files, SemVer version) =>
        TransformMultiFileScope.Create(files, text => MsBuildUtil.SetVersionInfo(text, version));

    public static async Task<TransformMultiFileScope> CreateAsync(
        IEnumerable<RootedPath> files,
        SemVer version,
        CancellationToken cancellationToken = default) =>
        await TransformMultiFileScope.CreateAsync(files,
            text => MsBuildUtil.SetVersionInfo(text, version),
            cancellationToken);
}
