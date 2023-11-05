using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mackiloha;
using Mackiloha.App.Json;
using Mackiloha.App.Metadata;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Render;

namespace Mackiloha.App.Extensions
{
    public static class MiloExtensions
    {
        public static int Size(this MiloObject entry) => entry is MiloObjectBytes ? (entry as MiloObjectBytes).Data.Length : -1;

        public static string Extension(this MiloObject entry)
        {
            if (entry == null || !((string)entry.Name).Contains('.')) return "";
            return Path.GetExtension(entry.Name); // Returns .cs
        }

        private static string MakeGenPath(string path, Platform platform)
        {
            var ext = (platform) switch
            {
                Platform.PS2 => "ps2",
                Platform.X360 => "xbox",
                _ => ""
            };

            var dir = Path.GetDirectoryName(path);
            var fileName = $"{Path.GetFileName(path)}_{ext}"; // TODO: Get platform extension from app state

            return Path.Combine(dir, "gen", fileName);
        }

        public static void ExtractToDirectory(this MiloObjectDir milo, string path, bool convertTextures, AppState state)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            /*
            var entriesByType = milo.Entries
                .GroupBy(x => x.Type)
                .ToDictionary(x => Path.Combine(path, x.Key), y => y.ToList())
                .ToList();*/

            var miloEntries = new List<MiloObject>();

            if (milo.Extras.ContainsKey("DirectoryEntry"))
            {
                var dirEntry = milo.Extras["DirectoryEntry"] as MiloObjectBytes;
                if (dirEntry != null)
                {
                    if (string.IsNullOrWhiteSpace(dirEntry.Name) && !string.IsNullOrWhiteSpace(dirEntry.Type))
                    {
                        // TODO: Handle differently?
                        // Some directory names are empty for whatever reason
                        dirEntry.Name = Path.GetFileNameWithoutExtension(milo.Name);
                    }
                    miloEntries.Add(dirEntry);

                    // Create meta
                    var dirMeta = new DirectoryMeta()
                    {
                        Name = dirEntry.Name,
                        Type = dirEntry.Type
                    };

                    // Write meta
                    var metaPath = Path.Combine(path, "rnd.json");
                    var metaJson = JsonSerializer.Serialize(dirMeta, MackilohaJsonContext.Default.DirectoryMeta);
                    File.WriteAllText(metaPath, metaJson);
                }
            }
            miloEntries.AddRange(milo.Entries);

            // Sanitize paths
            foreach (var entry in miloEntries)
            {
                // TODO: Remove \t and other escape characters
                entry.Name = entry.Name.Trim();
            }

            var typeDirs = miloEntries
                .Select(x => Path.Combine(path, x.Type))
                .Distinct()
                .ToList();

            foreach (var dir in typeDirs)
            {
                if (Directory.Exists(dir))
                    continue;

                Directory.CreateDirectory(dir);
            }

            // Filter out textures if converting
            var entries = miloEntries
                .Where(x => !convertTextures || x.Type != "Tex")
                .ToList();

            foreach (var entry in entries)
            {
                // TODO: Sanitize file name
                var filePath = Path.Combine(path, entry.Type, entry.Name);
                if (entry is MiloObjectBytes miloBytes)
                {
                    File.WriteAllBytes(filePath, miloBytes.Data);
                }
            }

            if (!convertTextures)
                return;

            var serializer = state.GetSerializer();

            var textureEntries = miloEntries
                .Where(x => x.Type == "Tex")
                .Select(x => x is Tex ? x as Tex : serializer.ReadFromMiloObjectBytes<Tex>(x as MiloObjectBytes))
                .ToList();

            // Update textures
            foreach (var texture in textureEntries.Where(x => x.UseExternal))
            {
                if (texture?.Bitmap?.RawData?.Length > 0)
                {
                    // Use already embedded texture instead
                    texture.UseExternal = false;
                    continue;
                }

                try
                {
                    var texPath = Path.Combine(state.GetWorkingDirectory().FullPath, MakeGenPath(texture.ExternalPath, state.SystemInfo.Platform));
                    var bitmap = serializer.ReadFromFile<HMXBitmap>(texPath);

                    texture.Bitmap = bitmap;
                    texture.UseExternal = false;
                }
                catch
                {

                }
            }

