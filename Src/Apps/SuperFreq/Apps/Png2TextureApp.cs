using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreq.Helpers;
using SuperFreq.Options;

namespace SuperFreq.Apps;

public class Png2TextureApp
{
    protected readonly ILogManager LogManager;

    public Png2TextureApp(ILogManager logManager)
    {
        LogManager = logManager;
    }

    public void Parse(Png2TextureOptions op)
    {
        LogManager.SetLogLevel(op.GetLogLevel());

        op.UpdateOptions();

        var appState = AppState.FromFile(op.InputPath);
        appState.UpdateSystemInfo(op.GetSystemInfo());

        var bitmap = TextureExtensions.BitmapFromImage(op.InputPath, appState.SystemInfo);
        var serializer = appState.GetSerializer();
        serializer.WriteToFile(op.OutputPath, bitmap);

        Log.Information("Wrote image to \"{outputPath}\"", op.OutputPath);
    }
}
