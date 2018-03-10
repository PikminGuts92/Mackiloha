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
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.DTB;
using Mackiloha.Milo;
using System.IO;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Archive ark;

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1 && System.IO.File.Exists(args[1]))
            {
                OpenArchive(args[1]);
            }
        }

        private void OpenArchive(string path)
        {
            try
            {
                ark = Archive.FromFile(path);
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
            List<string> entries = ark.Entries.Select(x => x.FullPath).ToList(); // Recursive
            foreach (string entry in entries)
            {
                tn = root;
                string currentPath = "";
                string[] splitNames = entry.Split('/');
                //string[] splitNames = new string[] { };
                
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
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
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
                            itemOpen.Click += ItemOpen_Click;

                            node.MouseDoubleClick += Node_MouseDoubleClick;
                            node.ContextMenu.Items.Insert(0, itemOpen);
                            break;
                        }
                    case ArkEntryType.Texture:
                    case ArkEntryType.Audio:
                        break;
                    case ArkEntryType.Archive:
                        {
                            MenuItem itemOpen = new MenuItem();
                            itemOpen.Header = "Open";
                            itemOpen.Click += ItemOpen_Click;

                            node.MouseDoubleClick += Node_MouseDoubleClick;
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

        private void Node_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedFile();
        }

        private void ItemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedFile();
        }

        private void OpenSelectedFile()
        {
            if (TreeView_Archive.SelectedItem == null || !(TreeView_Archive.SelectedItem is TreeViewItem)) return;

            TreeViewItem item = TreeView_Archive.SelectedItem as TreeViewItem;
            if (item.Tag == null || !(item.Tag is TreeArkEntryInfo)) return;

            TreeArkEntryInfo info = item.Tag as TreeArkEntryInfo;

            int selectedIdx;

            switch (info.EntryType)
            {
                case ArkEntryType.Script:
                    
                    // Open DTB file
                    TabItem dtbTab = TabControl_Files.Resources["TabItem_DTB"] as TabItem;
                    dtbTab.Header = GetFileName(info.InternalPath);
                    DTBEditor dtbEdit = dtbTab.Content as DTBEditor;

                    // Gets entry from ark
                    Stream dtbStream = ark.GetArkEntryFileStream(ark[info.InternalPath]);
                    
                    try
                    {
                        dtbEdit.OpenDTBFile(dtbStream, false, DTBEncoding.Classic);
                        selectedIdx = TabControl_Files.Items.Add(dtbTab);

                        dtbStream.Close();
                    }
                    catch (Exception ex)
                    {
                        dtbStream.Close();
                        return;
                    }
                    break;
                case ArkEntryType.Texture:
                case ArkEntryType.Audio:
                    return;
                case ArkEntryType.Archive:
                    
                    // Opens Milo file
                    TabItem miloTab = TabControl_Files.Resources["TabItem_Milo"] as TabItem;
                    miloTab.Header = GetFileName(info.InternalPath);
                    MiloEditor miloEdit = miloTab.Content as MiloEditor;
                    miloEdit.SetArk(this.ark);
                    miloEdit.SetFilePath(info.InternalPath);

                    // Gets entry from ark
                    Stream miloStream = ark.GetArkEntryFileStream(ark[info.InternalPath]);

                    miloEdit.OpenMiloFile(miloStream);
                    selectedIdx = TabControl_Files.Items.Add(miloTab);
                    break;
                case ArkEntryType.Video:
                case ArkEntryType.Midi:
                default: // ArkFileType.Default
                    return;
            }

            // Changes selected tab to most recently added
            TabControl_Files.Visibility = Visibility.Visible;
            TabControl_Files.SelectedIndex = selectedIdx;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ExtractFile_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView_Archive.SelectedItem == null || !(TreeView_Archive.SelectedItem is TreeViewItem)) return;

            TreeViewItem item = TreeView_Archive.SelectedItem as TreeViewItem;
            if (item.Tag == null || !(item.Tag is TreeArkEntryInfo)) return;

            TreeArkEntryInfo info = item.Tag as TreeArkEntryInfo;

            var entry = ark.Entries.FirstOrDefault(x => x.FullPath == info.InternalPath);
            
            sfd.Filter = $"{entry.Extension.ToUpper()}|*.{entry.Extension}";
            sfd.FileName = entry.FileName;
            if (sfd.ShowDialog() == false) return;

            var stream = ark.GetArkEntryFileStream(entry);

            // Copies stream to file
            using (FileStream fs = File.OpenWrite(sfd.FileName))
            {
                stream.CopyTo(fs);
            }
        }

        private void ReplaceFile_Click(object sender, RoutedEventArgs e)
        {
            if (TreeView_Archive.SelectedItem == null || !(TreeView_Archive.SelectedItem is TreeViewItem)) return;

            TreeViewItem item = TreeView_Archive.SelectedItem as TreeViewItem;
            if (item.Tag == null || !(item.Tag is TreeArkEntryInfo)) return;

            TreeArkEntryInfo info = item.Tag as TreeArkEntryInfo;

            var entry = ark.Entries.FirstOrDefault(x => x.FullPath == info.InternalPath);

            ofd.Filter = $"{entry.Extension.ToUpper()}|*.{entry.Extension}";
            if (ofd.ShowDialog() == false) return;

            var pending = new PendingArkEntry(entry.FileName, entry.Directory)
            {
                LocalFilePath = ofd.FileName
            };

            ark.AddPendingEntry(pending);
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

        private void Menu_Tools_Options_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        SaveFileDialog sfd = new SaveFileDialog();
        private void Menu_ExportHeader_Click(object sender, RoutedEventArgs e)
        {
            sfd.Filter = "HDR|*.hdr";
            sfd.FileName = ark.FileName;
            if (sfd.ShowDialog() == false) return;

            ark.WriteHeader(sfd.FileName);
        }

        private void Menu_SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            ark.CommitChanges();
        }
    }
}
