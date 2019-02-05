using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Mackiloha.UI;

namespace SuperFreq
{
    class Program
    {
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
