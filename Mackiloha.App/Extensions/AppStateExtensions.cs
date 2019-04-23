using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mackiloha.IO;
using Mackiloha.Milo2;

namespace Mackiloha.App.Extensions
{
    public static class AppStateExtensions
    {
        public static void ExtractMiloContents(this AppState state, string miloPath, string outputDir, bool convertTextures)
        {
            MiloFile miloFile;
            using (var fileStream = state.GetWorkingDirectory().GetStreamForFile(miloPath))
            {
                miloFile = MiloFile.ReadFromStream(fileStream);
            }

            var serializer = new MiloSerializer(new SystemInfo()
            {
                Version = miloFile.Version,
                BigEndian = miloFile.BigEndian
            });

            MiloObjectDir milo;
            using (var miloStream = new MemoryStream(miloFile.Data))
            {
                milo = serializer.ReadFromStream<MiloObjectDir>(miloStream);
            }

            milo.ExtractToDirectory(outputDir, convertTextures);
        }
    }
}
