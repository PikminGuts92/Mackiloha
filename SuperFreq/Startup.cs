using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Mackiloha.UI;
using Mackiloha.UI.Components;
using SuperFreq.Components;

namespace SuperFreq
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Components
            services.AddSingleton<IFileDialog, WinFileDialog>();
            services.AddSingleton<MainComponent>();

            services.AddSingleton<IApplicationWindow, ApplicationWindow>();
            services.AddSingleton<BaseApp, SuperFreqApp>();
        }
    }
}
