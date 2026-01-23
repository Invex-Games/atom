namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public sealed record RestoreLockedModeStep(GithubCustomStepOrder Order, int Priority = 0)
    : GithubCustomStepOption(Order, Priority)
{
    public override void WriteStep(GithubStepWriter writer) =>
        writer.WriteLine("- run: dotnet restore _atom --locked-mode");
}
