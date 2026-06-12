namespace Invex.Atom.Build.SourceGenerators;

internal static class Symbols
{
    public const string BuildDefinitionAttribute = "Invex.Atom.Build.Definition.BuildDefinitionAttribute";

    public const string BuildDefinitionInterfaceAttribute =
        "Invex.Atom.Build.Definition.BuildDefinitionInterfaceAttribute";

    public const string GenerateEntryPointAttribute = "Invex.Atom.Build.Hosting.GenerateEntryPointAttribute";

    public const string GenerateInterfaceMembersAttribute =
        "Invex.Atom.Build.Definition.GenerateInterfaceMembersAttribute";

    public const string GenerateSolutionModelAttribute = "Invex.Atom.Build.Definition.GenerateSolutionModelAttribute";

    public const string BuildDefinition = "Invex.Atom.Build.Definition.BuildDefinition";
    public const string IBuildDefinition = "Invex.Atom.Build.Definition.IBuildDefinition";
    public const string IBuildAccessor = "Invex.Atom.Build.IBuildAccessor";

    public const string Target = "Invex.Atom.Build.Definition.Target";

    public const string ParamDefinition = "Invex.Atom.Build.Params.ParamDefinition";
    public const string ParamDefinitionAttribute = "Invex.Atom.Build.Params.ParamDefinitionAttribute";
    public const string SecretDefinitionAttribute = "Invex.Atom.Build.Params.SecretDefinitionAttribute";

    public const string ConfigureHostAttribute = "Invex.Atom.Build.Hosting.ConfigureHostAttribute";
    public const string ConfigureHostBuilderAttribute = "Invex.Atom.Build.Hosting.ConfigureHostBuilderAttribute";

    public const string IBuildOption = "Invex.Atom.Build.BuildOptions.IBuildOption";

    public const string IReadOnlyDictionary = "System.Collections.Generic.IReadOnlyDictionary";
    public const string Dictionary = "System.Collections.Generic.Dictionary";
}

internal readonly record struct ClassNameWithSourceCode(string ClassName, string? SourceCode)
{
    public string ClassName { get; } = ClassName;

    public string? SourceCode { get; } = SourceCode;
}

internal readonly record struct InterfaceNameWithSourceCode(string InterfaceName, string? SourceCode)
{
    public string InterfaceName { get; } = InterfaceName;

    public string? SourceCode { get; } = SourceCode;
}

internal readonly record struct CodeRegion(string? RegionName, string[] RegionBlocks)
{
    public string? RegionName { get; } = RegionName;

    public string[] RegionBlocks { get; } = RegionBlocks;
}

internal readonly record struct TargetData(IPropertySymbol Property, INamedTypeSymbol Type)
{
    public IPropertySymbol Property { get; } = Property;

    public INamedTypeSymbol Type { get; } = Type;
}

internal readonly record struct ParamData(IPropertySymbol Property, AttributeData Attribute, INamedTypeSymbol Type)
{
    public IPropertySymbol Property { get; } = Property;

    public AttributeData Attribute { get; } = Attribute;

    public INamedTypeSymbol Type { get; } = Type;
}

internal readonly record struct TargetsAndParams(ImmutableArray<TargetData> Targets, ImmutableArray<ParamData> Params)
{
    public ImmutableArray<TargetData> Targets { get; } = Targets;

    public ImmutableArray<ParamData> Params { get; } = Params;
}

internal readonly record struct TextResult(string Text, bool Any)
{
    public string Text { get; } = Text;

    public bool Any { get; } = Any;
}

internal readonly record struct TypeWithProperty(INamedTypeSymbol Type, IPropertySymbol Property)
{
    public INamedTypeSymbol Type { get; } = Type;

    public IPropertySymbol Property { get; } = Property;
}

internal readonly record struct TypeWithMethod(INamedTypeSymbol Type, IMethodSymbol Method)
{
    public INamedTypeSymbol Type { get; } = Type;

    public IMethodSymbol Method { get; } = Method;
}

internal readonly record struct PropertiesWithMethods(
    ImmutableArray<TypeWithProperty> Properties,
    ImmutableArray<TypeWithMethod> Methods
)
{
    public ImmutableArray<TypeWithProperty> Properties { get; } = Properties;

    public ImmutableArray<TypeWithMethod> Methods { get; } = Methods;
}
