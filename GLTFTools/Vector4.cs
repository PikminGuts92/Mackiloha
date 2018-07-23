using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTFTools
{
    public class Vector4<T> : GLPrimitive where T : IComparable<T>
    {
        public T X;
        public T Y;
        public T Z;
        public T W;

        public Vector4() : this(default(T)) { }

        public Vector4(T x) : this(x, x, x, x) { }
        
        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
