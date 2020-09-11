using ArkHelper.Exceptions;
using CliWrap;
using Mackiloha.DTB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkHelper.Helpers
{
    public class ScriptHelper
    {
        private void WriteOutput(string text)
            => Console.WriteLine(text);

        public void ConvertNewDtbToOld(string newDtbPath, string oldDtbPath, bool fme = false)
        {
            var encoding = fme ? DTBEncoding.FME : DTBEncoding.RBVR;

            var dtb = DTBFile.FromFile(newDtbPath, encoding);
            dtb.Encoding = DTBEncoding.Classic;
            dtb.SaveToFile(oldDtbPath);
        }

        public string CreateDTAFile(string dtbPath, string tempDir, bool newEncryption, int arkVersion, string dtaPath = null)
        {
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            var decDtbPath = Path.Combine(tempDir, Path.GetRandomFileName());
            dtaPath = dtaPath ?? Path.Combine(tempDir, Path.GetRandomFileName());

            var dtaDir = Path.GetDirectoryName(dtaPath);
            if (!Directory.Exists(dtaDir))
                Directory.CreateDirectory(dtaDir);

            if (arkVersion < 7)
            {
                // Decrypt dtb
                Cli.Wrap("dtab")
                    .SetArguments(new[]
                    {
                        newEncryption ? "-d" : "-D",
                        dtbPath,
                        decDtbPath
                    })
                    .Execute();
            }
            else
            {
                // New dtb style, convert to old format to use dtab
                //  and assume not encrypted
                ConvertNewDtbToOld(dtbPath, decDtbPath, arkVersion < 9);
            }

            // Convert to dta (plaintext)
            var result = Cli.Wrap("dtab")
                .SetArguments(new[]
                {
                    "-a",
                    decDtbPath,
                    dtaPath
                })
                .EnableExitCodeValidation(false)
                .SetStandardOutputCallback(WriteOutput)
                .SetStandardErrorCallback(WriteOutput)
                .Execute();

            if (result.ExitCode != 0)
                throw new DTBParseException($"dtab.exe was unable to parse file from \'{decDtbPath}\'");

            return dtaPath;
        }
    }
}
