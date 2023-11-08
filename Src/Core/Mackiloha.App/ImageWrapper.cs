using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mackiloha.App;

public readonly record struct RGBAColor
{
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }
    public byte A { get; init; }
}

public interface IImageWrapper
{
    public int Width { get; }
    public int Height { get; }

    public void WriteToFile(string filePath);
}

public class ImageWrapper : IImageWrapper, IDisposable
{
    protected readonly Image<Rgba32> _image;

    private ImageWrapper(Image<Rgba32> image)
    {
        _image = image;
    }

    public ImageWrapper(Stream stream)
    {
        var image = Image.Load<Rgba32>(stream);
        _image = image;
    }

    public ImageWrapper(string filePath)
    {
        var image = Image.Load<Rgba32>(filePath);
        _image = image;
    }

    public static ImageWrapper FromRGBA(byte[] data, int width, int height)
    {
        var image = Image.LoadPixelData<Rgba32>(data, width, height);
        return new ImageWrapper(image);
    }

    public int Width => _image.Width;
    public int Height => _image.Height;

    public void Dispose()
    {
        _image?.Dispose();
    }

    public int TotalColors()
    {
        int totalCount = 0;

        // Count unique pixels
        _image.ProcessPixelRows(accessor =>
        {
            var pixels = new HashSet<uint>();

            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    pixels.Add(row[x].Rgba);
                }
            }

            totalCount = pixels.Count;
        });

        return totalCount;
    }

    public List<RGBAColor> GetUniqueColors()
    {
        var pixels = new HashSet<uint>();

        // Get unique pixels
        _image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    pixels.Add(row[x].Rgba);
                }
            }
        });

        return pixels
            .Select(x => new RGBAColor()
            {
                R = (byte)((x & 0x00_00_00_FF)),
                G = (byte)((x & 0x00_00_FF_00) >> 8),
                B = (byte)((x & 0x00_FF_00_00) >> 16),
                A = (byte)((x & 0xFF_00_00_00) >> 24),
            })
            .ToList();
    }

    public List<RGBAColor> GetPixels()
    {
        var pixels = new List<RGBAColor>();

        _image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];

                    pixels.Add(new RGBAColor()
                    {
                        R = pixel.R,
                        G = pixel.G,
                        B = pixel.B,
                        A = pixel.A
                    });
                }
            }
        });

        return pixels;
    }

    public void WriteToFile(string filePath)
    {
        _image.Save(filePath);
    }

    public byte[] AsRGBA()
    {
        var data = new byte[_image.Width * _image.Height * 4];

        _image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    var i = ((y * _image.Width) + x) * 4;
                    var pixel = row[x];

                    data[i    ] = pixel.R;
                    data[i + 1] = pixel.G;
                    data[i + 2] = pixel.B;
                    data[i + 3] = pixel.A;
                }
            }
        });

        return data;
    }

    public byte[] AsDXT1()
    {
        BcEncoder encoder = new BcEncoder();
        encoder.OutputOptions.GenerateMipMaps = false;
        encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        encoder.OutputOptions.Format = BCnEncoder.Shared.CompressionFormat.Bc1;
        encoder.OutputOptions.FileFormat = BCnEncoder.Shared.OutputFileFormat.Dds;

        using var ms = new MemoryStream();
        encoder.EncodeToStream(_image, ms);

        // Copy to array (definitely not the most efficient...)
        var data = new byte[(_image.Width * _image.Height) >> 1];
        ms.Seek(128, SeekOrigin.Begin);
        ms.Read(data, 0, data.Length);

        return data;
    }

    public byte[] AsDXT5()
    {
        BcEncoder encoder = new BcEncoder();
        encoder.OutputOptions.GenerateMipMaps = false;
        encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
        encoder.OutputOptions.Format = BCnEncoder.Shared.CompressionFormat.Bc3;
        encoder.OutputOptions.FileFormat = BCnEncoder.Shared.OutputFileFormat.Dds;

        using var ms = new MemoryStream();
        encoder.EncodeToStream(_image, ms);

        // Copy to array (definitely not the most efficient...)
        var data = new byte[_image.Width * _image.Height];
        ms.Seek(128, SeekOrigin.Begin);
        ms.Read(data, 0, data.Length);

        return data;
    }
}
