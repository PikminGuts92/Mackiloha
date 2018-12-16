using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mackiloha.IO.Serializers;
using Mackiloha.Render;

namespace Mackiloha.IO
{
    public class MiloSerializer
    {
        public readonly SystemInfo Info;
        private readonly AbstractSerializer[] Serializers;
        
        public MiloSerializer(SystemInfo info)
        {
            Info = info;

            Serializers = new AbstractSerializer[]
            {
                new HMXBitmapSerializer(this),
                new MiloObjectBytesSerializer(this),
                new MiloObjectDirSerializer(this),
                new TexSerializer(this)
            };
        }

        public MiloSerializer(SystemInfo info, AbstractSerializer[] serializers)
        {
            Info = info;

            // Just to be extra safe
            if (serializers == null)
                Serializers = new AbstractSerializer[0];
            else
                Serializers = serializers.Where(x => x != default(AbstractSerializer)).ToArray();
        }

        public T ReadFromFile<T>(string path) where T : ISerializable, new()
        {
            using (var fs = File.OpenRead(path))
            {
                return ReadFromStream<T>(fs);
            }
        }

        public T ReadFromStream<T>(Stream stream) where T : ISerializable, new()
        {
            var data = new T();
            var ar = new AwesomeReader(stream, Info.BigEndian);

            var serializer = Serializers.FirstOrDefault(x => x.IsOfType(data));

            if (serializer == null)
                throw new NotImplementedException($"Deserialization of {typeof(T).Name} is not supported yet!");

            serializer.ReadFromStream(ar, data);

            /*
            switch (data)
            {
                case MiloObjectDir dir:
                    ReadFromStream(ar, dir);
                    break;
                case Tex tex:
                    ReadFromStream(ar, tex);
                    break;
                case HMXBitmap bitmap:
                    this.ReadFromStream(ar, bitmap);
                    break;
                default:
                    throw new NotImplementedException($"Deserialization of {typeof(T).Name} is not supported yet!");
            }*/

            return data;
        }

        internal MiloObject ReadFromStream(Stream stream, string type)
        {
            MiloObject obj;

            switch (type)
            {
                case "Tex":
                    obj = ReadFromStream<Tex>(stream);
                    break;
                default:
                    throw new NotImplementedException($"Deserialization of {type} is not supported yet!");
            }

            return obj;
        }
        
        public void WriteToFile(string path, ISerializable obj)
        {
            using (var fs = File.OpenWrite(path))
            {
                WriteToStream(fs, obj);
            }
        }

        public void WriteToStream(Stream stream, ISerializable obj)
        {
            var aw = new AwesomeWriter(stream, Info.BigEndian);

            var serializer = Serializers.FirstOrDefault(x => x.IsOfType(obj));

            if (serializer == null)
                throw new NotImplementedException($"Serialization of {obj.GetType().Name} is not supported yet!");

            serializer.WriteToStream(aw, obj);

            /*
            switch (obj)
            {
                case Tex tex:
                    WriteToStream(aw, tex);
                    break;
                case HMXBitmap bitmap:
                    WriteToStream(aw, bitmap);
                    break;
                case MiloObjectBytes bytes:
                    WriteToStream(aw, bytes);
                    break;
                default:
                    throw new NotImplementedException($"Serialization of {obj.GetType().Name} is not supported yet!");
            } */
        }
    }
}
