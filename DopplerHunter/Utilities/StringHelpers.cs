using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Utilities
{
    public static class FileNameFormatter
    {
        public static string Shorten(string fileName, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Length <= maxLength)
                return fileName;

            return $"{fileName[..23]}...{fileName[^23..]}";
        }
    }
}
