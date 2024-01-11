using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreq.Helpers;
using SuperFreq.Options;

namespace SuperFreq.Apps;
public class Texture2PngApp
{
    protected readonly ILogManager LogManager;

    public Texture2PngApp(ILogManager logManager)
    {
        LogManager = logManager;
    }

    public void Parse(Texture2PngOptions op)
    {
        LogManager.SetLogLevel(op.GetLogLevel());

        op.UpdateOptions();

        var appState = AppState.FromFile(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());

        var serializer = appState.GetSerializer();
        var bitmap = serializer.ReadFromFile<HMXBitmap>(op.InputPath);
        bitmap.SaveAs(appState.SystemInfo, op.OutputPath);

        Log.Information("Wrote image to \"{outputPath}\"", op.OutputPath);
    }
}
