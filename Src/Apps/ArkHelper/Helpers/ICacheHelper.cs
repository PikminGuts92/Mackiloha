namespace ArkHelper.Helpers;

public interface ICacheHelper
{
    void LoadCache(string path, int arkVersion, bool arkEncrypted);
    void SaveCache();
    string GetCachedPathIfNotUpdated(string sourcePath, string internalPath);
    void UpdateCachedFile(string sourcePath, string internalPath, string genFilePath);
}
