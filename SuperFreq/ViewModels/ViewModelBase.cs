using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;
using Mackiloha.Ark;
using Avalonia.Controls;


namespace SuperFreq.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private Archive _ark;

        public ViewModelBase()
        {
            this.WhenAnyValue(x => x.Archive)
                .Subscribe(y =>
                {
                    Archive ark = y;

                    TreeViewItem root = new TreeViewItem();
                    root.Header = Path.GetFileNameWithoutExtension(ark?.FileName);
                    root.Classes.Add("root");

                    root.Tag = ark;
                    root.Name = "_";
                    this.Root = root;

                    if (ark == null)
                        return;

                    ProcessDirectories(ark.Entries, "", this.Root);
                });
            
            /*
            RootObservable = this.WhenAnyValue(x => x.Archive)
                .AsObservable<Archive>()
                .Select<Archive, TreeViewItem>(x =>
                {
                    TreeViewItem root = new TreeViewItem();
                    root.Header = Path.GetFileNameWithoutExtension(x.FileName);
                    root.Tag = x;
                    root.Name = x?.FileName ?? "_";

                    return root;
                });*/

            /*
            Root = Observable
                .Select<Archive, TreeViewItem>(Archive, x =>
                {
                    if (x == null)
                        return null;

                    TreeViewItem root = new TreeViewItem();
                    root.Header = Path.GetFileNameWithoutExtension(x.FileName);
                    root.Tag = x;
                    root.Name = "_";
                    //TreeView_Archive.RegisterName("_", root);

                    return root;
                });*/
        }

        private static void ProcessDirectories(IList<ArkEntry> entries, string currentPath, TreeViewItem currentNode)
        {
            /*
            var directories = entries
                        .Select(x => (x.Directory ?? "")
                        .ToLower())
                        .Distinct()
                        .SelectMany(w =>
                        {
                            var subDirs = w
                                .Split('/')
                                .ToList();

                            return Enumerable
                                .Range(1, subDirs.Count)
                                .Select(s => string.Join('/', subDirs.Take(s)))
                                .ToList();
                        })
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();*/


            var immediateDirs = entries
                .Where(x => !x.Directory.Equals(currentPath, StringComparison.CurrentCultureIgnoreCase))
                .Select(x => GetTopDirectory(x.Directory.Substring(currentPath.Length + (currentPath.Length > 0 ? 1 : 0))))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var immediateEntries = entries?
                .Where(x => x.Directory.Equals(currentPath, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x => x.FullPath)
                .ToList();

            var subItems = new List<TreeViewItem>();

            
            foreach (var dir in immediateDirs)
            {
                var subDir = currentPath.Length > 0 ? $"{currentPath}/{dir}" : dir;

                var subEntries = entries
                    .Where(x => x.Directory.StartsWith(subDir, StringComparison.CurrentCultureIgnoreCase))
                    .OrderBy(x => x.FullPath)
                    .ToList();

                var subNode = new TreeViewItem();
                subNode.Header = dir;
                subNode.Classes.Add("directory");

                //subNode.Tag = ark;
                //subNode.Name = "_";
                subItems.Add(subNode);

                ProcessDirectories(subEntries, subDir, subNode);
            }

            foreach (var entry in immediateEntries)
            {
                var subNode = new TreeViewItem();
                subNode.Header = entry.FileName;
                subNode.Classes.Add("file");

                //subNode.Tag = ark;
                //subNode.Name = "_";
                subItems.Add(subNode);
            }

            currentNode.Items = subItems;
        }

        private static string GetTopDirectory(string path)
        {
            if (path == null)
            {
                return null;
            }
            else if (path == "")
            {
                return "";
            }

            return path.Contains('/') ? path.Split('/').First() : path;
        }

        public Archive Archive
        {
            get => _ark;
            set => this.RaiseAndSetIfChanged(ref _ark, value);
        }
        
        //public IObservable<TreeViewItem> RootObservable { get; }
        public TreeViewItem Root { get; private set; }
    }
}
