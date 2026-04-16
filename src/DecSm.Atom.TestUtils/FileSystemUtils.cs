namespace DecSm.Atom.TestUtils;

public static class FileSystemUtils
{
    public static MockFileSystem DefaultMockFileSystem =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? new(new Dictionary<string, MockFileData>
                {
                    { @"C:\Atom\Atom.sln", new(string.Empty) },
                    { @"C:\Atom\AtomTest\AtomTest.csproj", new(string.Empty) },
                },
                @"C:\Atom\AtomTest")
            : new(new Dictionary<string, MockFileData>
                {
                    { "/Atom/Atom.sln", new(string.Empty) },
                    { "/Atom/AtomTest/AtomTest.csproj", new(string.Empty) },
                },
                "/Atom/AtomTest");
}
