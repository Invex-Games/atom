namespace DecSm.Atom.Module.Dotnet.Util;

/// <summary>
///     Provides utility methods for working with MSBuild project files, specifically for managing version information.
/// </summary>
/// <remarks>
///     This utility class offers functionality to manipulate MSBuild project files by updating version-related properties.
///     It ensures consistent version management across different version properties in MSBuild projects.
/// </remarks>
[PublicAPI]
public static class MsBuildUtil
{
    /// <summary>
    ///     Updates an MSBuild project file XML with version information from a semantic version.
    /// </summary>
    /// <param name="file">The MSBuild project file content as an XML string.</param>
    /// <param name="version">The semantic version to apply to the project file.</param>
    /// <returns>The updated MSBuild project file content as an XML string with version properties set.</returns>
    /// <remarks>
    ///     This method performs the following operations:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Removes all existing version-related properties from PropertyGroup elements</description>
    ///         </item>
    ///         <item>
    ///             <description>Creates a new PropertyGroup with standardized version properties</description>
    ///         </item>
    ///         <item>
    ///             <description>Sets Version to the semantic version prefix</description>
    ///         </item>
    ///         <item>
    ///             <description>Sets PackageVersion to the full semantic version</description>
    ///         </item>
    ///         <item>
    ///             <description>Sets AssemblyVersion to the prefix with ".0" appended</description>
    ///         </item>
    ///         <item>
    ///             <description>Sets FileVersion to the prefix with build number from pre-release</description>
    ///         </item>
    ///         <item>
    ///             <description>Sets InformationalVersion to the full semantic version</description>
    ///         </item>
    ///     </list>
    ///     The method removes duplicate version properties to ensure clean project files and consistent versioning.
    /// </remarks>
    /// <exception cref="System.Xml.XmlException">Thrown when the input file content is not valid XML.</exception>
    /// <example>
    ///     <code>
    /// var projectXml = File.ReadAllText("MyProject.csproj");
    /// var version = new SemVer("1.2.3-beta.4");
    /// var updatedXml = MsBuildUtil.SetVersionInfo(projectXml, version);
    /// File.WriteAllText("MyProject.csproj", updatedXml);
    /// </code>
    /// </example>
    public static string SetVersionInfo(string file, SemVer version)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(file);

        // First, remove all existing version properties
        // This is to ensure that we don't have duplicate properties
        var propertyGroups = xmlDocument.SelectNodes("//PropertyGroup");

        if (propertyGroups is not null)
            foreach (XmlNode propertyGroup in propertyGroups)
            foreach (XmlNode property in propertyGroup.ChildNodes)
                if (property is XmlElement
                    {
                        Name: "Version"
                        or "VersionPrefix"
                        or "VersionSuffix"
                        or "PackageVersion"
                        or "AssemblyVersion"
                        or "FileVersion"
                        or "InformationalVersion",
                    })
                    propertyGroup.RemoveChild(property);

        // Create a new property group and populate it with the version properties
        var propertyGroupNode = xmlDocument.CreateElement("PropertyGroup", xmlDocument.DocumentElement?.NamespaceURI);

        AddProperty("Version", version.Prefix, xmlDocument, propertyGroupNode);
        AddProperty("PackageVersion", version, xmlDocument, propertyGroupNode);
        AddProperty("AssemblyVersion", $"{version.Prefix}.0", xmlDocument, propertyGroupNode);

        AddProperty("FileVersion",
            $"{version.Prefix}.{version.BuildNumberFromPreRelease}",
            xmlDocument,
            propertyGroupNode);

        AddProperty("InformationalVersion", version, xmlDocument, propertyGroupNode);

        xmlDocument.DocumentElement?.AppendChild(propertyGroupNode);

        return xmlDocument.OuterXml;
    }

    private static void AddProperty(string name, string value, XmlDocument xmlDocument, XmlElement propertyGroupNode)
    {
        var propertyNode = xmlDocument.CreateElement(name, xmlDocument.DocumentElement?.NamespaceURI);
        propertyNode.InnerText = value;
        propertyGroupNode.AppendChild(propertyNode);
    }
}
