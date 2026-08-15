using System;
using System.Collections.Generic;
using System.Text;

namespace DopplerHunter.Services
{
    public interface IFileService
    {
        
        bool IsFileExists(string filePath);
        bool IsFileNotExists(string filePath);
        Task<string> OpenFileAsync(string filePath);
        Task<string> ComputeMD5(string filePath);
        Task<string> ComputeXXHash(string filePath);
    }
}
