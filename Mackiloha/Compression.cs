using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression; // For Gzip compression
using zlib; // For Zlib compression

namespace Mackiloha
{
    public enum CompressionType
    {
        ZLIB,
        GZIP
    }

    public static class Compression
    {
        private static byte[] ZLIB_MAGIC = { 0x78, 0x9C }; // Default compression

        public static byte[] InflateBlock(byte[] inBlock, CompressionType type, int offset = 0)
        {
            if (offset < 0) offset = 0;
            byte[] outBlock;

            switch(type)
            {
                case CompressionType.GZIP:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Decompresses gzip stream
                        GZipStream gzip = new GZipStream(new MemoryStream(), CompressionMode.Decompress);
                        gzip.Write(inBlock, offset, inBlock.Length - offset);

                        gzip.CopyTo(ms);
                        outBlock = ms.ToArray();
                        gzip.Flush();
                    }
                    break;
                case CompressionType.ZLIB:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Decompresses zlib stream
                        ZOutputStream outZStream = new ZOutputStream(ms);
                        outZStream.Write(ZLIB_MAGIC, 0, ZLIB_MAGIC.Length); // Required

                        outZStream.Write(inBlock, offset, inBlock.Length - offset);
                        outBlock = ms.ToArray();
                        outZStream.Flush();
                    }
                    break;
                default:
                    outBlock = new byte[inBlock.Length];
                    Array.Copy(inBlock, outBlock, inBlock.Length);
                    break;
            }

            return outBlock;
        }

        public static byte[] DeflateBlock(byte[] inBlock, CompressionType type, int offset = 0)
        {
            if (offset < 0) offset = 0;
            byte[] outBlock;

            switch (type)
            {
                case CompressionType.GZIP:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Compresses gzip stream
                        GZipStream gzip = new GZipStream(new MemoryStream(), CompressionMode.Compress);
                        gzip.Write(inBlock, offset, inBlock.Length - offset);

                        gzip.CopyTo(ms);
                        outBlock = ms.ToArray();
                        gzip.Flush();
                    }
                    break;
                case CompressionType.ZLIB:
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Compresses zlib stream
                        ZOutputStream outZStream = new ZOutputStream(ms, zlibConst.Z_DEFAULT_COMPRESSION);
                        outZStream.Write(inBlock, offset, inBlock.Length - offset);
                        outZStream.finish();

                        outBlock = ms.ToArray();
                        outZStream.Flush();
                    }
                    return outBlock.Skip(2).ToArray(); // Returns without magic
                default:
                    outBlock = new byte[inBlock.Length];
                    Array.Copy(inBlock, outBlock, inBlock.Length);
                    break;
            }

            return outBlock;
        }
    }
}
