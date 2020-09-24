using System;
using System.Collections.Generic;
using System.Text;

namespace P9SongTool.Exceptions
{
    public class UnsupportedMiloException : Exception
    {
        public UnsupportedMiloException(): this("Milo is unsupported")
        {

        }

        public UnsupportedMiloException(string message) : base(message)
        {

        }
    }
}
