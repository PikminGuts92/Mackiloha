using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

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

            this.FindControl<MenuItem>("Menu_File_Open").Click += Menu_File_Open_Click;
            this.FindControl<MenuItem>("Menu_File_Exit").Click += Menu_File_Exit_Click;
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
        }

        private void Menu_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}