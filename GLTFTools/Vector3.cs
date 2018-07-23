using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTFTools
{
    public class Vector3<T> : GLPrimitive where T : IComparable<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3() : this(default(T)) { }

        public Vector3(T x) : this(x, x, x) { }
        
        public Vector3(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
