using ArkHelper.Exceptions;
using ArkHelper.Helpers;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.CSV;
using Mackiloha.Milo2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkHelper.Apps
{
    public class Dir2ArkApp
    {
        protected readonly IScriptHelper ScriptHelper;

        public Dir2ArkApp(IScriptHelper scriptHelper)
        {
            ScriptHelper = scriptHelper;
        }

        public void Parse(Dir2ArkOptions op)
        {
            var arkDir = Path.GetFullPath(op.OutputPath);

            // Create directory if it doesn't exist
            if (!Directory.Exists(arkDir))
                Directory.CreateDirectory(arkDir);


            // Create ark
            var hdrPath = Path.Combine(arkDir, $"{op.ArkName}.hdr");
            var ark = ArkFile.Create(hdrPath, (ArkVersion)op.ArkVersion, (int?)op.Key);

            var files = Directory.GetFiles(op.InputPath, "*", SearchOption.AllDirectories);


            foreach (var file in files)
            {
                var internalPath = FileHelper.GetRelativePath(file, op.InputPath)
                    .Replace("\\", "/");


            }
        }
    }
}
