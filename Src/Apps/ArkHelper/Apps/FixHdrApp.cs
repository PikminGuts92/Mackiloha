using ArkHelper.Helpers;
using ArkHelper.Options;
using Mackiloha.Ark;

namespace ArkHelper.Apps;

public class FixHdrApp
{
    protected readonly ILogManager LogManager;

    public FixHdrApp(ILogManager logManager)
    {
        LogManager = logManager;
    }

    public void Parse(FixHdrOptions op)
    {
        LogManager.SetLogLevel(op.GetLogLevel());

        if (op.OutputPath is null)
            op.OutputPath = op.InputPath;

        var ark = ArkFile.FromFile(op.InputPath);
        ark.Encrypted = ark.Encrypted || op.ForceEncryption; // Force encryption
        ark.WriteHeader(op.OutputPath);
    }
}
