using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.IO
{
    public abstract class AbstractSerializer
    {
        protected readonly MiloSerializer MiloSerializer;

        public AbstractSerializer(MiloSerializer miloSerializer)
        {
            MiloSerializer = miloSerializer;
        }

        public int ReadMagic(AwesomeReader ar, ISerializable data)
        {
            int magic = Magic();
            if (magic == -1)
                throw new NotImplementedException($"{GetType().Name}: Deserialization of {data.GetType().Name} for serializer version 0x{MiloSerializer.Info.Version:X2} is not implemented yet");
            
            int version = ar.ReadInt32();
            if (magic != version)
                throw new NotSupportedException($"{GetType().Name}: Magic 0x{version:X2} does not correspond to serializer version 0x{MiloSerializer.Info.Version:X2} (Expected 0x{magic:X2})");

            return version;
        }

        public abstract void ReadFromStream(AwesomeReader ar, ISerializable data);

        public abstract void WriteToStream(AwesomeWriter aw, ISerializable data);

        public abstract bool IsOfType(ISerializable data);

        public abstract int Magic();
    }
}
