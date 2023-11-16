namespace ArkHelper;

public static class Constants
{
#if OS_WINDOWS
    internal const string APPLICATION_EXE_NAME = "arkhelper.exe";
#else
    internal const string APPLICATION_EXE_NAME = "arkhelper";
#endif
}
