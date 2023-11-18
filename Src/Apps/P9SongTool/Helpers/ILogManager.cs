using Serilog.Events;

namespace P9SongTool.Helpers;

public interface ILogManager
{
    public void SetLogLevel(LogEventLevel level);
}
