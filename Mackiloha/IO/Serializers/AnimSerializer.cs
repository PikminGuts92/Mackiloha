using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class AnimSerializer : AbstractSerializer
    {
        public AnimSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }

        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var anim = data as Anim;
            int version = ReadMagic(ar, data);

            int count = ar.ReadInt32();
            anim.Entries.Clear();
            anim.Entries.AddRange(
                RepeatFor(count, () => new AnimEntry()
                {
                    Name = ar.ReadString(),
                    F1 = ar.ReadSingle(),
                    F2 = ar.ReadSingle()
                }));

            count = ar.ReadInt32();
            anim.Animatables.Clear();
            anim.Animatables.AddRange(
                RepeatFor(count, () => (MiloString)ar.ReadString()));
        }
        
        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var anim = data as Anim;

            // TODO: Add version check
            var version = Magic();
            aw.Write(version);

            aw.Write((int)anim.Entries.Count);
            anim.Entries.ForEach(x =>
            {
                aw.Write((string)x.Name);
                aw.Write((float)x.F1);
                aw.Write((float)x.F2);
            });

            aw.Write((int)anim.Animatables.Count);
            anim.Animatables.ForEach(x => aw.Write((string)x));
        }

        protected static void WriteAnimEntry(AnimEntry ae, AwesomeWriter aw)
        {
            aw.Write((string)ae.Name);
            aw.Write((float)ae.F1);
            aw.Write((float)ae.F2);
        }

        public override bool IsOfType(ISerializable data) => data is Anim;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 10:
                    // GH1
                    return 0;
                default:
                    return -1;
            }
        }
    }
}
