using System;
using System.Collections.Generic;
using System.Drawing;
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
using Mackiloha;
using System.IO;

namespace SuperFreq
{
    /// <summary>
    /// Interaction logic for TextureEditor.xaml
    /// </summary>
    public partial class TextureEditor : UserControl
    {
        private HMXImage _imageFile;

        public TextureEditor()
        {
            InitializeComponent();
        }

        public void OpenImageFile(Stream source)
        {
            if (source == null) return;

            _imageFile = HMXImage.FromStream(source);
            Image_Texture.Source = BitmapToImageSource(_imageFile.Image.ToBitmap());
        }
        
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
