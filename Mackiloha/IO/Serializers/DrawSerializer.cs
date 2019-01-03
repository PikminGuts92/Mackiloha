using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class DrawSerializer : AbstractSerializer
    {
        public DrawSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }

        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var draw = data as Draw;
            int version = ReadMagic(ar, data);

            draw.Showing = ar.ReadBoolean();

            var drawableCount = ar.ReadInt32();
            draw.Drawables.Clear();
            draw.Drawables.AddRange(RepeatFor(drawableCount, () => (MiloString)ar.ReadString()));

            draw.Boundry = new Sphere()
            {
                X = ar.ReadSingle(),
                Y = ar.ReadSingle(),
                Z = ar.ReadSingle(),
                Radius = ar.ReadSingle()
            };
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var draw = data as Draw;

            // TODO: Add version check
            var version = Magic();
            aw.Write(version);

            aw.Write((bool)draw.Showing);

            aw.Write((int)draw.Drawables.Count);
            draw.Drawables.ForEach(x => aw.Write((string)x));
        }

        public override bool IsOfType(ISerializable data) => data is Draw;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 10:
                    // GH1
                    return 1;
                default:
                    return -1;
            }
        }
    }
}
