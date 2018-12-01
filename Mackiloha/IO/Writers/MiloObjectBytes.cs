using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private void WriteToStream(AwesomeWriter aw, MiloObjectBytes bytes)
        {
            if (bytes.Data == null)
                return;

            aw.Write(bytes.Data);
        }
    }
}
