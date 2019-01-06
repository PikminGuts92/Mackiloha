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

            int version = ReadMagic(ar, data);

            // Skips zeros
            if (version >= 10 && MiloSerializer.Info.Version == 24)
                ar.BaseStream.Position += 9; // GH2 PS2
            else if (version >= 10)
                ar.BaseStream.Position += 13; // GH2 360

            tex.Width = ar.ReadInt32();
            tex.Height = ar.ReadInt32();
            tex.Bpp = ar.ReadInt32();

            tex.ExternalPath = ar.ReadString();

            tex.IndexF = ar.ReadSingle();

            switch (tex.IndexF)
            {
                case -10.0f:
                case  -9.0f:
                case  -8.0f:
                case  -7.0f:
                case  -6.0f:
                case  -0.5f:
                case   0.0f:
                    break;
                default:
                    throw new NotSupportedException($"Expected number between -10.0 <-> 0.0, got {tex.IndexF}");
            }

            tex.Index = ar.ReadInt32();
            switch(tex.Index)
            {
                case 1:
                case 4:
                case 34:
                    break;
                default:
                    throw new NotSupportedException($"Unexpected number, got {tex.Index}");
            }
            
            tex.UseExternal = ar.ReadBoolean();
            tex.Bitmap = null;

            if (tex.UseExternal)
                return;

            if (!tex.UseExternal && ar.BaseStream.Position == ar.BaseStream.Length)
            {
                // What the heck? Even HMX had some bad serializations
                tex.UseExternal = true;
                return;
            }

            tex.Bitmap = MiloSerializer.ReadFromStream<HMXBitmap>(ar.BaseStream);
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var tex = data as Tex;

            // TODO: Add version check
            var version = Magic();
            aw.Write((int)version);

            if (version >= 10)
                aw.Write(new byte[9]);
            
            aw.Write((int)tex.Width);
            aw.Write((int)tex.Height);
            aw.Write((int)tex.Bpp);

            aw.Write(tex.ExternalPath);
            aw.Write((float)tex.IndexF);
            aw.Write((int)tex.Index);

            if (!tex.UseExternal && tex.Bitmap != null)
            {
                aw.Write(false);
                MiloSerializer.WriteToStream(aw.BaseStream, tex.Bitmap);
            }
            else
            {
                aw.Write(true);
            }
        }

        public override bool IsOfType(ISerializable data) => data is Tex;

        public override int Magic()
        {
            switch(MiloSerializer.Info.Version)
            {
                case 10:
                    // GH1
                    return 8;
                case 24:
                    // GH2 PS2
                    return 10; // TODO: Take into account other factors for demos
                case 25:
                    // GH2 360 / RB1
                    return 10;
                default:
                    return -1;
            }
        }
    }
}
