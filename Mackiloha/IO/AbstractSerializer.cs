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

        public abstract void ReadFromStream(AwesomeReader ar, ISerializable data);

        public abstract void WriteToStream(AwesomeWriter aw, ISerializable data);

        public abstract bool IsOfType(ISerializable data);
    }
}
