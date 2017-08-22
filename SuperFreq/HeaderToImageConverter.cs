using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using static System.IO.Path;
using Mackiloha.Ark;

// What a nice guy! :)
// http://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree 

namespace SuperFreq
{
    #region HeaderToImageConverter

    [ValueConversion(typeof(string), typeof(bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = "pack://application:,,,/Assets/";

            if (value is ArkFile)
            {
                // Root
                path += "box.png";
            }
            else if (value is TreeArkEntryInfo)
            {
                // Directory/file entry
                TreeArkEntryInfo fileInfo = value as TreeArkEntryInfo;

                switch (fileInfo.EntryType)
                {
                    case ArkEntryType.Folder:
                        path += "folder_closed.png";
                        break;
                    case ArkEntryType.Script:
                        path += "page_white_code_green.png";
                        break;
                    case ArkEntryType.Texture:
                        path += "image.png";
                        break;
                    case ArkEntryType.Audio:
                        path += "music.png";
                        break;
                    case ArkEntryType.Archive:
                        path += "bricks.png";
                        break;
                    case ArkEntryType.Video:
                        path += "film.png";
                        break;
                    case ArkEntryType.Midi:
                        path += "page_white_music.png";
                        break;
                    default: // ArkFileType.Default
                        path += "page_white.png";
                        break;
                }
            }
            else
            {
                path += "page_white.png";
            }

            // Returns assigned icon
            return new BitmapImage(new Uri(path));
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    #endregion // HeaderToImageConverter
}
