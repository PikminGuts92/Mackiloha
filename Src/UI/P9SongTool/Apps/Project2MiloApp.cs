using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Render;
using Mackiloha.Song;
using P9SongTool.Exceptions;
using P9SongTool.Helpers;
using P9SongTool.Json;
using P9SongTool.Models;
using P9SongTool.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace P9SongTool.Apps;

public class Project2MiloApp
{
    protected readonly (string extension, string miloType)[] SupportedExtraTypes;

    public Project2MiloApp()
    {
        SupportedExtraTypes = new (string extension, string miloType)[]
        {
            (".anim", "PropAnim"),
            (".font", "Font"),
            (".grp", "Group"),
            (".mat", "Mat"),
            (".mesh", "Mesh"),
            (".png", "PNG"), // Support png -> tex
            (".tex", "Tex"),
            (".txt", "Text")
        };
    }

    public void Parse(Project2MiloOptions op)
    {
        if (!Directory.Exists(op.InputPath))
            throw new MiloBuildException("Input directory doesn't exist");

        var inputDir = Path.GetFullPath(op.InputPath);
        var songMetaPath = Path.Combine(inputDir, "song.json");
        var midPath = Path.Combine(inputDir, "venue.mid");

        // Get lipsync files
        var lipsyncDir = Path.Combine(inputDir, "lipsync");
        var lipsyncPaths = Directory.Exists(lipsyncDir)
            ? Directory.GetFiles(lipsyncDir, "*.lipsync")
            : Array.Empty<string>();

        // Enforce files exist
        if (!File.Exists(songMetaPath))
            throw new MiloBuildException("Can't find \"song.json\" file");
        if (!File.Exists(midPath))
            throw new MiloBuildException("Can't find \"venue.mid\" file");

        var p9song = OpenP9File(songMetaPath);
        var songPref = ConvertFromSongPreferences(p9song.Preferences);
        var lyricConfigAnim = ConvertFromLyricConfigs(p9song.LyricConfigurations);

        var converter = new Midi2Anim(midPath);
        var anim = converter.ExportToAnim();

        // Create app state
        var state = new AppState(inputDir);
        state.UpdateSystemInfo(GetSystemInfo(op));

        var miloDir = CreateRootDirectory(p9song.Name?.ToLower() ?? "song");
        var miloDirEntry = miloDir.Extras["DirectoryEntry"] as MiloObjectDirEntry;

        // Add lipsync files
        foreach (var lipPath in lipsyncPaths)
        {
            var subDir = GetCharSubDirectory(lipPath);
            miloDirEntry.SubDirectories.Add(subDir);
        }

        // Add preference + anims
        miloDir.Entries.Add(songPref);
        miloDir.Entries.Add(anim);

        if (!(lyricConfigAnim is null))
        {
            miloDir.Entries.Add(lyricConfigAnim);
        }

        // Process extra files
        var extrasDir = Path.Combine(inputDir, "extra");
        if (Directory.Exists(extrasDir))
        {
            ProcessExtraFiles(extrasDir, miloDir, state);
        }

        // Merge anim entries in root directory
        var animEntries = miloDir
            .Entries
            .Where(x => x.Name != "song.anim"
                && x.Type == "PropAnim")
            .ToList();

        foreach (var animEntry in animEntries)
        {
            if (!(animEntry is MiloObjectBytes mob)) continue;

            try
            {
                // Try parsing as prop anim
                var extraAnim = state
                    .GetSerializer()
                    .ReadFromMiloObjectBytes<PropAnim>(mob);

                if (extraAnim.AnimName is "song_anim")
                {
                    MergePropAnims(anim, extraAnim);

                    // Remove old entry
                    miloDir.Entries.Remove(animEntry);
                    continue;
                }
            }
            catch { }
        }

        var serializer = state.GetSerializer();
        var outputMiloPath = Path.GetFullPath(op.OutputPath);

        var miloFile = new MiloFile
        {
            Data = serializer.WriteToBytes(miloDir)
        };

        miloFile.Structure = op.UncompressedMilo
            ? BlockStructure.MILO_A
            : BlockStructure.MILO_B;

        miloFile.WriteToFile(op.OutputPath);
        Log.Information("Successfully created milo at \"{OutputMiloPath}\" as {Compression}", outputMiloPath, op.UncompressedMilo ? "uncompressed" : "compressed");
    }

