using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public class Trans : AbstractEntry
    {
        private Matrix _mat1, _mat2;

        public Trans(string name, bool bigEndian = true) : base(name, "", bigEndian)
        {

        }

        public static Trans FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }
        public static Trans FromStream(Stream input)
        {
            using (AwesomeReader ar = new AwesomeReader(input))
            {
                int version;
                bool valid;
                Trans trans = new Trans("");

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
                if (!valid) return null; // Probably do something else later

                ar.BaseStream.Position += 9; // Should all be 0

                // Reads in matrix tables
                trans._mat1 = Matrix.FromStream(ar);
                trans._mat2 = Matrix.FromStream(ar);

                // TODO: Parse other stuff

                return trans;
            }
        }

        private static bool DetermineEndianess(byte[] head, out int version, out bool valid)
        {
            bool bigEndian = false;
            version = BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(int version)
        {
            switch (version)
            {
                case 9: // PS2 - GH2
                    return true;
                default:
                    return false;
            }
        }

        public Matrix Mat1 { get { return _mat1; } set { _mat1 = value; } }
        public Matrix Mat2 { get { return _mat2; } set { _mat2 = value; } }

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Trans";
    }
}
