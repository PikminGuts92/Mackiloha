using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha
{
    public interface IExportable
    {
        /// <summary>
        /// Reads object from JSON file
        /// </summary>
        /// <param name="path"></param>
        void Import(string path);

        /// <summary>
        /// Writes object to JSON file
        /// </summary>
        /// <param name="path"></param>
        void Export(string path);
    }
}
