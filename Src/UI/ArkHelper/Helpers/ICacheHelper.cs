using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHelper.Helpers
{
    public interface ICacheHelper
    {
        void LoadCache(string path, int arkVersion, bool arkEncrypted);
        string GetCachedPathIfNotUpdated(string realPath, string internalPath);
        void UpdateCachedFile(string realPath, string internalPath);
    }
}
