using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanicAttack;

namespace Mackiloha
{
    public struct Matrix
    {
        /*             (Column Major Order)
         *  |       X       Y       Z       W
         *  |       1       0       0       0   Right
         *  |       0       1       0       0   Up
         *  |       0       0       1       0   Forward
         *  |       0       0       0       1   Position
         */
        
        public static Matrix Identity()
        {
            return new Matrix()
            {
                RX = 1.0f,
                UY = 1.0f,
                FZ = 1.0f,
                PW = 1.0f
            };
        }

        public static Matrix FromStream(AwesomeReader ar)
        {
            // Reads from stream, usually embedded inside milo directories and mesh files
            Matrix mat = new Matrix();

            mat.RX = ar.ReadSingle();
            mat.RY = ar.ReadSingle();
            mat.RZ = ar.ReadSingle();

            mat.UX = ar.ReadSingle();
            mat.UY = ar.ReadSingle();
            mat.UZ = ar.ReadSingle();

            mat.FX = ar.ReadSingle();
            mat.FY = ar.ReadSingle();
            mat.FZ = ar.ReadSingle();

            mat.PX = ar.ReadSingle();
            mat.PY = ar.ReadSingle();
            mat.PZ = ar.ReadSingle();

            return mat;
        }
        
        public float RX; // Right
        public float RY;
        public float RZ;
        public float RW;

        public float UX; // Up
        public float UY;
        public float UZ;
        public float UW;

        public float FX; // Front
        public float FY;
        public float FZ;
        public float FW;

        public float PX; // Position
        public float PY;
        public float PZ;
        public float PW;
    }
}
