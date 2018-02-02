using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mackiloha.Chunk;

namespace ChunkyBear
{
    class Program
    {
        static void Main(string[] args)
        {
            // Decompresses Chunk archives
            if (args.Length < 1) return;

            string input = args[0], output;

            if (args.Length == 1)
            {
                string path = Path.GetDirectoryName(input);
                string fileName = Path.GetFileName(input);

                if (fileName.Contains("."))
                    fileName = fileName.Replace(".", "_output.");
                else
                    fileName += "_output";

                output = Path.Combine(path, fileName);
            }
            else
                output = args[1];

            Chunk.DecompressChunkFile(input, output);
        }
    }
}
