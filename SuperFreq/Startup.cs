using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Mackiloha.UI;

namespace SuperFreq
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationWindow, ApplicationWindow>();
            services.AddSingleton<BaseApp, SuperFreqApp>();
        }
    }
}
