using Microsoft.Extensions.DependencyInjection;
using SuperFreq.Apps;
using SuperFreq.Helpers;

namespace SuperFreq;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Helpers
        services.AddSingleton<ILogManager, LogManager>();

        // Apps
        services.AddSingleton<Dir2MiloApp>();
        services.AddSingleton<Milo2DirApp>();
        services.AddSingleton<Png2TextureApp>();
        services.AddSingleton<Texture2PngApp>();
    }
}
