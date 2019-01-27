using System;
using System.Linq;
using System.Text;
using Mackiloha.UI;

namespace SuperFreq
{
    class Program
    {
        static void Main(string[] args)
        {
            var window = new ApplicationWindow("SuperFreq");
            var app = new SuperFreqApp(window);
            window.Run();
        }
    }
}
