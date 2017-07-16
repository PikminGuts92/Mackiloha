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

        public float[,] GetRawMatrix => new float[,]
        {
            { RX, RY, RZ, RW },
            { UX, UY, UZ, UW },
            { FX, FY, FZ, FW },
            { PX, PY, PZ, PW },
        };

        public static Matrix FromStream(AwesomeReader ar)
        {
            // Reads from stream, usually embedded inside milo directories and mesh files
            Matrix mat = new Matrix();

            mat.RX = ar.ReadSingle(); // M11
            mat.RY = ar.ReadSingle(); // M12
            mat.RZ = ar.ReadSingle(); // M13

            mat.UX = ar.ReadSingle(); // M21
            mat.UY = ar.ReadSingle(); // M22
            mat.UZ = ar.ReadSingle(); // M23

            mat.FX = ar.ReadSingle(); // M31
            mat.FY = ar.ReadSingle(); // M32
            mat.FZ = ar.ReadSingle(); // M33

            mat.PX = ar.ReadSingle(); // M41
            mat.PY = ar.ReadSingle(); // M42
            mat.PZ = ar.ReadSingle(); // M43
            mat.PW = 1.0f;            // M44 - Implicit

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

        public float FX; // Forward
        public float FY;
        public float FZ;
        public float FW;

        public float PX; // Position
        public float PY;
        public float PZ;
        public float PW;
    }
}
