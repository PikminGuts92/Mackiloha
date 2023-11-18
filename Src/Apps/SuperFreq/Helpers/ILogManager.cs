using Serilog.Events;

namespace SuperFreq.Helpers;

public interface ILogManager
{
    public void SetLogLevel(LogEventLevel level);
}
