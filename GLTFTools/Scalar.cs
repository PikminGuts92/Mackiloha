using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTFTools
{
    public class Scalar<T> : GLPrimitive where T : IComparable<T>
    {
        public T Value;

        public Scalar() : this(default(T)) { }

        public Scalar(T value)
        {
            Value = value;
        }

        public static implicit operator Scalar<T>(T value) => new Scalar<T>(value);

        public static implicit operator T(Scalar<T> t) => t.Value;
    }
}
