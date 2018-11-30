using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mackiloha.Milo2;
using Mackiloha;

namespace Boom.Extensions
{
    public static class MiscExtensions
    {
        private static int GetNumber(byte[] data, bool bigEndian) =>
            (bigEndian) ? (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | (data[3])
                        : (data[3] << 24) | (data[2] << 16) | (data[1] << 8) | (data[0]);

        public static int GetMagic(this MiloObject entry)
        {
            var entryBytes = entry as MiloObjectBytes;

            if (entryBytes == null || entryBytes.Data == null || entryBytes.Data.Length < 4)
                return -1;
            
            var magic = GetNumber(entryBytes.Data, false);
            if (magic < 0 || magic > 100) magic = GetNumber(entryBytes.Data, true);

            return magic;
        }
    }
}
