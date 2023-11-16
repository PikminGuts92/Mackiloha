namespace SuperFreq;

public static class Constants
{
#if OS_WINDOWS
    internal const string APPLICATION_EXE_NAME = "superfreq.exe";
#else
    internal const string APPLICATION_EXE_NAME = "superfreq";
#endif
}
