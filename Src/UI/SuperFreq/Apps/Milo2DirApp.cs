using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreq.Helpers;
using SuperFreq.Options;

namespace SuperFreq.Apps;

public class Milo2DirApp
{
    protected readonly ILogManager LogManager;

    public Milo2DirApp(ILogManager logManager)
    {
        LogManager = logManager;
    }

    public void Parse(Milo2DirOptions op)
    {
        LogManager.SetLogLevel(op.GetLogLevel());

        op.UpdateOptions();
        op.VerifySupportedOptions();

        var appState = AppState.FromFile(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());
        appState.ExtractMiloContents(op.InputPath, op.OutputPath, op.ConvertTextures);

        Log.Information("Extracted milo contents to \"{outputPath}\"", op.OutputPath);
    }
}
