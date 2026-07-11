namespace Atom.Tests;

[TestFixture]
internal sealed class ReleaseAssetManifestTests
{
    [Test]
    public void CreateArtifactNames_IncludesToolAndAllInputsExactlyOnce()
    {
        var result = ReleaseAssetManifest.CreateArtifactNames(["Package.One", "Package.Two"],
            "Invex.Atom.Tool",
            ["Tests.One", "Invex.Atom.Tool"]);

        result.ShouldBe(["Package.One", "Package.Two", "Invex.Atom.Tool", "Tests.One"], false);
    }

    [Test]
    public void CreateExpectedAssetNames_ReturnsSortedZipNames()
    {
        var result = ReleaseAssetManifest.CreateExpectedAssetNames(["Tests", "Invex.Atom.Tool"]);

        result.ShouldBe(["Invex.Atom.Tool.zip", "Tests.zip"], false);
    }

    [Test]
    public void ValidateUploadedAssets_WithExactManifest_DoesNotThrow() =>
        Should.NotThrow(() => ReleaseAssetManifest.ValidateUploadedAssets(["Invex.Atom.Tool.zip", "Package.zip"],
            ["Package.zip", "Invex.Atom.Tool.zip"]));

    [Test]
    public void ValidateUploadedAssets_WithMissingAsset_Throws() =>
        Should.Throw<StepFailedException>(() =>
            ReleaseAssetManifest.ValidateUploadedAssets(["Invex.Atom.Tool.zip", "Package.zip"], ["Package.zip"]));

    [Test]
    public void ValidateUploadedAssets_WithDuplicateAsset_Throws() =>
        Should.Throw<StepFailedException>(() => ReleaseAssetManifest.ValidateUploadedAssets(
            ["Invex.Atom.Tool.zip"],
            ["Invex.Atom.Tool.zip", "Invex.Atom.Tool.zip"]));
}
