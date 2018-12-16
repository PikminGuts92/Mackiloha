using Microsoft.Win32; // OpenFileDialog
using System;
using System.Collections.Generic;
using System.IO;
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
//using System.Windows.Shapes;
using Mackiloha;
using Mackiloha.IO;
using Mackiloha.Milo2;

namespace MiloStardust
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog ofd = new OpenFileDialog();
        SaveFileDialog sfd = new SaveFileDialog();

        string miloPath;

        public MainWindow()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();

            if (args != null && args.Length > 1)
            {
                OpenMilo(args[1]);
            }
        }

        private void OpenMilo(string path)
        {
            var mf = MiloFile.ReadFromFile(path);
            var serializer = new MiloSerializer(new SystemInfo()
            {
                BigEndian = mf.BigEndian, Version = mf.Version,
                // TODO: Implement full file path parsing for paltform
                Platform = path.EndsWith("xbox", StringComparison.CurrentCultureIgnoreCase)
                    ? Platform.X360 :
                    Platform.PS2
            });
            miloPath = path;
            Milo_Editor.MiloPath = path;

            // TODO: Add try-catch block
            using (var ms = new MemoryStream(mf.Data))
            {
                Milo_Editor.Milo = serializer.ReadFromStream<MiloObjectDir>(ms);
                Milo_Editor.Serializer = serializer;
            }
        }

        private void SaveMilo(string path)
        {
            var mf = new MiloFile();

            using (var ms = new MemoryStream())
            {
                Milo_Editor.Serializer.WriteToStream(ms, Milo_Editor.Milo);
                ms.Seek(0, SeekOrigin.Begin);
                mf.Data = ms.ToArray();
            }

            // TODO: Finish implementing saving
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Hides the stupid overflow arrow
            // Source: http://stackoverflow.com/questions/4662428/how-to-hide-arrow-on-right-side-of-a-toolbar

            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void Menu_File_Open_Click(object sender, RoutedEventArgs e)
        {
            ofd.Title = "Open MILO file";
            ofd.Filter = "MILO|*.milo_ps2;*.milo_ps3;*.milo_wii;*.milo_xbox;*.rnd_gc;*.rnd_ps2;*.rnd_xbox;*.gh";

            if (ofd.ShowDialog() == false) return;

            OpenMilo(ofd.FileName);
        }

        private void Menu_File_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var ext = Path.GetExtension(miloPath);

            sfd.Title = $"Save MILO file";
            sfd.Filter = $"{ext.Remove(0, 1).ToUpper()}|*{ext}";
            sfd.FileName = Path.GetFileName(miloPath);

            if (sfd.ShowDialog() == false) return;
            
            SaveMilo(sfd.FileName);
            MessageBox.Show($"Successfully saved {sfd.SafeFileName}");
        }

        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Menu_Tools_Options_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Menu_Help_About_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            var firstFile = files.First();

            OpenMilo(firstFile);
        }
    }
}
