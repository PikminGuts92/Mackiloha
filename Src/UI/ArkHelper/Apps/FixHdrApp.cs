using ArkHelper.Options;
using Mackiloha.Ark;
using Mackiloha.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHelper.Apps
{
    public class FixHdrApp
    {
        public void Parse(FixHdrOptions op)
        {
            if (op.OutputPath == null)
                op.OutputPath = op.InputPath;

            var ark = ArkFile.FromFile(op.InputPath);
            ark.Encrypted = ark.Encrypted || op.ForceEncryption; // Force encryption
            ark.WriteHeader(op.OutputPath);
        }
    }
}
