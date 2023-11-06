using ArkHelper.Models;
using System.Text.Json.Serialization;

namespace ArkHelper.Json
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ArkCache))]
    public partial class ArkHelperJsonContext : JsonSerializerContext { }
}
