using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.UI.Components
{
    public interface IFileDialog
    {
        string Title { get; set; }
        string Filter { get; set; }
        string FileName { get; set; }
        string[] Selection { get; }

        bool OpenFile();
        bool SaveFile();
    }
}
