using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mackiloha;
using Mackiloha.DTB;
using System.IO;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for DTBEditor.xaml
    /// </summary>
    public partial class DTBEditor : UserControl
    {
        private DTBFile _dtbFile;

        public DTBEditor()
        {
            InitializeComponent();
        }

        public void OpenDTBFile(Stream source, bool newStyleEncryption, DTBEncoding encoding)
        {
            if (source == null) return;

            // Copies stream
            MemoryStream ms = new MemoryStream();
            source.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            // Decrypts non-RBVR DTB files
            if (encoding != DTBEncoding.RBVR)
            {
                byte[] keyBytes = new byte[4];
                int key;

                // Reads key
                ms.Read(keyBytes, 0, 4);
                key = BitConverter.ToInt32(keyBytes, 0);

                // Decrypts dtb stream
                Crypt.DTBCrypt(ms, key, newStyleEncryption);
            }

            // Opens DTB file
            using (AwesomeReader ar = new AwesomeReader(ms))
            {
                _dtbFile = DTBFile.FromStream(ar, encoding);
            }

            TextBox_DTB.Text = _dtbFile.ToString();
        }
    }
}
