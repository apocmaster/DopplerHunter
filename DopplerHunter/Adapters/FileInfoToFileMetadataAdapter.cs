using DopplerHunter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DopplerHunter.Adapters
{
    public class FileInfoToFileMetadataAdapter
    {
        public static List<FileMetadata> Convert(List<FileInfo> files)
        {
            var result = files.Select(file => new FileMetadata
            {
                FullPath = file.FullName,
                FileName = Path.GetFileNameWithoutExtension(file.Name),
                FileSize = file.Length,
                LastModified = file.LastWriteTime,
                IsHashCalculated = false,
                FolderPath = Path.GetFileName(file.DirectoryName) ?? string.Empty,
                Extension = Path.GetExtension(file.FullName) ?? string.Empty
            }).ToList();

            return result;
        }
    }
}
