using P9SongTool.Models;
using System.Text.Json.Serialization;

namespace P9SongTool.Json;

[JsonSourceGenerationOptions(WriteIndented = true, Converters = new[] { typeof(SingleLineFloatArrayConverter) })]
[JsonSerializable(typeof(P9Song))]
public partial class P9SongToolJsonContext : JsonSerializerContext { }
