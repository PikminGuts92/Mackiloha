using ArkHelper.Options;
using Mackiloha.Ark;

namespace ArkHelper.Apps;

public class FixHdrApp
{
    public void Parse(FixHdrOptions op)
    {
        if (op.OutputPath == null)
            op.OutputPath = op.InputPath;

        var ark = ArkFile.FromFile(op.InputPath);
        ark.Encrypted = ark.Encrypted || op.ForceEncryption; // Force encryption
        ark.WriteHeader(op.OutputPath);
    }
}
