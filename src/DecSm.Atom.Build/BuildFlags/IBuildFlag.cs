namespace DecSm.Atom.Build.BuildFlags;

[PublicAPI]
public interface IBuildFlag
{
    bool Enabled { get; }
}
