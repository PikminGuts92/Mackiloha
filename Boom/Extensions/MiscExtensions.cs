using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mackiloha.Milo2;

namespace Boom.Extensions
{
    public static class MiscExtensions
    {
        private static int GetNumber(byte[] data, bool bigEndian) =>
            (bigEndian) ? (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | (data[3])
                        : (data[3] << 24) | (data[2] << 16) | (data[1] << 8) | (data[0]);

        public static int GetMagic(this MiloEntry entry)
        {
            if (entry == null || entry.Data == null || entry.Data.Length < 4)
                return -1;

            var magic = GetNumber(entry.Data, false);
            if (magic < 0 || magic > 100) magic = GetNumber(entry.Data, true);

            return magic;
        }
    }
}
