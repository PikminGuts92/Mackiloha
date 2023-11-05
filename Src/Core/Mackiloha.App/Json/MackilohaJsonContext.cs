using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mackiloha.App.Metadata;

namespace Mackiloha.App.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(DirectoryMeta))]
    [JsonSerializable(typeof(TexMeta))]
    public partial class MackilohaJsonContext : JsonSerializerContext { }
}
