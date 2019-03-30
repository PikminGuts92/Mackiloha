using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
//using System.Linq;
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
                    root.Header = Path.GetFileNameWithoutExtension(ark.FileName);
                    root.Tag = ark;
                    root.Name = "_";
                    this.Root = root;

                    if (ark == null)
                        return;

                    

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

        public Archive Archive
        {
            get => _ark;
            set => this.RaiseAndSetIfChanged(ref _ark, value);
        }
        
        //public IObservable<TreeViewItem> RootObservable { get; }
        public TreeViewItem Root { get; private set; }
    }
}
