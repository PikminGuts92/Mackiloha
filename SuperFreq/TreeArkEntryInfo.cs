using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.IO.Path;

namespace SuperFreq
{
    public enum ArkEntryType : int
    {
        Default,
        Folder,
        Script,     // dtb
        Texture,    // png
        Audio,      // mogg, vgs
        Archive,    // milo
        Video,      // bik
        Midi        // mid
    }

    public class TreeArkEntryInfo
    {
        string _internalPath, _treeViewKey;
        bool _folder;
        ArkEntryType _fileType;

        public TreeArkEntryInfo(string internalPath, bool folder, string key)
        {
            _internalPath = internalPath;
            _folder = folder;
            _treeViewKey = key;
            _fileType = GetEntryType(internalPath, folder);
        }

        /// <summary>
        /// Determines entry type based on file extension
        /// </summary>
        /// <param name="path">Internal ark path</param>
        /// <param name="folder">Folder?</param>
        /// <returns>Entry type</returns>
        private ArkEntryType GetEntryType(string path, bool folder)
        {
            if (folder) return ArkEntryType.Folder;
            else if (path == null) return ArkEntryType.Default;

            switch (GetExtension(path).ToLowerInvariant())
            {
                // Switch case for known file types
                //case ".bin": // Amp?
                //case ".py":  // FreQ?
                case ".dtb":
                case ".dta_dta_pc": // RBVR
                case ".script_dta_pc":
                case ".fusion_dta_pc":
                    return ArkEntryType.Script;
                case ".bmp":
                case ".bmp_ps2":
                case ".bmp_ps3":
                case ".bmp_wii":
                case ".bmp_xbox":
                case ".png":
                case ".png_ps2":
                case ".png_ps3":
                case ".png_wii":
                case ".png_xbox":
                    return ArkEntryType.Texture;
                case ".mogg":
                case ".vgs":
                    return ArkEntryType.Audio;
                case ".gh":
                case ".milo_ps2":
                case ".milo_ps3":
                case ".milo_wii":
                case ".milo_xbox":
                case ".rnd":
                case ".rnd.gz":
                case ".rnd_ps2":
                    return ArkEntryType.Archive;
                case ".bik":
                case ".pss":
                    return ArkEntryType.Video;
                case ".mid":
                    return ArkEntryType.Midi;
                default:
                    return ArkEntryType.Default;
            }
        }

        /// <summary>
        /// Gets internal ark path
        /// </summary>
        public string InternalPath { get { return _internalPath; } }

        /// <summary>
        /// Gets boolean value of whether entry is a directory
        /// </summary>
        public bool IsDirectory { get { return _folder; } }

        /// <summary>
        /// Gets treeview key
        /// </summary>
        public string TreeViewKey { get { return _treeViewKey; } }

        /// <summary>
        /// Gets file type
        /// </summary>
        public ArkEntryType EntryType { get { return _fileType; } }
    }
}
