using System;
using System.Collections.Generic;
using System.Text;

namespace SuperFreqCLI.Exceptions
{
    internal class DTBParseException : Exception
    {
        public DTBParseException(string message) : base(message) { }
    }
}