    protected void ProcessExtraFiles(string extrasDir, MiloObjectDir miloDir, AppState state)
    {
        var existingEntries = miloDir
            .Entries
            .Select(x => x.Name.ToLower())
            .ToHashSet();

        // Get extra files
        var extraFiles = GetExtraFilesToProcess(extrasDir, miloDir);

        // Iterate over extra milo objects
        Parallel.ForEach(extraFiles,
            extraFile =>
            {
                var (miloDir, miloType, path) = extraFile;

                var miloObj = miloType switch
                {
                    "PNG" => CreateTex(path, state.SystemInfo),
                    _ => CreateObject(path, miloType)
                };

                // Add milo object to directory
                lock (miloDir)
                {
                    miloDir.Entries.Add(miloObj);
                }
            });
    }

    protected List<(MiloObjectDir, string, string)> GetExtraFilesToProcess(string extraPath, MiloObjectDir miloDir)
    {
        var filesToProcess = new List<(MiloObjectDir, string, string)>(); // (Dir, type, path)

        var existingEntries = miloDir
            .Entries
            .Select(x => x.Name.ToLower())
            .ToHashSet();

        // Process entries
        foreach (var filePath in Directory.GetFiles(extraPath))
        {
            // Get milo type
            var miloType = GetMiloType(filePath);

            // Ignore unsupported files and existing entries
            if (miloType is null
                || (existingEntries.Contains(Path.GetFileName(filePath).ToLower())))
                continue;

            filesToProcess.Add((miloDir, miloType, filePath));
        }

        var miloDirEntry = miloDir.Extras["DirectoryEntry"] as MiloObjectDirEntry;

        var existingDirs = miloDirEntry
            .SubDirectories
            .ToDictionary(x => x.Name.ToLower(), y => y);

        // Process directories
        foreach (var dirPath in Directory.GetDirectories(extraPath))
        {
            var dirName = Path.GetFileName(dirPath);
            var isNewDir = false;

            if (!existingDirs.TryGetValue(dirName.ToLower(), out var subDir))
            {
                // Create sub dir
                subDir = CreateMiloDir(dirName);
                isNewDir = true;
            }

            var subFiles = GetExtraFilesToProcess(dirPath, subDir);
            if (!subFiles.Any()) continue;

            // Add files to process
            filesToProcess.AddRange(subFiles);
            if (isNewDir) miloDirEntry.SubDirectories.Add(subDir);
        }

        return filesToProcess;
    }

