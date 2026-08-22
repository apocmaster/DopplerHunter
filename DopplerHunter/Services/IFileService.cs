using DopplerHunter.Events;
using DopplerHunter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DopplerHunter.Services
{
    public interface IFileService
    {
        event EventHandler<HashesCalculatedEventArgs>? HashesCalculated;

        bool IsFileExists(string filePath);
        bool IsFileNotExists(string filePath);
        Task<string> OpenFileAsync(string filePath);
        Task<string> ComputeMD5(string filePath);
        Task<string> ComputeXXHash(string filePath);
        Task CalculatePossibleDuplicates(ObservableCollection<FileMetadata> files);
        Task MarkFilesDuplicates(ObservableCollection<FileMetadata> files);
        Task GroupFilesDuplicated(ObservableCollection<FileMetadata> files);
    }
}
