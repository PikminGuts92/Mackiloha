using ArkHelper.Apps;
using ArkHelper.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace ArkHelper;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Helpers
        services.AddSingleton<ICacheHelper, CacheHelper>();
        services.AddSingleton<ILogManager, LogManager>();
        services.AddSingleton<IScriptHelper, ScriptHelperDtab>();

        // Apps
        services.AddSingleton<Ark2DirApp>();
        services.AddSingleton<ArkCompareApp>();
        services.AddSingleton<Dir2ArkApp>();
        services.AddSingleton<FixHdrApp>();
        services.AddSingleton<HashFinderApp>();
        services.AddSingleton<PatchCreatorApp>();
    }
}
