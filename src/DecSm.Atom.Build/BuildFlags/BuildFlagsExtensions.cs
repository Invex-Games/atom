namespace DecSm.Atom.Build.BuildFlags;

[PublicAPI]
public static class BuildFlagsExtensions
{
    extension<T>(T)
        where T : IBuildFlag
    {
        [PublicAPI]
        public static bool IsEnabled<TBuildDefinition>(TBuildDefinition buildDefinition)
            where TBuildDefinition : IBuildDefinition =>
            buildDefinition
                .Flags
                .OfType<T>()
                .LastOrDefault() is { Enabled: true };
    }
}