    protected string GetMiloType(string path)
        => SupportedExtraTypes
            .Where(x => path.EndsWith(x.extension, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => x.miloType)
            .FirstOrDefault();

    protected void MergePropAnims(PropAnim baseAnim, PropAnim anim)
    {
        foreach (var inDirGroup in anim.DirectorGroups)
        {
            var baseDirGroupIdx = baseAnim
                .DirectorGroups
                .FindIndex(x => x.DirectorName == inDirGroup.DirectorName
                    && x.PropName == inDirGroup.PropName
                    && x.PropName2 == inDirGroup.PropName2);

            if (baseDirGroupIdx == -1)
            {
                baseAnim.DirectorGroups.Add(inDirGroup);
                continue;
            }

            // TODO: Merge events to existing group
        }
    }

    protected MiloObject CreateObject(string path, string type)
    {
        var fileName = Path.GetFileName(path);
        var data = File.ReadAllBytes(path);
        Log.Information("Adding \"{FileName}\" as {Type}", fileName, type);

        return new MiloObjectBytes(type)
        {
            Name = fileName,
            Data = data
        };
    }

    protected Tex CreateTex(string pngPath, SystemInfo info)
    {
        var fileName = Path.GetFileName(pngPath);
        Log.Information("Adding \"{FileName}\" (and encoding) as {Type}", fileName, "Tex");

        return TextureExtensions
            .TexFromImage(pngPath, info);
    }

    protected P9Song OpenP9File(string p9songPath)
    {
        var appState = AppState.FromFile(p9songPath);
        var jsonText = File.ReadAllText(p9songPath);

        return JsonSerializer.Deserialize<P9Song>(jsonText, P9SongToolJsonContext.Default.P9Song);
    }

    protected P9SongPref ConvertFromSongPreferences(SongPreferences preferences)
    {
        var songPref = new P9SongPref();

        songPref.Name = "P9SongPref";
        songPref.Venue = preferences.Venue;
        songPref.MiniVenues.AddRange(preferences.MiniVenues);
        songPref.Scenes.AddRange(preferences.Scenes);

        songPref.DreamscapeOutfit = preferences.DreamscapeOutfit;
        songPref.StudioOutfit = preferences.StudioOutfit;

        songPref.GeorgeInstruments.AddRange(preferences.GeorgeInstruments);
        songPref.JohnInstruments.AddRange(preferences.JohnInstruments);
        songPref.PaulInstruments.AddRange(preferences.PaulInstruments);
        songPref.RingoInstruments.AddRange(preferences.RingoInstruments);

        songPref.Tempo = preferences.Tempo;
        songPref.SongClips = preferences.SongClips;
        songPref.DreamscapeFont = preferences.DreamscapeFont;

        // TBRB specific
        songPref.GeorgeAmp = preferences.GeorgeAmp;
        songPref.JohnAmp = preferences.JohnAmp;
        songPref.PaulAmp = preferences.PaulAmp;
        songPref.Mixer = preferences.Mixer;

        var camera = preferences.DreamscapeCamera;
        if (!(camera is null)
            && (camera != "None")
            && !camera.StartsWith("kP9"))
            camera = $"kP9{camera}"; // Prepend with prefix

        Enum.TryParse<DreamscapeCamera>(camera, out var dreamCam);
        songPref.DreamscapeCamera = dreamCam;

        songPref.LyricPart = preferences.LyricPart;

        return songPref;
    }

    protected PropAnim ConvertFromLyricConfigs(LyricConfig[] lyricConfigs)
    {
        if (lyricConfigs is null || lyricConfigs.Length <= 0)
        {
            return default;
        }

        var lyricAnim = new PropAnim()
        {
            Name = "lyric_config.anim",
            AnimName = "",
            TotalTime = lyricConfigs.Count() // Directed cuts count
        };

        var maxLyricCount = lyricConfigs
            .Select(x => x.Lyrics.Count())
            .Max();

        var propGroups = Enumerable
            .Range(1, maxLyricCount)
            .Select(i =>
            {
                var name = $"venue_lyric{i:d2}";

                return new []
                {
                    Midi2Anim.CreateDirectorGroup("position", name), // Pos
                    Midi2Anim.CreateDirectorGroup("rotation", name), // Rot
                    Midi2Anim.CreateDirectorGroup("scale", name)     // Scale
                };
            })
            .ToArray();

        // Iterate over directed lyric cuts
        foreach (var lyricConfig in lyricConfigs
            // TODO: Some sort of name validation
            .Select(x => (int.Parse(x.Name.Substring(x.Name.LastIndexOf("_") + 1)), x))
            .OrderBy(x => x.Item1))
        {
            var dcIndex = lyricConfig.Item1;

            // Iterate over lyrics
            int i = 0;
            foreach (var lyric in lyricConfig.x.Lyrics)
            {
                // Get indexed groups
                var posGroup = propGroups[i][0];
                var rotGroup = propGroups[i][1];
                var scaleGroup = propGroups[i][2];

                // Convert position
                var posEvent = new DirectedEventVector3()
                {
                    Position = dcIndex,
                    Value = new Vector3()
                    {
                        X = (!(lyric.Position is null)
                            && lyric.Position.Length > 0)
                            ? lyric.Position[0]
                            : 0.0f,
                        Y = (!(lyric.Position is null)
                            && lyric.Position.Length > 1)
                            ? lyric.Position[1]
                            : 0.0f,
                        Z = (!(lyric.Position is null)
                            && lyric.Position.Length > 2)
                            ? lyric.Position[2]
                            : 0.0f
                    }
                };
                posGroup.Events.Add(posEvent);

                // Convert rotation
                var rotEvent = new DirectedEventVector4()
                {
                    Position = dcIndex,
                    Value = new Vector4()
                    {
                        X = (!(lyric.Rotation is null)
                            && lyric.Rotation.Length > 0)
                            ? lyric.Rotation[0]
                            : 0.0f,
                        Y = (!(lyric.Rotation is null)
                            && lyric.Rotation.Length > 1)
                            ? lyric.Rotation[1]
                            : 0.0f,
                        Z = (!(lyric.Rotation is null)
                            && lyric.Rotation.Length > 2)
                            ? lyric.Rotation[2]
                            : 0.0f,
                        W = (!(lyric.Rotation is null)
                            && lyric.Rotation.Length > 3)
                            ? lyric.Rotation[3]
                            : 1.0f,
                    }
                };
                rotGroup.Events.Add(rotEvent);

                // Convert scale
                var scaleEvent = new DirectedEventVector3()
                {
                    Position = dcIndex,
                    Value = new Vector3()
                    {
                        X = (!(lyric.Scale is null)
                            && lyric.Scale.Length > 0)
                            ? lyric.Scale[0]
                            : 1.0f,
                        Y = (!(lyric.Scale is null)
                            && lyric.Scale.Length > 1)
                            ? lyric.Scale[1]
                            : 1.0f,
                        Z = (!(lyric.Scale is null)
                            && lyric.Scale.Length > 2)
                            ? lyric.Scale[2]
                            : 1.0f
                    }
                };
                scaleGroup.Events.Add(scaleEvent);

                i++;
            }
        }

        // Add groups to lyric anim
        foreach (var group in propGroups)
        {
            foreach (var subGroup in group)
            {
                lyricAnim.DirectorGroups.Add(subGroup);
            }
        }

        return lyricAnim;
    }

    protected SystemInfo GetSystemInfo(Project2MiloOptions op)
        => new SystemInfo()
        {
            Version = 25,
            BigEndian = true,
            Platform = op.OutputPath
                .ToLower()
                .EndsWith("_ps3")
                ? Platform.PS3
                : Platform.X360
        };

    protected MiloObjectDir CreateRootDirectory(string name)
    {
        var miloDirEntry = new MiloObjectDirEntry()
        {
            Name = name,
            Version = 22,
            SubVersion = 2,
            ProjectName = "song",
            ImportedMiloPaths = new []
            {
                "../../world/shared/camera.milo",
                "../../world/shared/director.milo"
            },
            SubDirectories = new List<MiloObjectDir>()
        };

        var miloDir = new MiloObjectDir()
        {
            Name = name ?? ""
        };

        miloDir.Extras.Add("DirectoryEntry", miloDirEntry);
        return miloDir;
    }

    protected MiloObjectDir CreateMiloDir(string name)
    {
        var miloDirEntry = new MiloObjectDirEntry()
        {
            Name = name,
            Version = 22,
            SubVersion = 2,
            ProjectName = "",
            ImportedMiloPaths = Array.Empty<string>(),
            SubDirectories = new List<MiloObjectDir>()
        };

        var miloDir = new MiloObjectDir()
        {
            Name = name
        };

        miloDir.Extras.Add("DirectoryEntry", miloDirEntry);
        return miloDir;
    }

    protected MiloObjectDir GetCharSubDirectory(string lipPath)
    {
        var name = Path.GetFileNameWithoutExtension(lipPath).ToLower();
        var fileName = Path.GetFileName(lipPath).ToLower();
        var data = File.ReadAllBytes(lipPath);

        var lipsync = new MiloObjectBytes("CharLipSync")
        {
            Name = fileName,
            Data = data
        };

        var miloDir = CreateMiloDir(name);

        miloDir.Entries.Add(lipsync);
        return miloDir;
    }
}
