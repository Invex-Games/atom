namespace DecSm.Atom.Module.GitVersion;

/// <summary>
///     Provides a <see cref="JsonSerializerContext" /> for source generation of <see cref="JsonElement" />.
/// </summary>
/// <remarks>
///     This internal class is used by <see cref="System.Text.Json" /> for efficient serialization
///     and deserialization of <see cref="JsonElement" /> instances within the GitVersion module.
/// </remarks>
[JsonSerializable(typeof(JsonElement))]
internal sealed partial class JsonElementContext : JsonSerializerContext;
