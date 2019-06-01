using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SuperFreq.ViewModels;
using Mackiloha.Ark;

namespace SuperFreq.Views
{
    public class MainWindow : Window
    {
        private readonly OpenFileDialog OFD = new OpenFileDialog();
        private readonly string OrigTitle;

        public MainWindow()
        {
            InitializeComponent();
            OrigTitle = this.Title;
            
            this.AttachDevTools();

            this.FindControl<MenuItem>("Menu_File_Open").Click += Menu_File_Open_Click;
            this.FindControl<MenuItem>("Menu_File_Exit").Click += Menu_File_Exit_Click;

            //this.FindControl<TreeView>("TreeView_Archive").item
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Menu_File_Open_Click(object sender, RoutedEventArgs e)
        {
            OFD.AllowMultiple = false;
            var results = await OFD.ShowAsync(this);

            if (results.Length <= 0)
                return;

            this.Title = $"{OrigTitle} - {System.IO.Path.GetFileName(results.First())}";

            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel.Archive = ArkFile.FromFile(results.First());

            var treeView = this.FindControl<TreeView>("TreeView_Archive");

            //treeView.DataContext = viewModel.Root;
            treeView.Items = new [] { viewModel.Root };
        }

        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}