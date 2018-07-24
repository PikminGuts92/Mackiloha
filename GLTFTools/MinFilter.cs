using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(MinFilterConverter))]
    public enum MinFilter : int
    {
        Nearest = 9728,
        Linear,
        NearestMipMapNearest = 9984,
        LinearMipMapNearest,
        NearestMipMapLinear,
        LinearMipMapLinear
    }

    internal class MinFilterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
