using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    [JsonConverter(typeof(ComponentTypeConverter))]
    public enum ComponentType : int
    {
        Byte = 5120,
        UnsignedByte,
        Short,
        UnsignedShort,
        UnsignedInt = 5125,
        Float
    }

    internal class ComponentTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new JsonReaderException($"\'{reader.Path}\': Value must be a number!");

            var value = Convert.ToInt32(reader.Value);
            return Parse(value, reader.Path);
        }

        public static ComponentType Parse(int value, string readerPath = "")
        {
            if (!Enum.IsDefined(typeof(ComponentType), value))
                throw new JsonReaderException($"\'{readerPath}\': Value of \'{value}\' is not supported!");

            return (ComponentType)(value);
        }

        public static bool TryParse(int value, out ComponentType type)
        {
            type = (ComponentType)(value);
            return Enum.IsDefined(typeof(ComponentType), value);
        }

        public static Type GetType(ComponentType type)
        {
            switch(type)
            {
                default:
                    return typeof(object);
                case ComponentType.Byte:
                    return typeof(sbyte);
                case ComponentType.UnsignedByte:
                    return typeof(byte);
                case ComponentType.Short:
                    return typeof(short);
                case ComponentType.UnsignedShort:
                    return typeof(ushort);
                case ComponentType.UnsignedInt:
                    return typeof(uint);
                case ComponentType.Float:
                    return typeof(float);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
