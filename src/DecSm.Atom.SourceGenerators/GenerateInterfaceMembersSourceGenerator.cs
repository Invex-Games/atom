namespace DecSm.Atom.SourceGenerators;

[Generator]
public class GenerateInterfaceMembersSourceGenerator : IIncrementalGenerator
{
    private static readonly HashSet<string> ExcludedPropertyNames =
    [
        "GlobalWorkflowOptions",
        "Workflows",
        "ParamDefinitions",
        "TargetDefinitions",
        "Logger",
        "FileSystem",
        "ProcessRunner",
        "Services",
    ];

    private static readonly HashSet<string> ExcludedMethodNames =
    [
        ".ctor", "ConfigureBuilder", "GetService", "GetServices", "GetParam",
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classSymbols = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(GenerateInterfaceMembersAttribute,
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, _) => (INamedTypeSymbol)context.TargetSymbol)
            .WithTrackingName(nameof(GenerateInterfaceMembersSourceGenerator));

        context.RegisterSourceOutput(classSymbols.Select(static (symbol, ct) => GeneratePartial(symbol, ct)),
            static (context, data) =>
            {
                if (data.SourceCode is not null)
                    context.AddSource($"{data.ClassName}.g.cs", SourceText.From(data.SourceCode, Encoding.UTF8));
            });
    }

    private static ClassNameWithSourceCode GeneratePartial(
        INamedTypeSymbol classSymbol,
        CancellationToken cancellationToken)
    {
        var (properties, methods) = GetInterestingMembers(classSymbol, cancellationToken);

        if (properties.IsEmpty && methods.IsEmpty)
            return new(classSymbol.Name, null);

        var memberLines = GenerateMemberLines(properties, methods);
        var indentedMembers = string.Join("\n\n", memberLines.Select(line => $"    {line}"));

        var sourceCode = BuildSourceCode(classSymbol, indentedMembers);

        return new(classSymbol.Name, sourceCode);
    }

    private static PropertiesWithMethods GetInterestingMembers(
        INamedTypeSymbol classSymbol,
        CancellationToken cancellationToken)
    {
        var allInterfaceMembers = classSymbol
            .AllInterfaces
            .Where(static x => x.ToDisplayString() != IBuildDefinition)
            .SelectMany(static interfaceSymbol => interfaceSymbol.GetMembers(),
                (interfaceSymbol, member) => (interfaceSymbol, member));

        var propertiesBuilder = ImmutableArray.CreateBuilder<TypeWithProperty>();
        var methodsBuilder = ImmutableArray.CreateBuilder<TypeWithMethod>();

        foreach (var (interfaceSymbol, member) in allInterfaceMembers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (member.DeclaredAccessibility is not (Accessibility.Public
                or Accessibility.Protected
                or Accessibility.ProtectedOrInternal))
                continue;

            switch (member)
            {
                case IPropertySymbol propertySymbol:
                {
                    if (!propertySymbol.IsStatic && !ExcludedPropertyNames.Contains(propertySymbol.Name))
                        propertiesBuilder.Add(new(interfaceSymbol, propertySymbol));

                    break;
                }

                case IMethodSymbol methodSymbol:
                {
                    if (!methodSymbol.IsStatic &&
                        !methodSymbol.Name.StartsWith("get_") &&
                        !ExcludedMethodNames.Contains(methodSymbol.Name))
                        methodsBuilder.Add(new(interfaceSymbol, methodSymbol));

                    break;
                }
            }
        }

        return new(propertiesBuilder.ToImmutable(), methodsBuilder.ToImmutable());
    }

    private static ImmutableArray<string> GenerateMemberLines(
        ImmutableArray<TypeWithProperty> properties,
        ImmutableArray<TypeWithMethod> methods)
    {
        var propertyLines = properties.Select(GeneratePropertyLine);
        var methodLines = methods.Select(GenerateMethodLine);

        return [..propertyLines.Concat(methodLines)];
    }

    private static string GeneratePropertyLine(TypeWithProperty typeWithProperty)
    {
        var interfaceName = typeWithProperty.Type.ToDisplayString();
        var propertyName = typeWithProperty.Property.Name;
        var propertyType = typeWithProperty.Property.Type.ToDisplayString();

        return $"private {propertyType} {propertyName} => (({interfaceName})this).{propertyName};";
    }

    private static string GenerateMethodLine(TypeWithMethod typeWithMethod)
    {
        var interfaceName = typeWithMethod.Type.ToDisplayString();
        var methodName = typeWithMethod.Method.Name;
        var methodReturnType = typeWithMethod.Method.ReturnType.ToDisplayString();

        var methodInputParams = string.Join(", ",
            typeWithMethod.Method.Parameters.Select(static param =>
            {
                var attributes = param
                    .GetAttributes()
                    .Where(x => x
                        .AttributeClass
                        ?.ToDisplayString()
                        .StartsWith("System.Runtime.CompilerServices", StringComparison.OrdinalIgnoreCase) is false)
                    .ToArray();

                var type = param.Type.ToDisplayString();
                var name = param.Name;
                string? defaultValue = null;

                if (param is { HasExplicitDefaultValue: true, DeclaringSyntaxReferences.Length: > 0 })
                {
                    var syntaxNode = param
                        .DeclaringSyntaxReferences[0]
                        .SyntaxTree
                        .GetRoot()
                        .FindNode(param.DeclaringSyntaxReferences[0].Span);

                    if (syntaxNode is ParameterSyntax { Default: { } equalsValueClauseSyntax })
                        defaultValue = $" = {equalsValueClauseSyntax.Value.ToFullString().Trim()}";
                    else
                        defaultValue = param.ExplicitDefaultValue is null
                            ? " = null"
                            : $" = {param.ExplicitDefaultValue}";
                }

                var attributeString = attributes.Length > 0
                    ? $"[{string.Join(", ", attributes.Select(a => a.AttributeClass!.ToDisplayString()))}]"
                    : string.Empty;

                var refKindString = param.RefKind switch
                {
                    RefKind.None => string.Empty,
                    RefKind.Ref => "ref ",
                    RefKind.Out => "out ",
                    RefKind.In => "in ",
                    _ => string.Empty,
                };

                return $"{attributeString} {refKindString}{type} {name}{defaultValue}".Trim();
            }));

        var methodCallParams = string.Join(", ",
            typeWithMethod.Method.Parameters.Select(static param => param.RefKind switch
            {
                RefKind.None => param.Name,
                RefKind.Ref => $"ref {param.Name}",
                RefKind.Out => $"out {param.Name}",
                RefKind.In => $"in {param.Name}",
                _ => param.Name,
            }));

        var genericParameters = typeWithMethod.Method.IsGenericMethod
            ? $"<{string.Join(", ", typeWithMethod.Method.TypeParameters.Select(static param => param.Name))}>"
            : string.Empty;

        return $"""
                private {methodReturnType} {methodName}{genericParameters}({methodInputParams}) =>
                        (({interfaceName})this).{methodName}{genericParameters}({methodCallParams});
                """;
    }

    private static string BuildSourceCode(INamedTypeSymbol classSymbol, string classMembers)
    {
        var @namespace = classSymbol.ContainingNamespace.ToDisplayString();

        var namespaceLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"namespace {@namespace};";

        var @class = classSymbol.Name;
        var classFull = classSymbol.ToDisplayString();

        var globalUsingStaticLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"global using static {classFull};";

        return $$"""
                 // <auto-generated/>

                 // ReSharper disable MemberHidesInterfaceMemberWithDefaultImplementation

                 #nullable enable

                 {{globalUsingStaticLine}}

                 {{namespaceLine}}

                 partial class {{@class}}
                 {
                 {{classMembers}}
                 }
                 """;
    }
}
