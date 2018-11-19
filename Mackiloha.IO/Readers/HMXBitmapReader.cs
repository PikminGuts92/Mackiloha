using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.System;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private void ReadFromStream(AwesomeReader ar, HMXBitmap bitmap)
        {
            if (ar.ReadByte() != 0x01)
                throw new Exception($"HMXBitmapReader: Expected 0x01 at offset 0");

            bitmap.Bpp = ar.ReadByte();
            bitmap.Encoding = ar.ReadInt32();
            bitmap.MipMaps = ar.ReadByte();
            
            bitmap.Width = ar.ReadUInt16();
            bitmap.Height = ar.ReadUInt16();
            bitmap.BPL = ar.ReadUInt16();

            ar.BaseStream.Position += 19; // Skips zeros
            bitmap.RawData = ar.ReadBytes(CalculateTextureByteSize(bitmap.Width, bitmap.Height, bitmap.Bpp, bitmap.MipMaps));
        }

        private int CalculateTextureByteSize(int w, int h, int bpp, int mips)
        {
            int bytes = 0;

            while (mips >= 0)
            {
                bytes += (w * h * bpp) / 8;
                w >>= 1;
                h >>= 1;
                mips -= 1;
            }

            return bytes;
        }
    }
}
