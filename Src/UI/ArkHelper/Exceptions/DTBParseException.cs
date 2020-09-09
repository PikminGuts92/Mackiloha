using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHelper.Exceptions
{
    internal class DTBParseException : Exception
    {
        public DTBParseException(string message) : base(message) { }
    }
}
