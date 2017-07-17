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
using Microsoft.Win32; // OpenFileDialog
using System.ComponentModel; // SortDescription
using static System.IO.Path;
using GameArchives;
using GameArchives.Ark;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArkPackage ark;

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1 && System.IO.File.Exists(args[1]))
            {
                OpenArchive(args[1]);
            }
        }

        private async void OpenArchive(string path)
        {
            try
            {
                AbstractPackage pack = await Task.Run(() => PackageReader.ReadPackageFromFile(path));

                if (pack is ArkPackage)
                    ark = pack as ArkPackage;
                else
                    ark = null;
            }
            catch
            {
                ark = null;
            }
            finally
            {
                RefreshFileTree();
            }
        }

        private void UnregisterNode(TreeViewItem parent)
        {
            foreach (TreeViewItem child in parent.Items)
            {
                UnregisterNode(child);
            }

            // Unregisters name
            TreeView_Archive.UnregisterName(parent.Name);
        }

        private void SortNode(TreeViewItem parent)
        {
            foreach (TreeViewItem child in parent.Items)
            {
                SortNode(child);
            }

            // Sorts nodes
            parent.Items.SortDescriptions.Clear();
            parent.Items.SortDescriptions.Add(new SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void RefreshFileTree()
        {
            // Unregisters nodes
            foreach (TreeViewItem node in TreeView_Archive.Items)
                UnregisterNode(node);

            TreeView_Archive.Items.Clear();

            if (ark == null) return;

            TreeViewItem root = new TreeViewItem();
            root.Header = GetFileNameWithoutExtension(ark.FileName);
            root.Tag = ark;
            root.Name = "_";
            TreeView_Archive.RegisterName("_", root);
            //root.ContextMenu = TreeView_Archive.Resources["CM_Directory"] as ContextMenu;

            TreeViewItem tn = root;
            foreach (List<IFile> entry in ark.RootDirectory.Files)
            {
                tn = root;
                string currentPath = "";
                //string[] splitNames = entry.Name.Split('/');
                string[] splitNames = new string[] { };
                
                for (int i = 0; i < splitNames.Length; i++)
                {
                    currentPath += splitNames[i];
                    if (i == (splitNames.Length - 1))
                        // File entry
                        tn = AddNode(tn, currentPath, splitNames[i], false);
                    else
                    {
                        // Folder entry
                        currentPath += "/";
                        tn = AddNode(tn, currentPath, splitNames[i], true);
                    }
                }
            }

            root.Items.SortDescriptions.Clear();
            root.Items.SortDescriptions.Add(new SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
            //root.Items.Refresh();

            // Sorts nodes
            foreach (TreeViewItem node in root.Items)
                SortNode(node);

            TreeView_Archive.Items.Add(root);
        }

        private string CreateKey(string path, bool folder)
        {
            if (path == "") return "_";

            List<char> newKey = new List<char>();

            newKey.Add((folder) ? 'a' : 'b'); // So folders sort first

            foreach (char c in path)
            {
                if ((c >= '0' && c <= 9) || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    newKey.Add(c);
                else if (c == '.')
                {
                    // So that periods sort before slashes
                    newKey.Add('_');
                    newKey.Add('a');
                }
                else if (c == '_')
                {
                    newKey.Add('_');
                    newKey.Add('b');
                }
                else
                    newKey.Add('_');
            }

            // Returns unique key
            return string.Concat(newKey);
        }

        private TreeViewItem AddNode(TreeViewItem parent, string currentPath, string text, bool folder)
        {
            //node.Items.Cast<TreeViewItem>().
            string key = CreateKey(currentPath, folder);
            object needle = TreeView_Archive.FindName(key);

            if (needle != null)
                return needle as TreeViewItem;
            else
            {
                //TreeFileEntry temp = new TreeFileEntry();
                TreeViewItem child = new TreeViewItem();
                child.Header = text;
                child.Name = key;
                //temp.Path = currentPath;
                TreeView_Archive.RegisterName(key, child);

                int returnIdx;
                returnIdx = parent.Items.Add(child);
                child.Tag = new TreeArkEntryInfo(currentPath, folder, key);
                SetNodeProperties(child);


                return parent.Items[returnIdx] as TreeViewItem;
            }
        }

        private void SetNodeProperties(TreeViewItem node)
        {
            if (node.Tag == null) return;

            if (node.Tag is TreeArkEntryInfo)
            {
                TreeArkEntryInfo info = node.Tag as TreeArkEntryInfo;

                if (info.EntryType == ArkEntryType.Folder)
                {
                    node.ContextMenu = TreeView_Archive.Resources["CM_Directory"] as ContextMenu;
                    return;
                }
                else
                    node.ContextMenu = TreeView_Archive.Resources["CM_File"] as ContextMenu;

                switch (info.EntryType)
                {
                    case ArkEntryType.Script:
                        {
                            MenuItem itemOpen = new MenuItem();
                            itemOpen.Header = "Open";
                            //itemOpen.Click += ItemOpen_Click;

                            //node.MouseDoubleClick += Node_MouseDoubleClick;
                            //node.ContextMenu.Items.Insert(0, itemOpen);
                            break;
                        }
                    case ArkEntryType.Texture:
                    case ArkEntryType.Audio:
                        break;
                    case ArkEntryType.Archive:
                        {
                            MenuItem itemOpen = new MenuItem();
                            itemOpen.Header = "Open";
                            //itemOpen.Click += ItemOpen_Click;

                            //node.MouseDoubleClick += Node_MouseDoubleClick;
                            node.ContextMenu.Items.Insert(0, itemOpen);
                            break;
                        }
                    case ArkEntryType.Video:
                    case ArkEntryType.Midi:
                    default: // ArkFileType.Default
                        break;
                }

            }
            //else if (node.Tag is ArkFile)
            //{
            //    node.ContextMenu = TreeView_Archive.Resources["CM_Directory"] as ContextMenu;
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ExtractFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExtractDir_Click(object sender, RoutedEventArgs e)
        {

        }

        OpenFileDialog ofd = new OpenFileDialog();
        private void Menu_File_Open_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "HDR|*.hdr";
            if (ofd.ShowDialog() == false) return;

            OpenArchive(ofd.FileName);
        }

        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
