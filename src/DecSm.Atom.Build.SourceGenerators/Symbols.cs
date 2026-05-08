namespace DecSm.Atom.Build.SourceGenerators;

public static class Symbols
{
    public const string BuildDefinitionAttribute = "DecSm.Atom.Build.Definition.BuildDefinitionAttribute";

    public const string BuildDefinitionInterfaceAttribute =
        "DecSm.Atom.Build.Definition.BuildDefinitionInterfaceAttribute";

    public const string GenerateEntryPointAttribute = "DecSm.Atom.Build.Hosting.GenerateEntryPointAttribute";

    public const string GenerateInterfaceMembersAttribute =
        "DecSm.Atom.Build.Definition.GenerateInterfaceMembersAttribute";

    public const string GenerateSolutionModelAttribute = "DecSm.Atom.Build.Definition.GenerateSolutionModelAttribute";

    public const string BuildDefinition = "DecSm.Atom.Build.Definition.BuildDefinition";
    public const string IBuildDefinition = "DecSm.Atom.Build.Definition.IBuildDefinition";
    public const string IBuildAccessor = "DecSm.Atom.Build.IBuildAccessor";

    public const string Target = "DecSm.Atom.Build.Definition.Target";

    public const string ParamDefinition = "DecSm.Atom.Build.Params.ParamDefinition";
    public const string ParamDefinitionAttribute = "DecSm.Atom.Build.Params.ParamDefinitionAttribute";
    public const string SecretDefinitionAttribute = "DecSm.Atom.Build.Params.SecretDefinitionAttribute";

    public const string ConfigureHostAttribute = "DecSm.Atom.Build.Hosting.ConfigureHostAttribute";
    public const string ConfigureHostBuilderAttribute = "DecSm.Atom.Build.Hosting.ConfigureHostBuilderAttribute";

    public const string IBuildOption = "DecSm.Atom.Build.BuildOptions.IBuildOption";

    public const string IReadOnlyDictionary = "System.Collections.Generic.IReadOnlyDictionary";
    public const string Dictionary = "System.Collections.Generic.Dictionary";
}

public readonly record struct ClassNameWithSourceCode(string ClassName, string? SourceCode)
{
    public string ClassName { get; } = ClassName;

    public string? SourceCode { get; } = SourceCode;
}

public readonly record struct InterfaceNameWithSourceCode(string InterfaceName, string? SourceCode)
{
    public string InterfaceName { get; } = InterfaceName;

    public string? SourceCode { get; } = SourceCode;
}

public readonly record struct CodeRegion(string? RegionName, string[] RegionBlocks)
{
    public string? RegionName { get; } = RegionName;

    public string[] RegionBlocks { get; } = RegionBlocks;
}

public readonly record struct TargetData(IPropertySymbol Property, INamedTypeSymbol Type)
{
    public IPropertySymbol Property { get; } = Property;

    public INamedTypeSymbol Type { get; } = Type;
}

public readonly record struct ParamData(IPropertySymbol Property, AttributeData Attribute, INamedTypeSymbol Type)
{
    public IPropertySymbol Property { get; } = Property;

    public AttributeData Attribute { get; } = Attribute;

    public INamedTypeSymbol Type { get; } = Type;
}

public readonly record struct TargetsAndParams(ImmutableArray<TargetData> Targets, ImmutableArray<ParamData> Params)
{
    public ImmutableArray<TargetData> Targets { get; } = Targets;

    public ImmutableArray<ParamData> Params { get; } = Params;
}

public readonly record struct TextResult(string Text, bool Any)
{
    public string Text { get; } = Text;

    public bool Any { get; } = Any;
}

public readonly record struct TypeWithProperty(INamedTypeSymbol Type, IPropertySymbol Property)
{
    public INamedTypeSymbol Type { get; } = Type;

    public IPropertySymbol Property { get; } = Property;
}

public readonly record struct TypeWithMethod(INamedTypeSymbol Type, IMethodSymbol Method)
{
    public INamedTypeSymbol Type { get; } = Type;

    public IMethodSymbol Method { get; } = Method;
}

public readonly record struct PropertiesWithMethods(
    ImmutableArray<TypeWithProperty> Properties,
    ImmutableArray<TypeWithMethod> Methods
)
{
    public ImmutableArray<TypeWithProperty> Properties { get; } = Properties;

    public ImmutableArray<TypeWithMethod> Methods { get; } = Methods;
}
