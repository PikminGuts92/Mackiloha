namespace P9SongTool;

public static class Constants
{
#if OS_WINDOWS
    internal const string APPLICATION_EXE_NAME = "p9songtool.exe";
#else
    internal const string APPLICATION_EXE_NAME = "p9songtool";
#endif
}
