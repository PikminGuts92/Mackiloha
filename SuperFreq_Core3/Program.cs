using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Mackiloha.UI;
using Mackiloha.UI.Components;

namespace SuperFreq
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            Startup.ConfigureServices(services);
            
            services.BuildServiceProvider()
                .GetService<BaseApp>()
                .Window
                .Init("SuperFreq")
                .Run();
        }
    }
}
