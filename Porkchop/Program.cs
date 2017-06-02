using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mackiloha.Milo;

namespace Porkchop
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Porkchop v1");
            int argIdx = 0;
            
            while (argIdx < args.Length)
            {
                List<string> otherArgs;

                switch (args[argIdx].ToLower())
                {
                    case "-milo":
                    case "--milo":
                        otherArgs = GetArguments(args, ref argIdx);

                        if (otherArgs[0].Equals("serialize", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Opens input milo file
                            MiloFile milo = MiloFile.FromFile(otherArgs[1]);

                            // TODO: Write output json file
                        }

                        break;
                    default:
                        return;
                }
            }
        }

        static List<string> GetArguments(string[] args, ref int argIdx)
        {
            argIdx++;
            List<string> other = new List<string>();

            while (argIdx < args.Length && !(args[argIdx].StartsWith("-") || args[argIdx].StartsWith("--")))
            {
                other.Add(args[argIdx]);
                argIdx++;
            }

            return other;
        }

    }
}
