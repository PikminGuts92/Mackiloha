using Mackiloha.App.Metadata;
using System.Text.Json.Serialization;

namespace Mackiloha.App.Json;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DirectoryMeta))]
[JsonSerializable(typeof(TexMeta))]
public partial class MackilohaJsonContext : JsonSerializerContext { }
