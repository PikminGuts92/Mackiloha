using System;
using System.Collections.Generic;
using System.Globalization;
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
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.App;
using Mackiloha.App.Extensions;

namespace SuperFreq
{
    public class Node
    {
        public Node()
        {

        }

        public Node(string name)
        {
            Name = name;
        }

        public List<Node> Children { get; set; } = new List<Node>();
        public string Name { get; set; }
    }

    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // FolderSolid, FolderOpenSolid, FileAltSolid
            var node = value as Node;

            return (node == null || node.Children.Count > 0) ? "FolderSolid" : "FileAltSolid";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var root = new Node()
            {
                Name = "Root",
                Children = new List<Node>()
                {
                    new Node("Entry 1"),
                    new Node("Entry 2")
                    {
                        Children = new List<Node>()
                        {
                            new Node("Sub Entry a"),
                            new Node("Sub Entry b"),
                            new Node("Sub Entry c")
                        }
                    },
                    new Node("Entry 3"),
                }
            };

            var rooObj = new Node() { Children = new List<Node>() { root } };

            TreeView_MiloEntries.DataContext = rooObj;
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
    }
}
