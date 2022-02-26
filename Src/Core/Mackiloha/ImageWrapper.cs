using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Mackiloha
{
    public interface IImageWrapper
    {
        public int Width { get; }
        public int Height { get; }

        public void WriteToFile(string filePath);
    }

    public class ImageWrapper : IImageWrapper, IDisposable
    {
        protected readonly Image<Rgba32> _image;

        public ImageWrapper(Stream stream)
        {
            var image = Image.Load<Rgba32>(stream);
            _image = image;
        }

        public int Width => _image.Width;
        public int Height => _image.Height;

        public void Dispose()
        {
            _image?.Dispose();
        }

        public int TotalColors()
        {
            _image.ProcessPixelRows(accessor =>
            {
                
            });
            return 0;
        }

        public void WriteToFile(string filePath)
        {
            _image.Save(filePath);
        }
    }
}
