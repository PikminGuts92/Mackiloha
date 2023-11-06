using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Mackiloha;

namespace SuperFreqCLI.Options
{
    [Verb("crypt", HelpText = "Encrypt and decrypt dtb files")]
    public class CryptOptions
    {
        [Value(0, Required = true, MetaName = "inputPath", HelpText = "Path to input file")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "outputPath", HelpText = "Path to output file")]
        public string OutputPath { get; set; }

        [Option('d', HelpText = "New style decryption")]
        public bool DecryptNew { get; set; }

        [Option('e', HelpText = "New style encryption")]
        public bool EncryptNew { get; set; }

        [Option('D', HelpText = "Old style decryption")]
        public bool DecryptOld { get; set; }

        [Option('E', HelpText = "Old style encryption")]
        public bool EncryptOld { get; set; }

        [Option('k', "key", Default = 0x295E2D5E, HelpText = "Key to use (for encryption)")]
        public int Key { get; set; }

        [Option('x', "xor", HelpText = "Value to xor with crypt (advanced use only)")]
        public byte Xor { get; set; }

        public static void Parse(CryptOptions op)
        {
            var modeCount = new []
                {
                    op.DecryptNew,
                    op.EncryptNew,
                    op.DecryptOld,
                    op.EncryptOld
                }
                .Where(x => x)
                .Count();

            if (modeCount == 0)
            {
                Log.Error("At least one crypt mode (-d, -e, -D, -E) must be set");
                return;
            }
            else if (modeCount > 1)
            {
                Log.Error("Only a single crypt mode (-d, -e, -D, -E) can be set at a time");
                return;
            }

            // Checks if inputs are files
            if (Directory.Exists(op.InputPath))
            {
                Log.Error("Input of \"{InputPath}\" is a directory, not file", op.InputPath);
                return;
            }
            else if (!File.Exists(op.InputPath))
            {
                Log.Error("Input of \"{InputPath}\" does not exist", op.InputPath);
                return;
            }

            var newStyle = op.DecryptNew || op.EncryptNew;
            var encrypt = op.EncryptNew || op.EncryptOld;
            
            if (encrypt)
            {
                Crypt.EncryptFile(op.InputPath, op.OutputPath, newStyle, op.Key, op.Xor);
            }
            else
            {
                Crypt.DecryptFile(op.InputPath, op.OutputPath, newStyle, op.Xor);
            }

            var encMode = encrypt
                ? "encrypted"
                : "decrypted";

            Log.Information("Successfully {Encryption} \"{InputPath}\" and wrote output to \"{OutputPath}\"", encMode, op.InputPath, op.OutputPath);
        }
    }
}
