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
using System.Windows.Shapes;
using Mackiloha.Milo2;
using MiloOG = Mackiloha.Milo;
using Mackiloha.Wpf.Extensions;
using GLTFTools;
using ImageMagick;

namespace Mackiloha.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for MiloEditor.xaml
    /// </summary>
    public partial class MiloEditor : UserControl
    {
        private string _selectedType;
        private MiloEntry _selectedEntry;
        private OpenFileDialog ofd = new OpenFileDialog();
        private SaveFileDialog sfd = new SaveFileDialog();
        
        private string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                SelectedTypeChanged();
            }
        }

        private void SelectedTypeChanged()
        {
            this.ItemsControl_MiloEntries.ItemsSource = FilteredMiloEntries;
            this.ListView_MiloEntries.ItemsSource = FilteredMiloEntries;
        }

        private MiloEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                SelectedEntryChanged();
            }
        }

        private void SelectedEntryChanged()
        {
            Image_TexPreview.Source = null;
            if (_selectedEntry == null) return;
            
            try
            {
                var tex = MiloOG.Tex.FromStream(new MemoryStream(_selectedEntry.Data));
                Image_TexPreview.Source = tex.Image.Image.ToBitmapSource();
                
            }
            catch
            {
                Image_TexPreview.Source = null;
            }
        }

        public MiloEditor()
        {
            InitializeComponent();
        }

        private void BuildTreeView()
        {
            var milo = this.Resources["Milo"] as MiloFile;

            TreeView_MiloTypes.Items.Clear();
            Image_TexPreview.Source = null;

            var root = new TreeViewItem()
            {
                Header = "Root",
                ContextMenu = this.Resources["ContextMenu_RootScene"] as ContextMenu
            };

            var types = milo.Entries.Select(x => x.Type).Distinct().OrderBy(x => x);

            foreach (var type in types)
            {
                var itemType = new TreeViewItem()
                {
                    Header = type
                };

                root.Items.Add(itemType);
            }
            
            TreeView_MiloTypes.Items.Add(root);
        }

        private List<IMiloEntry> FilterEntriesByType(string type) =>
            this.Milo.Entries.Where(x => x.Type.Equals(type, StringComparison.CurrentCultureIgnoreCase)).OrderBy(x => x.Name).ToList();

        private List<IMiloEntry> FilteredMiloEntries => this.FilterEntriesByType(this.SelectedType);

        public MiloFile Milo
        {
            get => this.Resources["Milo"] as MiloFile;
            set
            {
                this.Resources["Milo"] = value;
                BuildTreeView();
            }
        }

        private void TreeView_MiloTypes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem)
            {
                var item = e.NewValue as TreeViewItem;
                SelectedType = item.Header as string;
            }
            else
            {
                Image_TexPreview.Source = null;
                SelectedType = null;
            }
        }
        
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var radioBtn = sender as RadioButton;
            if (radioBtn == null) return;

            RadioButton_Detailed.IsChecked = radioBtn.Name == "RadioButton_Detailed" && (radioBtn.IsChecked ?? false);
            RadioButton_Image.IsChecked = radioBtn.Name == "RadioButton_Image" && (radioBtn.IsChecked ?? false);
            
            this.ListView_MiloEntries.Visibility = RadioButton_Detailed.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
            this.ItemsControl_MiloEntries.Visibility = RadioButton_Image.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
        }

        private void MenuItem_MiloEntry_Extract_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as MenuItem).DataContext;
            if (!(context is MiloEntry)) return;

            var entry = context as MiloEntry;
            var type = entry.Type.ToUpper();
            var ext = entry.Extension();

            sfd.Title = $"Save {type} file";
            sfd.Filter = $"{type}|*{ext}";
            sfd.FileName = entry.Name;

            if (sfd.ShowDialog() == false) return;

            File.WriteAllBytes(sfd.FileName, entry.Data);
            MessageBox.Show($"Successfully saved {sfd.SafeFileName}");
        }
        
        private void MenuItem_MiloEntry_Replace_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as MenuItem).DataContext;
            if (!(context is MiloEntry)) return;

            var entry = context as MiloEntry;
            var type = entry.Type.ToUpper();
            var ext = entry.Extension();

            ofd.Title = $"Open {type} file";
            ofd.Filter = $"{type}|*{ext}";

            if (ofd.ShowDialog() == false) return;

            var data = File.ReadAllBytes(ofd.FileName);
            entry.Data = data;
        }

        private void MenuItem_MiloEntry_Rename_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as MenuItem).DataContext;
            if (!(context is MiloEntry)) return;

            var entry = context as MiloEntry;
        }

        private void MenuItem_MiloEntry_Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_RootScene_Import_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Get milo from sender item
            var milo = this.Milo;

            ofd.Title = $"Open GLTF file";
            ofd.Filter = $"GLTF|*.gltf";

            if (ofd.ShowDialog() == false) return;

            var scene = GLTF.FromFile(ofd.FileName);
            var json = scene.ToJson();
        }

        private void MenuItem_RootScene_Extract_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Get milo from sender item
            var milo = this.Milo;

            sfd.Title = $"Save GLTF file";
            sfd.Filter = $"GLTF|*.gltf";
            sfd.FileName = "output.gltf";

            if (sfd.ShowDialog() == false) return;

            milo.ExportToGLTF(sfd.FileName);
            //milo.WriteTree(sfd.FileName);
            //milo.WriteTree2(sfd.FileName);
            MessageBox.Show($"Successfully saved {sfd.SafeFileName}");
        }

        private void ListView_MiloEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView))
            {
                Image_TexPreview.Source = null;
                return;
            }
            
            this.SelectedEntry = (sender as ListView).SelectedItem as MiloEntry;
        }
    }
}
