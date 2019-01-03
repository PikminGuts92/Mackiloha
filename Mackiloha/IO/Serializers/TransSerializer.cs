using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class TransSerializer : AbstractSerializer
    {
        public TransSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }
        
        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var trans = data as Trans;
            int version = ReadMagic(ar, data);

            trans.Mat1 = ReadMatrix(ar);
            trans.Mat2 = ReadMatrix(ar);
            
            var transformableCount = ar.ReadInt32();
            trans.Transformables.Clear();
            trans.Transformables.AddRange(RepeatFor(transformableCount, () => (MiloString)ar.ReadString()));

            var always0 = ar.ReadInt32();
            if (always0 != 0)
                throw new Exception("This should be 0");

            trans.Camera = ar.ReadString();

            always0 = ar.ReadByte();
            if (always0 != 0)
                throw new Exception("This should be 0");

            trans.Transform = ar.ReadString();
        }

        protected static Matrix4 ReadMatrix(AwesomeReader ar)
        {
            return new Matrix4()
            {
                M11 = ar.ReadSingle(), // M11
                M12 = ar.ReadSingle(), // M12
                M13 = ar.ReadSingle(), // M13
                
                M21 = ar.ReadSingle(), // M21
                M22 = ar.ReadSingle(), // M22
                M23 = ar.ReadSingle(), // M23
                
                M31 = ar.ReadSingle(), // M31
                M32 = ar.ReadSingle(), // M32
                M33 = ar.ReadSingle(), // M33
                
                M41 = ar.ReadSingle(), // M41
                M42 = ar.ReadSingle(), // M42
                M43 = ar.ReadSingle(), // M43
                M44 = 1.0f             // M44 - Implicit
            };
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            var trans = data as Trans;

            // TODO: Add version check
            var version = Magic();
            aw.Write(version);
            
            WriteMatrix(trans.Mat1, aw);
            WriteMatrix(trans.Mat2, aw);

            aw.Write((int)trans.Transformables.Count);
            trans.Transformables.ForEach(x => aw.Write((string)x));

            aw.Write((int)0);
            aw.Write((string)trans.Camera);
            aw.Write((byte)0);

            aw.Write((string)trans.Transform);
        }

        protected static void WriteMatrix(Matrix4 mat, AwesomeWriter aw)
        {
            aw.Write((float)mat.M11);
            aw.Write((float)mat.M12);
            aw.Write((float)mat.M13);

            aw.Write((float)mat.M21);
            aw.Write((float)mat.M22);
            aw.Write((float)mat.M23);

            aw.Write((float)mat.M31);
            aw.Write((float)mat.M32);
            aw.Write((float)mat.M33);

            aw.Write((float)mat.M41);
            aw.Write((float)mat.M42);
            aw.Write((float)mat.M43);
        }

        public override bool IsOfType(ISerializable data) => data is Trans;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 10:
                    // GH1
                    return 8;
                default:
                    return -1;
            }
        }
    }
}
