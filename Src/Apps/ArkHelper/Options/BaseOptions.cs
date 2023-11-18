using CommandLine;
using Serilog.Events;

namespace ArkHelper.Options;

public abstract class BaseOptions
{
#if DEBUG
    protected const string DEFAULT_LOG_LEVEL = "all";
#else
    protected const string DEFAULT_LOG_LEVEL = "info";
#endif

    [Option('l', "logLevel", Default = DEFAULT_LOG_LEVEL, HelpText = "Log level (all, info, error, none)")]
    public string LogLevel { get; set; }

    public LogEventLevel GetLogLevel() => ResolveLogLevel(LogLevel);

    private LogEventLevel ResolveLogLevel(string level)
    {
        return level?.ToLower() switch
        {
            "all" => LogEventLevel.Verbose,
            "info" => LogEventLevel.Information,
            "error" => LogEventLevel.Error,
            "none" => LogEventLevel.Fatal,
            _ => GetDefaultLogLevel(level),
        };
    }

    private LogEventLevel GetDefaultLogLevel(string level)
    {
        Log.Warning("Unable to resolve log level for \"{level}\". Using default \"{defaultLevel}\" instead.", level, DEFAULT_LOG_LEVEL);

        return ResolveLogLevel(DEFAULT_LOG_LEVEL);
    }
}
