﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha
{
    // Just in case I port this to other platforms and something breaks
    public static class FileHelper
    {
        public static string GetDirectory(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }

        public static string ReplaceExtension(string filePath, string extension)
        {
            return Path.ChangeExtension(filePath, extension);
        }

        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public static string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public static void CreateDirectoryIfNotExists(string filePath)
        {
            string directory = GetDirectory(filePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }

        public static string RemoveExtension(string filePath)
        {
            return $@"{GetDirectory(filePath)}\{GetFileNameWithoutExtension(filePath)}";
        }
    }
}