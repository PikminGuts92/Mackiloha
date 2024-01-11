using Serilog.Events;

namespace ArkHelper.Helpers;

public interface ILogManager
{
    public void SetLogLevel(LogEventLevel level);
}
