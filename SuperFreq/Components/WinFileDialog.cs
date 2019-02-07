using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using Mackiloha.UI.Components;

namespace SuperFreq.Components
{
    public class WinFileDialog : IFileDialog
    {
        private readonly OpenFileDialog OFD;
        private readonly SaveFileDialog SFD;

        public string Title { get => OFD.Title; set { OFD.Title = value; SFD.Title = value; } }
        public string Filter { get => OFD.Filter; set { OFD.Filter = value; SFD.Filter = value; } }
        public string FileName { get => OFD.FileName; set { OFD.FileName = value; SFD.FileName = value; } }
        public string[] Selection { get; private set; }

        public WinFileDialog()
        {
            OFD = new OpenFileDialog();
            SFD = new SaveFileDialog();
            Selection = Array.Empty<string>();
        }
        
        public bool OpenFile()
        {
            if (OFD.ShowDialog() ?? false)
            {
                Selection = OFD.FileNames;
            }
            else
            {
                Selection = Array.Empty<string>();
            }

            return true;
        }

        public bool SaveFile()
        {
            if (SFD.ShowDialog() ?? false)
            {
                Selection = SFD.FileNames;
            }
            else
            {
                Selection = Array.Empty<string>();
            }

            return true;
        }
    }
}
