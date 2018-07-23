using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTFTools
{
    public class Vector2<T> : GLPrimitive where T : IComparable<T>
    {
        public T X;
        public T Y;

        public Vector2() : this(default(T)) { }

        public Vector2(T x) : this(x, x) { }

        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }
    }
}
