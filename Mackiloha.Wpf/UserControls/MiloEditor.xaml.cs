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
using Mackiloha.IO;
using Mackiloha.Render;
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
        private MiloObject _selectedEntry;
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

        private MiloObject SelectedEntry
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
            if (_selectedEntry == null || !(_selectedEntry is MiloObjectBytes)) return;

            var miloEntry = _selectedEntry as MiloObjectBytes;
            

            switch (_selectedEntry.Type)
            {
                case "Tex":
                    try
                    {
                        var tex = Serializer.ReadFromMiloObjectBytes<Tex>(miloEntry);

                        if (!tex.UseExternal)
                        {
                            Image_TexPreview.Source = tex.Bitmap.ToBitmapSource(Serializer.Info);
                            return;
                        }

                        var pwd = System.IO.Path.GetDirectoryName(MiloPath);
                        var dir = System.IO.Path.GetDirectoryName(tex.ExternalPath);
                        var file = System.IO.Path.GetFileName(tex.ExternalPath);

                        var fullImagePath = System.IO.Path.Combine(pwd, dir, file);

                        // Look in gen folder if not found
                        if (!File.Exists(fullImagePath))
                            fullImagePath = System.IO.Path.Combine(pwd, dir, "gen", $"{file}_ps2");

                        var bitmap = Serializer.ReadFromFile<HMXBitmap>(fullImagePath);
                        Image_TexPreview.Source = bitmap.ToBitmapSource(Serializer.Info);
                    }
                    catch
                    {
                        Image_TexPreview.Source = null;
                    }

                    break;
            }

            /*
            switch(_selectedEntry)
            {
                case Tex tex:
                    try
                    {
                        //var tex = MiloOG.Tex.FromStream(new MemoryStream(_selectedEntry.Data));

                        // TODO: Fix
                        Image_TexPreview.Source = null;

                        //Image_TexPreview.Source = tex.Image.Image.ToBitmapSource();



                    }
                    catch
                    {
                        Image_TexPreview.Source = null;
                    }
                    break;
                default:
                    return;
            }*/
        }

        public MiloEditor()
        {
            InitializeComponent();
        }

        private void BuildTreeView()
        {
            TreeView_MiloTypes.Items.Clear();
            Image_TexPreview.Source = null;

            var root = new TreeViewItem()
            {
                Header = "Root",
                ContextMenu = this.Resources["ContextMenu_RootScene"] as ContextMenu
            };

            var types = Milo.Entries.Select(x => (string)x.Type).Distinct().OrderBy(x => x);

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

        private List<MiloObject> FilterEntriesByType(string type) => this.Milo.FilterByType(type);

        private List<MiloObject> FilteredMiloEntries => this.FilterEntriesByType(this.SelectedType);

        public MiloObjectDir Milo
        {
            get => this.Resources["Milo"] as MiloObjectDir;
            set
            {
                this.Resources["Milo"] = value;
                BuildTreeView();
            }
        }

        public MiloSerializer Serializer { get; set; }

        public string MiloPath { get; set; }

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
            if (!(context is MiloObject)) return;

            var entry = context as MiloObject;
            var type = ((string)entry.Type).ToUpper();
            var ext = entry.Extension();

            sfd.Title = $"Save {type} file";
            sfd.Filter = $"{type}|*{ext}";
            sfd.FileName = entry.Name;

            if (sfd.ShowDialog() == false) return;

            Serializer.WriteToFile(sfd.FileName, entry as ISerializable);
            MessageBox.Show($"Successfully saved {sfd.SafeFileName}");
        }
        
        private void MenuItem_MiloEntry_Replace_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as MenuItem).DataContext;
            if (!(context is MiloObject)) return;

            var entry = context as MiloObject;
            var type = ((string)entry.Type).ToUpper();
            var ext = entry.Extension();

            ofd.Title = $"Open {type} file";
            ofd.Filter = $"{type}|*{ext}";

            if (ofd.ShowDialog() == false) return;

            // Removes old entry
            Milo.Entries.Remove(entry);

            // Adds new entry
            var newEntry = new MiloObjectBytes(entry.Type) { Name = entry.Name };
            newEntry.Data = File.ReadAllBytes(ofd.FileName);
            Milo.Entries.Add(newEntry);
        }

        private void MenuItem_MiloEntry_Rename_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as MenuItem).DataContext;
            if (!(context is MiloObject)) return;

            var entry = context as MiloObject; // TODO: Implement?
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
            
            this.SelectedEntry = (sender as ListView).SelectedItem as MiloObject;
        }
        
        private void ContextMenu_MiloEntry_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as ContextMenu;
            if (menu == null) return;

            var entry = menu.DataContext as MiloObject;
            if (entry == null || !((string)entry.Type).Equals("Tex", StringComparison.CurrentCultureIgnoreCase)) return;

            var texItem = new MenuItem()
            {
                Header = "Tex Options"
            };

            var exportTexItem = new MenuItem() { Header = "Export as PNG" };
            exportTexItem.Click += ExportTexItem_Click;
            var importTexItem = new MenuItem() { Header = "Import as PNG" };
            importTexItem.Click += ImportTexItem_Click;
            
            texItem.Items.Add(exportTexItem);
            texItem.Items.Add(importTexItem);
            
            var idx = menu.Items.Add(texItem);
        }

        private void ExportTexItem_Click(object sender, RoutedEventArgs e)
        {
            var elm = sender as FrameworkElement;
            if (elm == null) return;

            var entry = elm.DataContext as MiloObject;
            if (entry == null || !((string)entry.Type).Equals("Tex", StringComparison.CurrentCultureIgnoreCase)) return;

            sfd.Title = $"Save PNG file";
            sfd.Filter = $"PNG|*.png";
            sfd.FileName = $"{System.IO.Path.GetFileNameWithoutExtension(entry.Name)}.png";

            if (sfd.ShowDialog() == false) return;
            
            try
            {
                /*
                // TODO: Re-implement this!
                var tex = MiloOG.Tex.FromStream(new MemoryStream((entry as MiloObjectBytes).Data));
                tex.Image.SaveAs(sfd.FileName);
                */

                // TODO: Merge code with bitmap previewer
                var tex = Serializer.ReadFromMiloObjectBytes<Tex>(entry as MiloObjectBytes);

                if (tex.UseExternal)
                {
                    var pwd = System.IO.Path.GetDirectoryName(MiloPath);
                    var dir = System.IO.Path.GetDirectoryName(tex.ExternalPath);
                    var file = System.IO.Path.GetFileName(tex.ExternalPath);

                    var fullImagePath = System.IO.Path.Combine(pwd, dir, file);

                    // Look in gen folder if not found
                    if (!File.Exists(fullImagePath))
                        fullImagePath = System.IO.Path.Combine(pwd, dir, "gen", $"{file}_ps2");

                    var bitmap = Serializer.ReadFromFile<HMXBitmap>(fullImagePath);
                    bitmap.SaveAs(Serializer.Info, sfd.FileName);
                }
                else
                {
                    tex.Bitmap.SaveAs(Serializer.Info, sfd.FileName);
                }
                
                MessageBox.Show($"Successfully saved {sfd.SafeFileName}");
            }
            catch
            {
                MessageBox.Show($"Oops. There were issues saving {sfd.SafeFileName}");
            }
        }

        private void ImportTexItem_Click(object sender, RoutedEventArgs e)
        {
            var elm = sender as FrameworkElement;
            if (elm == null) return;

            var entry = elm.DataContext as MiloObject;
            if (entry == null || !((string)entry.Type).Equals("Tex", StringComparison.CurrentCultureIgnoreCase)) return;
            
            ofd.Title = $"Open PNG file";
            ofd.Filter = $"PNG|*.png";

            if (ofd.ShowDialog() == false) return;

            // TODO: Clean this up A LOT!
            var tex = MiloOG.Tex.FromStream(new MemoryStream((entry as MiloObjectBytes).Data));
            tex.Image.ImportImageFromFile(ofd.FileName);
            var data = tex.WriteToBytes();

            // Removes old entry
            Milo.Entries.Remove(entry);

            // Adds new entry
            var newEntry = new MiloObjectBytes(entry.Type) { Name = entry.Name, Data = data };
            Milo.Entries.Add(newEntry);
            
            // Updates image
            SelectedEntryChanged();
        }
    }
}
