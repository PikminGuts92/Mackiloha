using System;
using System.IO;
using Mackiloha.Render;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private readonly SystemInfo _info;

        public MiloSerializer(SystemInfo info)
        {
            _info = info;
        }

        public void ReadFromFile<T>(string path) where T : ISerializable, new()
        {
            using (var fs = File.OpenRead(path))
            {
                ReadFromStream<T>(fs);
            }
        }

        public T ReadFromStream<T>(Stream stream) where T : ISerializable, new()
        {
            var data = new T();
            var ar = new AwesomeReader(stream, _info.BigEndian);

            switch (data)
            {
                case MiloObjectDir dir:
                    ReadFromStream(ar, dir);
                    break;
                case Tex tex:
                    ReadFromStream(ar, tex);
                    break;
                case HMXBitmap bitmap:
                    ReadFromStream(ar, bitmap);
                    break;
                default:
                    throw new NotImplementedException($"Deserialization of {typeof(T).Name} is not supported yet!");
            }

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
            var aw = new AwesomeWriter(stream, _info.BigEndian);

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
            } 
        }
    }
}
