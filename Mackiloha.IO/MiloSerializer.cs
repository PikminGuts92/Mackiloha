using System;
using System.IO;
using Mackiloha.System;
using Mackiloha.System.Render;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private readonly SystemInfo _info;

        public MiloSerializer(SystemInfo info)
        {
            _info = info;
        }

        public void ReadFromFile(string path, ISerializable obj)
        {
            using (var fs = File.OpenRead(path))
            {
                ReadFromStream(fs, obj);
            }
        }

        public void ReadFromStream(Stream stream, ISerializable obj)
        {
            using (var ar = new AwesomeReader(stream, _info.BigEndian))
            {
                switch (obj)
                {
                    case Tex tex:
                        ReadFromStream(ar, tex);
                        break;
                    case HMXBitmap bitmap:
                        ReadFromStream(ar, bitmap);
                        break;
                    default:
                        throw new NotImplementedException($"Deserialization of {obj.GetType().Name} is not supported yet!");
                }
            }
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
            using (var aw = new AwesomeWriter(stream, _info.BigEndian))
            {
                switch (obj)
                {
                    default:
                        throw new NotImplementedException($"Serialization of {obj.GetType().Name} is not supported yet!");
                }
            }
        }
    }
}
