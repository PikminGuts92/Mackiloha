using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class TexSerializer : AbstractSerializer
    {
        public TexSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }
        
        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var tex = data as Tex;

            // TODO: Add version check
            if (ar.ReadInt32() != 0x08)
                throw new NotSupportedException($"TexReader: Expected 0x08 at offset 0");

            tex.Width = ar.ReadInt32();
            tex.Height = ar.ReadInt32();
            tex.Bpp = ar.ReadInt32();

            tex.ExternalPath = ar.ReadString();

            if (ar.ReadSingle() != -8.0f)
                throw new NotSupportedException("TexReader: Expected -8.0");

            if (ar.ReadInt32() != 0x01)
                throw new NotSupportedException($"TexReader: Expected 0x01");

            tex.UseExternal = ar.ReadBoolean();

            tex.Bitmap = MiloSerializer.ReadFromStream<HMXBitmap>(ar.BaseStream);
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var tex = data as Tex;

            // TODO: Add version check
            aw.Write((int)0x08);

            aw.Write((int)tex.Width);
            aw.Write((int)tex.Height);
            aw.Write((int)tex.Bpp);

            aw.Write(tex.ExternalPath);
            aw.Write((float)-8.0);
            aw.Write((int)0x01);

            if (tex.UseExternal && tex.Bitmap != null)
            {
                aw.Write(true);
                MiloSerializer.WriteToStream(aw.BaseStream, tex.Bitmap);
            }
            else
            {
                aw.Write(false);
            }
        }

        public override bool IsOfType(ISerializable data) => data is Tex;
    }
}
