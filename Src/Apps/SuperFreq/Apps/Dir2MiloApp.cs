using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreq.Helpers;
using SuperFreq.Options;

namespace SuperFreq.Apps;

public class Dir2MiloApp
{
    protected readonly ILogManager LogManager;

    public Dir2MiloApp(ILogManager logManager)
    {
        LogManager = logManager;
    }

    public void Parse(Dir2MiloOptions op)
    {
        LogManager.SetLogLevel(op.GetLogLevel());

        op.UpdateOptions();
        op.VerifySupportedOptions();

        var appState = new AppState(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());
        appState.BuildMiloArchive(op.InputPath, op.OutputPath);

        Log.Information("Wrote milo to \"{outputPath}\"", op.OutputPath);
    }
}
