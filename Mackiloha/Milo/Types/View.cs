using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public enum ViewVersion : int
    {
        GH1 = 7
    }

    public class View : AbstractEntry
    {
        private ViewVersion _version;
        private Matrix _mat1, _mat2;

        private List<string> _views;
        private List<string> _bones;
        private string _transform;
        private List<string> _meshes;

        public View(string name, bool bigEndian = true) : base(name, "View", bigEndian)
        {
            _version = ViewVersion.GH1;
            _views = new List<string>();
            _bones = new List<string>();
            _transform = null;
            _meshes = new List<string>();
        }

        public static View FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static View FromStream(Stream input)
        {
            View view = new View("");

            using (AwesomeReader ar = new AwesomeReader(input))
            {
                bool valid;

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out view._version, out valid);
                if (!valid) return null;
                
                ar.BaseStream.Position += 4; // Skips zero'd bytes

                uint unknownEntries = ar.ReadUInt32();
                for (int i = 0; i < unknownEntries; i++)
                {
                    ar.ReadString(); // Usually 0 or 1 in length
                    ar.BaseStream.Position += 8; // Two floats (Exactly 0 or close to it)
                }

                // Reads sub views (Usually 0)
                string[] views = new string[ar.ReadUInt32()];
                for (int i = 0; i < views.Length; i++) views[i] = ar.ReadString();
                view._views = new List<string>(views);

                ar.BaseStream.Position += 4; // 8 constant

                // Reads in matrix tables
                view._mat1 = Matrix.FromStream(ar);
                view._mat2 = Matrix.FromStream(ar);

                // Reads sub mesh strings
                if (view._version == ViewVersion.GH1)
                {
                    string[] submeshes = new string[ar.ReadUInt32()];
                    for (int i = 0; i < submeshes.Length; i++) submeshes[i] = ar.ReadString();

                    view._meshes = new List<string>(submeshes);
                }

                // Skips unknown stuff
                ar.BaseStream.Position += 9;
                view._transform = ar.ReadString(); // Reads view

                // Skipping these other mesh strings
                ar.BaseStream.Position += 5;
                uint meshCount = ar.ReadUInt32();
                view._meshes = new List<string>();

                for (int i = 0; i < meshCount; i++) view._meshes.Add(ar.ReadString());
                ar.BaseStream.Position += 16; // Four floats - Bounding box

                ar.ReadString(); // View (again)
                ar.ReadInt32();
                ar.ReadInt32();
            }

            return view;
        }

        public static View FromStreamAsGroup(Stream input)
        {
            // TODO: Delete this
            // HACKY GH2 FIX!!!
            View view = new View("");

            using (AwesomeReader ar = new AwesomeReader(input))
            {
                // Guesses endianess
                view._version = (ViewVersion)ar.ReadInt32(); // Should be 12 (11 in OPM demo)
                ar.BigEndian = false;

                // Skips constants + unknown data
                ar.BaseStream.Position += 25;
                
                // Reads in matrix tables
                view._mat1 = Matrix.FromStream(ar);
                view._mat2 = Matrix.FromStream(ar);
                
                ar.BaseStream.Position += 9;

                ar.ReadString(); // View name (Not always present)

                ar.BaseStream.Position += 8; // 3, 1
                ar.BaseStream.Position += 17;

                uint meshCount = ar.ReadUInt32();
                view._meshes = new List<string>();

                for (int i = 0; i < meshCount; i++) view._meshes.Add(ar.ReadString());
            }

            return view;
        }

        private static bool DetermineEndianess(byte[] head, out ViewVersion version, out bool valid)
        {
            bool bigEndian = false;
            version = (ViewVersion)BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = (ViewVersion)BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(ViewVersion version)
        {
            switch (version)
            {
                case ViewVersion.GH1: // PS2 - GH1
                    return true;
                default:
                    return false;
            }
        }

        public Matrix Mat1 { get { return _mat1; } }
        public Matrix Mat2 { get { return _mat2; } }

        public List<string> Views => _views;
        public List<string> Bones => _bones;
        public string Transform => _transform;
        public List<string> Meshes => _meshes;

        public override byte[] Data => throw new NotImplementedException();
    }
}
