using Serilog.Core;
using Serilog.Events;

namespace SuperFreq.Helpers;

public class LogManager : ILogManager
{
    protected readonly LoggingLevelSwitch _levelSwitch;

    public LogManager()
    {
        // Set default level
#if DEBUG
        var minLevel = LogEventLevel.Verbose;
#else
        var minLevel = LogEventLevel.Information;
#endif

        _levelSwitch = new LoggingLevelSwitch(minLevel);

        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_levelSwitch)
            .WriteTo.Console()
            .CreateLogger();
    }

    public void SetLogLevel(LogEventLevel level)
    {
        _levelSwitch.MinimumLevel = level;
        Log.Information("Using log level {level}", level);
    }
}
