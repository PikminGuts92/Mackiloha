﻿using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO.Serializers
{
    public class MatSerializer : AbstractSerializer
    {
        public MatSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }
        
        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var mat = data as Mat;
            int version = ReadMagic(ar, data);

            var textureCount = ar.ReadInt32();
            mat.TextureEntries.Clear();
            mat.TextureEntries.AddRange(RepeatFor(textureCount, () =>
            {
                var texEntry = new TextureEntry()
                {
                    Unknown1 = ar.ReadInt32(),
                    Unknown2 = ar.ReadInt32(),
                    Mat = ReadMatrix(ar),
                    Unknown3 = ar.ReadInt32(),
                    Texture = ar.ReadString()
                };

                return texEntry;
            }));

            var num = ar.ReadInt32();
            switch (num)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    break;
                default:
                    throw new Exception($"Unexpected number, got {num}");
            }
            
            mat.BaseColor = new Color4()
            {
                R = ar.ReadSingle(),
                G = ar.ReadSingle(),
                B = ar.ReadSingle(),
                A = ar.ReadSingle()
            };

            num = ar.ReadByte();
            num = ar.ReadInt16();

            num = ar.ReadInt32();
            num = ar.ReadInt16();

            mat.Blend = (BlendFactor)ar.ReadInt32();

            if (!Enum.IsDefined(typeof(BlendFactor), mat.Blend))
                throw new Exception($"Unknown blend factor of {mat.Blend}");

            num = ar.ReadInt16();
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
            throw new NotImplementedException();
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

        public override bool IsOfType(ISerializable data) => data is Mat;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 10:
                    // GH1
                    return 21;
                default:
                    return -1;
            }
        }
    }
}