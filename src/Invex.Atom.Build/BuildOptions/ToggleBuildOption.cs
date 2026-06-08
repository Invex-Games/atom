namespace Invex.Atom.Build.BuildOptions;

[PublicAPI]
public abstract record ToggleBuildOption : IBuildOption
{
    public bool Enabled { get; init; } = true;
}