            var defaultMeta = TexMeta.DefaultFor(state.SystemInfo.Platform);
            foreach (var texEntry in textureEntries)
            {
                var entryName = Path.GetFileNameWithoutExtension(texEntry.Name);

                // TODO: Skip?
                texEntry.UseExternal = false;
                if (texEntry.Bitmap is null) continue; // Skip for now

                if (texEntry.UseExternal)
                    throw new NotSupportedException("Can't extract external textures yet");

                // Saves png
                var pngName = $"{entryName}.png";
                var pngPath = Path.Combine(path, texEntry.Type, pngName);
                texEntry.Bitmap.SaveAs(state.SystemInfo, pngPath);

                // Saves metadata
                var metaName = $"{entryName}.meta.json";
                var metaPath = Path.Combine(path, texEntry.Type, metaName);

                var meta = new TexMeta()
                {
                    Encoding = (texEntry.Bitmap.Encoding, state.SystemInfo.Platform) switch
                    {
                        (var enc, _) when enc == 3 => TexEncoding.Bitmap,
                        (var enc, var plat) when enc == 8 && plat == Platform.XBOX => TexEncoding.Bitmap,
                        (var enc, _) when enc == 8 => TexEncoding.DXT1,
                        (var enc, _) when enc == 24 => TexEncoding.DXT5,
                        (var enc, _) when enc == 32 => TexEncoding.ATI2,
                        _ => (TexEncoding?)null
                    },
                    MipMaps = texEntry.Bitmap.MipMaps > 0
                };

                if ((meta.Encoding == null || meta.Encoding == defaultMeta.Encoding) && meta.MipMaps == defaultMeta.MipMaps)
                    continue;

                var metaJson = JsonSerializer.Serialize(meta, MackilohaJsonContext.Default.TexMeta);
                File.WriteAllText(metaPath, metaJson);
            }
        }

        /*
        public static void WriteTree(this MiloObjectDir milo, string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach (var view in milo.Entries.Where(x => x.Type == "View"))
                    WriteTree(milo, view.Name, sw, 0);
            }
        }

        public static void WriteTree2(this MiloFile milo, string path)
        {
            MiloOG.AbstractEntry GetOGEntry(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view;
                    default:
                        return null;
                }
            }

            string GetTransformName(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh.Transform;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans.Name;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view.Transform;
                    default:
                        return null;
                }
            }

            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                var children = new Dictionary<string, List<string>>();
                
                foreach (var entry in milo.Entries)
                {
                    // if (!children.ContainsKey(entry.Name))
                    //     children.Add(entry.Name, new List<string>());

                    var trans = GetTransformName(entry.Name);
                    if (trans == null || trans == entry.Name) continue;

                    if (!children.ContainsKey(trans))
                        children.Add(trans, new List<string>(new string[] { entry.Name }));
                    else if (!children[trans].Contains(entry.Name))
                        children[trans].Add(entry.Name);

                    //WriteTree(milo, view.Name, sw, 0);
                }
            }
        }

        private static void WriteTree(MiloFile milo, string entry, StreamWriter sw, int depth, bool bone = false)
        {
            MiloOG.AbstractEntry GetOGEntry(string name)
            {
                var e = milo.Entries.First(x => x.Name == name) as MiloEntry;

                switch (e.Type)
                {
                    case "Mesh":
                        var mesh = MiloOG.Mesh.FromStream(new MemoryStream(e.Data));
                        mesh.Name = e.Name;
                        return mesh;
                    case "Trans":
                        var trans = MiloOG.Trans.FromStream(new MemoryStream(e.Data));
                        trans.Name = e.Name;
                        return trans;
                    case "View":
                        var view = MiloOG.View.FromStream(new MemoryStream(e.Data));
                        view.Name = e.Name;
                        return view;
                    default:
                        return null;
                }
            }

            dynamic transEntry = GetOGEntry(entry);
            List<string> subBones = transEntry.Meshes;
            List<string> subEntries = transEntry.Meshes;
            string type = bone ? "Bone" : "Mesh";

            sw.WriteLine($"{new string('\t', depth)}{type}: {transEntry.Name} ({transEntry.Transform})");

            foreach (var sub in subBones)
            {
                WriteTree(milo, sub, sw, depth + 1, true);
            }

            foreach (var sub in subEntries)
            {
                WriteTree(milo, sub, sw, depth + 1);
            }
        }*/
    }
}
