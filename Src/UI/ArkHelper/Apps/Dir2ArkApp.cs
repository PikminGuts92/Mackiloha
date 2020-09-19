using ArkHelper.Exceptions;
using ArkHelper.Helpers;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.CSV;
using Mackiloha.Milo2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkHelper.Apps
{
    public class Dir2ArkApp
    {
        protected readonly IScriptHelper ScriptHelper;

        public Dir2ArkApp(IScriptHelper scriptHelper)
        {
            ScriptHelper = scriptHelper;
        }

        public void Parse(Dir2ArkOptions op)
        {

        }
    }
}
