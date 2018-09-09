using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public enum MeshVersion : int
    {
        GH1 = 25,
        GH2 = 28,
        GH2_X360 = 29, // And GH2 360?
        RB1 = 34, // RB2/LRB
        TBRB = 36,
        GDRB = 37,
        RB3 = 38
    }

    public class Mesh : AbstractEntry
    {
        private MeshVersion _version;
        private Matrix _mat1, _mat2;
        private Vertex[] _vertices;
        private ushort[][] _faces;

        private List<string> _bones;
        private string _transform;
        private List<string> _meshes;

        public Mesh(string name, bool bigEndian = true) : base(name, "Mesh", bigEndian)
        {
            _version = MeshVersion.GH1;
            _bones = new List<string>();
            _transform = null;
            _meshes = new List<string>();
        }

        public static Mesh FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static Mesh FromStream(Stream input)
        {
            Mesh mesh = new Mesh("");
            
            using (AwesomeReader ar = new AwesomeReader(input))
            {
                bool valid;

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out mesh._version, out valid);
                if (!valid) return null;

                if (mesh._version == MeshVersion.GH1) ar.BaseStream.Position += 4; // Always 8
                else if (mesh._version == MeshVersion.GH2)
                {
                    ar.BaseStream.Position += 9;
                    if (ar.ReadInt32() != 9)
                    {
                        // X360 version has extra data
                        mesh._version = MeshVersion.GH2_X360;
                        ar.BaseStream.Position += 4;
                    }
                }
                else ar.BaseStream.Position += 17;

                // Reads in matrix tables
                mesh._mat1 = Matrix.FromStream(ar);
                mesh._mat2 = Matrix.FromStream(ar);

                // Reads sub mesh strings
                if (mesh._version == MeshVersion.GH1)
                {
                    string[] submeshes = new string[ar.ReadUInt32()];
                    for (int i = 0; i < submeshes.Length; i++) submeshes[i] = ar.ReadString();

                    mesh._meshes = new List<string>(submeshes);
                }

                ar.ReadUInt32(); // Always 0?
                ar.ReadString(); // Camera view?
                ar.ReadByte(); // Always 0

                mesh._transform = ar.ReadString(); // Reads view

                if (mesh._version <= MeshVersion.GH1)
                {
                    // Skipping these other mesh strings
                    ar.BaseStream.Position += 5;
                    uint meshCount = ar.ReadUInt32();
                    mesh._meshes = new List<string>();

                    for (int i = 0; i < meshCount; i++) mesh._meshes.Add(ar.ReadString());
                    ar.BaseStream.Position += 16; // Four floats - Bounding box
                }
                else
                {
                    ar.BaseStream.Position += 25;
                    if (mesh._version == MeshVersion.GDRB) ar.BaseStream.Position += 4;
                }
                
                mesh.Material = ar.ReadString(); // Reads material
                ar.ReadString(); // Reads mesh name
                ar.BaseStream.Position += 9;

                mesh._vertices = new Vertex[ar.ReadUInt32()];
                if ((int)mesh._version >= 36) ar.BaseStream.Position += 9; // Skips unknown stuff

                // Reads vertex data
                for (int i = 0; i < mesh._vertices.Length; i++)
                {
                    #region Vertices Notes
                    // GH1/GH2: 48 bytes
                    // float Vert X
                    // float Vert Y
                    // float Vert Z
                    // float Norm X
                    // float Norm Y
                    // float Norm Z
                    // float Color R
                    // float Color G
                    // float Color B
                    // float Color A
                    // float Texture Coordinate U
                    // float Texture Coordinate V

                    // RB1-RB2: 80 bytes
                    // float Vert X
                    // float Vert Y
                    // float Vert Z
                    // float Vert W *
                    // float Norm X
                    // float Norm Y
                    // float Norm Z
                    // float Norm W *
                    // float Color R
                    // float Color G
                    // float Color B
                    // float Color A
                    // float Texture Coordinate U
                    // float Texture Coordinate V
                    // short Unknown 1 *
                    // short Unknown 2 *
                    // short Unknown 3 *
                    // short Unknown 4 *
                    // float Unknown 5 *
                    // float Unknown 6 *
                    // float Unknown 7 *
                    // float Unknown 8 *

                    // TBRB-RB3: 36 bytes
                    // float Vert X
                    // float Vert Y
                    // float Vert Z
                    // byte Color R
                    // byte Color G
                    // byte Color B
                    // byte Color A
                    // half Texture Coordinate U
                    // half Texture Coordinate V
                    // half Norm X
                    // half Norm Y
                    // half Norm Z
                    // half Norm W
                    // byte Unknown 1 -1 or 0
                    // byte Unknown 2 -1 or 0
                    // byte Unknown 3 -1 or 0
                    // byte Unknown 4 -1 or 0
                    // byte Unknown 5  3
                    // byte Unknown 6  2
                    // byte Unknown 7  1
                    // byte Unknown 8  0
                    #endregion
                    mesh._vertices[i] = new Vertex();

                    mesh._vertices[i].VertX = ar.ReadSingle();
                    mesh._vertices[i].VertY = ar.ReadSingle();
                    mesh._vertices[i].VertZ = ar.ReadSingle();
                    if (mesh._version == MeshVersion.RB1) mesh._vertices[i].VertW = ar.ReadSingle();

                    if ((int)mesh._version < 35)
                    {
                        mesh._vertices[i].NormX = ar.ReadSingle();
                        mesh._vertices[i].NormY = ar.ReadSingle();
                        mesh._vertices[i].NormZ = ar.ReadSingle();
                        if (mesh._version == MeshVersion.RB1) mesh._vertices[i].NormW = ar.ReadSingle();
                    }
                    else
                    {
                        //ar.BaseStream.Position += 24;
                        mesh._vertices[i].ColorR = (float)ar.ReadByte();
                        mesh._vertices[i].ColorG = (float)ar.ReadByte();
                        mesh._vertices[i].ColorB = (float)ar.ReadByte();
                        mesh._vertices[i].ColorA = (float)ar.ReadByte();

                        /* Implement read half-precision
                        vertices[i].U = ar.ReadHalf();
                        vertices[i].V = ar.ReadHalf();
                        */
                        ar.BaseStream.Position += 20;

                        continue;
                        /*
                        _vertices[i].ColorR = 0.0f;
                        _vertices[i].ColorG = 0.0f;
                        _vertices[i].ColorB = 0.0f;
                        _vertices[i].ColorA = 0.0f;

                        _vertices[i].U = 0.0f;
                        _vertices[i].V = 0.0f
                         */
                    }

                    mesh._vertices[i].ColorR = ar.ReadSingle();
                    mesh._vertices[i].ColorG = ar.ReadSingle();
                    mesh._vertices[i].ColorB = ar.ReadSingle();
                    mesh._vertices[i].ColorA = ar.ReadSingle();

                    mesh._vertices[i].U = ar.ReadSingle();
                    mesh._vertices[i].V = ar.ReadSingle();

                    // Skips unknown bytes.
                    if (mesh._version == MeshVersion.RB1) ar.BaseStream.Position += 24;
                }

                // Reads face data
                mesh._faces = new ushort[ar.ReadUInt32()][];

                for (int i = 0; i < mesh._faces.Length; i++)
                {
                    mesh._faces[i] = new ushort[3];
                    mesh._faces[i][0] = ar.ReadUInt16();
                    mesh._faces[i][1] = ar.ReadUInt16();
                    mesh._faces[i][2] = ar.ReadUInt16();
                }
                
                byte[] groups = ar.ReadBytes(ar.ReadInt32()); // Sum should equal count of faces
                //if (groups.Sum(x => x) != mesh._faces.Length) ;

                string[] bones = new string[4];
                bones[0] = ar.ReadString();

                if (!string.IsNullOrEmpty(bones[0]))
                {
                    // Reads rest of strings
                    bones[1] = ar.ReadString();
                    bones[2] = ar.ReadString();
                    bones[3] = ar.ReadString();

                    // TODO: Read transform matrices
                    ar.BaseStream.Position += 192;
                }

                if (groups.Length == 0 || groups[0] == 0) return mesh;

                // TODO: Figure out why group data isn't embedded when group count > 0
                if (ar.BaseStream.Position == ar.BaseStream.Length) return mesh;

                foreach(byte group in groups)
                {
                    uint numberCount = ar.ReadUInt32();
                    uint indexCount = ar.ReadUInt32();

                    // TODO: Parse offsets
                    ar.BaseStream.Position += (numberCount << 2) + (indexCount << 1);
                }
            }

            return mesh;
        }

        private static bool DetermineEndianess(byte[] head, out MeshVersion version, out bool valid)
        {
            bool bigEndian = false;
            version = (MeshVersion)BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = (MeshVersion)BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(MeshVersion version)
        {
            switch (version)
            {
                case MeshVersion.GH1: // PS2 - GH1
                case MeshVersion.GH2:
                    return true;
                default:
                    return false;
            }
        }

        public MeshVersion Version { get { return _version; } }
        public Matrix Mat1 { get { return _mat1; } }
        public Matrix Mat2 { get { return _mat2; } }
        
        public string Material { get; set; }
        public Vertex[] Vertices { get { return _vertices; } }
        public ushort[][] Faces { get { return _faces; } }

        public List<string> Bones => _bones;
        public string Transform => _transform;
        public List<string> Meshes => _meshes;

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Mesh";
    }
}
