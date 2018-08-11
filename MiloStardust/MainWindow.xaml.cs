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
                Milo_Editor.Milo = MiloFile.ReadFromFile(args[1]);
                miloPath = args[1];
            }
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
            ofd.Filter = "MILO|*.milo_ps2;*.milo_ps3;*.milo_xbox";

            if (ofd.ShowDialog() == false) return;

            Milo_Editor.Milo = MiloFile.ReadFromFile(ofd.FileName);
            miloPath = ofd.FileName;
        }

        private void Menu_File_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            var ext = Path.GetExtension(miloPath);

            sfd.Title = $"Save MILO file";
            sfd.Filter = $"{ext.Remove(0, 1).ToUpper()}|*{ext}";
            sfd.FileName = Path.GetFileName(miloPath);

            if (sfd.ShowDialog() == false) return;

            Milo_Editor.Milo.WriteToFile(sfd.FileName);
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

            Milo_Editor.Milo = MiloFile.ReadFromFile(firstFile);
            miloPath = firstFile;
        }
    }
}
