using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Models
{
    public class NameCollection<T1, T2>
    {
        public T1 Name { get; set; }
        public List<T2> Values { get; set; } = new List<T2>();
    }
}
