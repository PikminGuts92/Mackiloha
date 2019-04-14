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
                    root.Tag = ark;
                    root.Name = "_";
                    this.Root = root;

                    if (ark == null)
                        return;

                    var directories = ark.Entries
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
                        .ToList();

                    ProcessDirectories(directories, ark.Entries, "", this.Root);
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

        private static void ProcessDirectories(IList<string> subDirectories, IList<ArkEntry> entries, string currentPath, TreeViewItem currentNode)
        {
            var immediateDirs = subDirectories
                .Select(x => x.Contains('/') ? x.Substring(0, x.IndexOf('/')) : x)
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
                var subDirs = subDirectories
                    .Where(x => x.StartsWith($"{subDir}/", StringComparison.CurrentCultureIgnoreCase))
                    .Select(x => x.Remove(0, x.IndexOf('/') + 1))
                    .OrderBy(x => x)
                    .ToList();

                var subEntries = entries
                    .Where(x => x.Directory.Equals(subDir, StringComparison.CurrentCultureIgnoreCase))
                    .OrderBy(x => x.FullPath)
                    .ToList();

                var subNode = new TreeViewItem();
                subNode.Header = dir;
                //subNode.Tag = ark;
                //subNode.Name = "_";
                subItems.Add(subNode);

                ProcessDirectories(subDirs, subEntries, subDir, subNode);
            }

            foreach (var entry in immediateEntries)
            {

            }

            currentNode.Items = subItems;
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
