using System.Text.Json.Serialization;

namespace P9SongTool.Json;

[JsonSerializable(typeof(float[]))]
public partial class PrimitiveJsonContext : JsonSerializerContext { }
