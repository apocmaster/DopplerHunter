using DopplerHunter.Events;
using DopplerHunter.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DopplerHunter.Services
{
    public interface IDirectoryService
    {
        event EventHandler<DirectoryAnalizedEventArgs>? DirectoryAnalized;
        event EventHandler<FilesExtractionCompletedEventArgs>? FilesExtractionCompleted;

        Task<List<FileInfo>> GetFilesInDirectories(List<DirectoryInfo> directories);
        Task<List<FileInfo>> GetFilesInDirectory(DirectoryInfo directory);
        Task<List<DirectoryInfo>> ScanDirectoriesAndSubdirectories(List<FileSystemItemViewModel> directoriesSelected);
        Task ScanSubdirectories(DirectoryInfo directory, bool includeSubdirectories, List<DirectoryInfo> result);
    }
}
