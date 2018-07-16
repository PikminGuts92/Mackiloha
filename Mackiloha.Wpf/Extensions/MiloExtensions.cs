using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mackiloha.Milo2;

namespace Mackiloha.Wpf.Extensions
{
    public static class MiloExtensions
    {
        public static int Size(this IMiloEntry entry) => entry is MiloEntry ? (entry as MiloEntry).Data.Length : -1;
    }
}
