namespace ArkHelper.Helpers;

public interface IScriptHelper
{
    string ConvertDtaToDtb(string dtaPath, string tempDir, bool newEncryption, int arkVersion);
    string ConvertDtbToDta(string dtbPath, string tempDir, bool newEncryption, int arkVersion, string dtaPath = null, int indentSize = 3);
    void UpdateTabIndention(string inputDta, string outputDta, int indentSize = 3);
}