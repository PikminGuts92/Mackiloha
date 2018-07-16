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
using Mackiloha.Milo2;
using Mackiloha.Wpf.Extensions;

namespace Mackiloha.Wpf.UserControls
{
    /// <summary>
    /// Interaction logic for MiloEditor.xaml
    /// </summary>
    public partial class MiloEditor : UserControl
    {
        private string _selectedType;

        public MiloEditor()
        {
            InitializeComponent();
        }

        private void BuildTreeView()
        {
            var milo = this.Resources["Milo"] as MiloFile;

            TreeView_MiloTypes.Items.Clear();

            var root = new TreeViewItem()
            {
                Header = "Root"
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

        private List<IMiloEntry> FilteredMiloEntries => this.FilterEntriesByType(this._selectedType);

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
                _selectedType = item.Header as string;
                
                this.ItemsControl_MiloEntries.ItemsSource = FilteredMiloEntries;
                this.ListView_MiloEntries.ItemsSource = FilteredMiloEntries;
            }
            else
                _selectedType = null;
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
    }
}
