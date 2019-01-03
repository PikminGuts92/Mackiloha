using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class ViewSerializer : AbstractSerializer
    {
        public ViewSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }

        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var view = data as View;
            int version = ReadMagic(ar, data);
            
            MiloSerializer.ReadFromStream(ar.BaseStream, view.Anim);
            MiloSerializer.ReadFromStream(ar.BaseStream, view.Trans);
            MiloSerializer.ReadFromStream(ar.BaseStream, view.Draw);

            view.MainView = ar.ReadString();

            var always0 = ar.ReadInt32();
            if (always0 != 0)
                throw new Exception("This should be 0");

            always0 = ar.ReadInt32();
            if (always0 != 0)
                throw new Exception("This should be 0");
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var view = data as View;

            // TODO: Add version check
            var version = Magic();
            aw.Write(version);

            MiloSerializer.WriteToStream(aw.BaseStream, view.Anim);
            MiloSerializer.WriteToStream(aw.BaseStream, view.Trans);
            MiloSerializer.WriteToStream(aw.BaseStream, view.Draw);

            aw.Write((string)view.MainView);
            aw.Write((long)0); // Unknown
        }

        public override bool IsOfType(ISerializable data) => data is View;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 7:
                    // GH1
                    return 0;
                default:
                    return -1;
            }
        }
    }
}
