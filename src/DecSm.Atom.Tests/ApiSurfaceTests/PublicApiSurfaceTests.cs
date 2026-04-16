namespace DecSm.Atom.Tests.ApiSurfaceTests;

[TestFixture]
public class PublicApiSurfaceTests
{
    [Test]
    public async Task VerifyPublicApiSurface()
    {
        // Get all types in DecSm.Atom assembly that are annotated with [PublicAPI] attribute
        var publicApiSurface = typeof(BuildDefinition)
            .Assembly
            .GetTypes()
            .Where(static t => t is { IsPublic: true })
            .Select(static t => new Type(t.FullName!,
                t
                    .GetMembers(BindingFlags.Instance |
                                BindingFlags.Static |
                                BindingFlags.Public |
                                BindingFlags.DeclaredOnly)
                    .Select(static m => GetMember(m))
                    .Where(static m => m is not null)
                    .Select(x => x!)
                    .OrderBy(static m => m.Name)
                    .ToList()))
            .OrderBy(static t => t.Name)
            .ToList();

        var jsonRepresentation = JsonSerializer.Serialize(publicApiSurface);

        await VerifyJson(jsonRepresentation);
    }

    private static IMember? GetMember(MemberInfo arg) =>
        arg switch
        {
            FieldInfo fieldInfo => new Field(fieldInfo.Name, fieldInfo.FieldType.FullName!),
            PropertyInfo propertyInfo => new Property(propertyInfo.Name, propertyInfo.PropertyType.FullName!),
            MethodInfo { IsSpecialName: false } methodInfo => new Method(methodInfo.Name,
                methodInfo.ReturnType.FullName!,
                methodInfo
                    .GetParameters()
                    .Select(p => new Parameter(p.Name!, p.ParameterType.FullName!))
                    .ToList()),
            _ => null,
        };
}

public sealed record Type(string Name, [UsedImplicitly] IReadOnlyList<IMember> Members);

public interface IMember
{
    string Name { get; }
}

public sealed record Field(string Name, string Type) : IMember
{
    public override string ToString() =>
        $"{Type} {Name}";
}

public sealed record Property(string Name, string Type) : IMember
{
    public override string ToString() =>
        $"{Type} {Name}";
}

public sealed record Method(string Name, string ReturnType, IReadOnlyList<Parameter> Parameters) : IMember
{
    public override string ToString() =>
        $"{ReturnType} {Name}({string.Join(", ", Parameters)})";
}

public sealed record Parameter(string Name, string Type)
{
    public override string ToString() =>
        $"{Type} {Name}";
}
